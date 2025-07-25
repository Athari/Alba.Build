﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TextTemplating;
using VsDte = EnvDTE.DTE;
using VsProjectItem = EnvDTE.ProjectItem;
using VsProperties = EnvDTE.Properties;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable UseCollectionExpression
#pragma warning disable IDE0028 // Simplify collection initialization

namespace TmpCodeGen;

[PublicAPI]
internal class T4Manager : IDisposable
{
    private class Section
    {
        public string Name { get; set; } = "";
        public int Start { get; set; }
        public int Length { get; set; }

        public string ToString(StringBuilder template) =>
            template.ToString(Start, Length);
    }

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

    private readonly ITextTemplatingEngineHost _host;
    private readonly StringBuilder _template;
    private readonly Section _header = new();
    private readonly Section _footer = new();
    private readonly List<Section> _files = new();
    protected readonly List<string> _fileNames = new();
    private Section? _currentSection;

    public bool PrefixFileNames { get; set; } = true;
    public bool SyncProject { get; set; } = false;
    public bool MultipleFiles { get; set; } = true;
    public bool TrimText { get; set; } = true;

    protected T4Manager(ITextTemplatingEngineHost host, StringBuilder template)
    {
        _host = host;
        _template = template;
    }

    public static T4Manager Create(ITextTemplatingEngineHost host, StringBuilder template) =>
        host is IServiceProvider ? new VsT4Manager(host, template) : new T4Manager(host, template);

    // Hints

    private T GetContextData<T>(string name) => (T)CallContext.LogicalGetData(name);
    private string GetContextData(string name) => GetContextData<string>(name);

    public virtual string? ProjectNamespace => null;
    public string NamespaceHint => GetContextData("NamespaceHint");
    public string Namespace => GetCustomNamespaceFor(_host.TemplateFile) ?? NamespaceHint;

    public virtual string? GetCustomNamespaceFor(string fileName) => null;
    public string NamespaceFor(string fileName) => GetCustomNamespaceFor(fileName) ?? NamespaceHint;

    public string ClassName {
        get {
            var name = _host.TemplateFile;
            var iDot = name.IndexOf('.');
            return iDot == -1 ? name : name.Substring(0, iDot);
        }
    }

    // Loading data

    public string ResolvePath(string? fileName = null, string? ext = null) =>
        _host.ResolvePath(fileName ?? Path.ChangeExtension(_host.TemplateFile, ext));

    public string LoadText(string? fileName = null, string? ext = "txt") =>
        File.ReadAllText(ResolvePath(fileName, ext), Utf8);

    public XDocument LoadXml(string? fileName = null, string ext = "xml") =>
        XDocument.Load(ResolvePath(fileName, ext));

    // Multiple file output

    private void StartSection(Section newSection)
    {
        if (_currentSection != null)
            EndSection();
        _currentSection = newSection;
        _currentSection.Start = _template.Length;
    }

    private void EndSection()
    {
        if (_currentSection == null)
            return;
        _currentSection.Length = _template.Length - _currentSection.Start;
        _currentSection = null;
    }

    public void WriteFile(string name, Action generate)
    {
        var section = new Section { Name = name ?? throw new ArgumentNullException(nameof(name)) };
        _files.Add(section);
        StartSection(section);
        generate();
    }

    public void WriteFooter(Action generate)
    {
        StartSection(_footer);
        generate();
    }

    public void WriteHeader(Action generate)
    {
        StartSection(_header);
        generate();
    }

    public virtual void Process()
    {
        if (!MultipleFiles)
            return;

        EndSection();
        string fileNamePrefix = PrefixFileNames ? $"{ClassName}." : "";
        string headerText = _header.ToString(_template);
        string footerText = _footer.ToString(_template);
        string outputPath = Path.GetDirectoryName(_host.TemplateFile)!;
        foreach (var section in _files.AsEnumerable().Reverse()) {
            string fileName = Path.Combine(outputPath, fileNamePrefix + section.Name);
            string text = $"{headerText}{section.ToString(_template)}{footerText}";
            if (TrimText)
                text = text.Trim();
            _fileNames.Add(fileName);
            if (!File.Exists(fileName) || File.ReadAllText(fileName, Utf8) != text)
                File.WriteAllText(fileName, text, Utf8);
        }
    }

    public void Dispose() => Process();
}

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Not my problem")]
internal class VsT4Manager : T4Manager
{
    private readonly VsProjectItem _t4ProjectItem;
    private readonly VsDte _dte;

    public override string ProjectNamespace =>
        GetString(_t4ProjectItem.ContainingProject.Properties, "DefaultNamespace");

    public override string GetCustomNamespaceFor(string fileName) =>
        GetString(_dte.Solution.FindProjectItem(fileName).Properties, "CustomToolNamespace");

    public override void Process()
    {
        base.Process();
        if (_t4ProjectItem.ProjectItems != null)
            ProjectSync(_fileNames);
    }

    internal VsT4Manager(ITextTemplatingEngineHost host, StringBuilder template) : base(host, template)
    {
        _dte = (host as IServiceProvider)?.GetService(typeof(VsDte)) as VsDte ??
            throw new ArgumentException("Could not obtain DTE from the host.");
        _t4ProjectItem = _dte.Solution.FindProjectItem(host.TemplateFile);
    }

    private void ProjectSync(IList<string> fileNames)
    {
        if (!SyncProject)
            return;

        var keepFileNames = fileNames.ToHashSet();
        var originalFilePrefix = Path.GetFileNameWithoutExtension(_t4ProjectItem.FileNames[0]) + ".";
        var projectFiles = _t4ProjectItem.ProjectItems.OfType<VsProjectItem>().ToDictionary(i => i.FileNames[0]);

        foreach (var pair in projectFiles)
            if (!fileNames.Contains(pair.Key) && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(originalFilePrefix))
                pair.Value.Delete();
        foreach (string fileName in keepFileNames)
            if (!projectFiles.ContainsKey(fileName))
                _t4ProjectItem.ProjectItems.AddFromFile(fileName);
    }

    private static string GetString(VsProperties props, string name) =>
        props.Item(name).Value.ToString();
}

[PublicAPI]
internal class _
{
    public static ITextTemplatingEngineHost Host = null!;
    public static StringBuilder GenerationEnvironment = null!;

    private T4Manager T4 { get; } = T4Manager.Create(Host, GenerationEnvironment);

    private string IfFormat(string format, object? value) =>
        value != null ? string.Format(format, value) : "";

    private string CamelCase(string str) =>
        string.IsNullOrEmpty(str) ? "" : char.ToLowerInvariant(str[0]) + str.Substring(1);
}