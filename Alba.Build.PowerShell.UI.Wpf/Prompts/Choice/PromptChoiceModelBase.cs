using System.Collections.ObjectModel;
using System.Ling;
using Alba.Build.PowerShell.Common;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts.Choice;

internal abstract partial class PromptChoiceModelBase : PromptModelBase<IList<int>>
{
    public ObservableCollection<ChoiceModel> Choices { get; } = [ ];

    public override void OnOk() => Success([ ..Choices.Where(c => c.IsSelected).Select(Choices.IndexOf) ]);
}

internal partial class PromptSingleChoiceModel : PromptChoiceModelBase, IPromptModel<int>
{
    async Task<Result<int>> IPromptModel<int>.GetResultAsync() =>
        Result.Success((await base.GetResultAsync()).GetValueOrThrow().FirstOrDefault(-1));
}

internal partial class PromptMultipleChoiceModel : PromptChoiceModelBase { }