using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Alba.Build.PowerShell;

internal static class Env
{
    [MaybeNull]
    public static string CurrentDirectory
    {
        get => Directory.GetCurrentDirectory();
        set => Directory.SetCurrentDirectory(value);
    }

    [Conditional("DEBUG")]
    public static void LaunchDebugger()
    {
        if (!Debugger.IsAttached)
            Debugger.Launch();
    }

    public static CurrentDirectoryScope WithCurrentDirectory(string? dir) => new(dir);

    internal readonly struct CurrentDirectoryScope : IDisposable
    {
        private readonly string? _prevDir;

        internal CurrentDirectoryScope(string? newDir)
        {
            if (newDir != null)
                (_prevDir, CurrentDirectory) = (CurrentDirectory, newDir);
        }

        public void Dispose()
        {
            if (_prevDir != null)
                CurrentDirectory = _prevDir;
        }
    }
}