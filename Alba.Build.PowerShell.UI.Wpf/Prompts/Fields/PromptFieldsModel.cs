using System.Collections.ObjectModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts.Fields;

internal partial class PromptFieldsModel : PromptModelBase<IDictionary<string, object?>>
{
    public ObservableCollection<FieldModel> Fields { get; } = [ ];

    public override void OnOk() => Success(Fields.ToDictionary(f => f.Name, f => f.Value));
}