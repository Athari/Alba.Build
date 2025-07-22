using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using Alba.Build.PowerShell.Common;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Alba.Build.PowerShell.UI.Wpf.Prompts;
using Alba.Build.PowerShell.UI.Wpf.Prompts.Choice;
using Alba.Build.PowerShell.UI.Wpf.Prompts.Fields;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Main;

internal partial class MainModel : ModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = "MSBuild PowerShell UI";

    [ObservableProperty]
    public partial PromptModelBase? Prompt { get; set; }

    public Task<Result<string?>> ReadLine(string? prompt) =>
        DoPrompt(new ReadLineModel {
            Caption = prompt ?? "Please enter text",
        });

    public Task<Result<SecureString?>> SecureReadLine(string? prompt) =>
        DoPrompt(new SecureReadLineModel {
            Caption = prompt ?? "Please enter text",
        });

    public Task<Result<IDictionary<string, object?>>> PromptFields(
        string caption, string message, IList<FieldDescription> descriptions) =>
        DoPrompt(new PromptFieldsModel {
            Caption = caption.NullIfEmpty(),
            Message = message.NullIfEmpty(),
        }.With(p =>
            p.Fields.AddRange(descriptions.Select(FieldModel.Create))));

    public Task<Result<int>> PromptChoiceSingle(
        string caption, string message, IList<ChoiceDescription> choices, int defaultChoice) =>
        DoPrompt<int>(new PromptSingleChoiceModel {
            Caption = caption.NullIfEmpty(),
            Message = message.NullIfEmpty(),
        }.With(p =>
            p.Choices.AddRange(choices.Select((c, i) => new ChoiceModel {
                Label = c.Label.NullIfEmpty(),
                HelpMessage = c.HelpMessage.NullIfEmpty(),
                IsSelected = i == defaultChoice,
            }))));

    public Task<Result<IList<int>>> PromptChoiceMultiple(
        string caption, string message, IList<ChoiceDescription> choices, IList<int> defaultChoices) =>
        DoPrompt(new PromptMultipleChoiceModel {
            Caption = caption.NullIfEmpty(),
            Message = message.NullIfEmpty(),
        }.With(p =>
            p.Choices.AddRange(choices.Select((c, i) => new ChoiceModel {
                Label = c.Label.NullIfEmpty(),
                HelpMessage = c.HelpMessage.NullIfEmpty(),
                IsSelected = defaultChoices.Contains(i),
            }))));

    public Task<Result<PSCredential?>> PromptCredential(
        string caption, string message, string? userName, string? targetName,
        PSCredentialTypes allowedTypes, PSCredentialUIOptions options) =>
        DoPrompt(new PromptCredentialModel {
            Caption = caption.NullIfEmpty(),
            Message = message.NullIfEmpty(),
            UserName = userName ?? "",
            TargetName = targetName.NullIfEmpty(),
            AllowedTypes = allowedTypes,
            Options = options,
        });

    private Task<Result<T>> DoPrompt<T>(IPromptModel<T> prompt)
    {
        Prompt = (PromptModelBase)prompt;
        return prompt.GetResultAsync();
    }
}