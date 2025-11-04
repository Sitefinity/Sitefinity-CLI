using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class MultipleChoice : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "MetaField-Title", "Label" },
        { "Example", "InstructionalText" },
        { "MetaField-DefaultValue", "PredefinedValue" },
        { "Model-HasOtherChoice", "HasAdditionalChoice" },
        { "ValidatorDefinition-RequiredViolationMessage", "RequiredErrorMessage" },
        { "ValidatorDefinition-Required", "Required" },
        { "Model-Hidden", "Hidden" },
        { "Model-CssClass", "CssClass" }
    };

    public override string FieldType => "MultipleChoice";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties;

        var migratedProperties = ProcessProperties(propsToRead, null, propertiesToRename);
        ConfigureChoices(propsToRead, migratedProperties);

        return await Task.FromResult(new MigratedWidget("SitefinityMultipleChoice", migratedProperties));
    }

    protected static void ConfigureChoices(IDictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        if (propsToRead.TryGetValue("Model-SerializedChoices", out string choiceTitles))
        {
            var choiceTitlesArray = JsonSerializer.Deserialize<List<string>>(choiceTitles);
            var choices = new List<object>();

            propsToRead.TryGetValue("MetaField-DefaultValue", out string selectedTitleString);
            var selectedTitlesArray = !string.IsNullOrEmpty(selectedTitleString) ? selectedTitleString.Trim(';').Split(';').ToList() : new List<string>();

            foreach (var choiceTitle in choiceTitlesArray)
            {
                if (!string.IsNullOrEmpty(choiceTitle))
                {
                    choices.Add(new { Name = choiceTitle, Value = choiceTitle, Selected = selectedTitlesArray.Contains(choiceTitle) });
                }
            }
            migratedProperties.Add("Choices", JsonSerializer.Serialize(choices));
        }
    }
}
