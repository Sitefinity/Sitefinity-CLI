using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class MultipleChoice : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Title", "Label" },
        { "Example", "InstructionalText" },
        { "DefaultValue", "PredefinedValue" },
        { "EnableAddOther", "HasAdditionalChoice" },
        { "ValidatorDefinition-RequiredViolationMessage", "RequiredErrorMessage" },
        { "ValidatorDefinition-Required", "Required" }
    };

    protected static readonly string[] propertiesToCopy = new string[] { "CssClass" };

    private Dictionary<string, int> ColumnsModeMap = new Dictionary<string, int>()
    {
        { "OneColumn", 1 },
        { "TwoColumns", 2 },
        { "ThreeColumns", 3 },
        { "Inline", 0 }
    };

    public override string FieldType => "MultipleChoice";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        ConfigureColumnsNumber(context, migratedProperties);
        ConfigureChoices(context, migratedProperties);

        if (context.Source.Properties.TryGetValue("OtherTitleText", out string otherTitleText) && !string.IsNullOrEmpty(otherTitleText) && otherTitleText != "Other")
        {
            await context.LogWarning("The label for 'Other' option now can be edited directly in the field view file.");
        }

        return await Task.FromResult(new MigratedWidget("SitefinityMultipleChoice", migratedProperties));
    }

    protected static void ConfigureChoices(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        var sortAlphabetically = context.Source.Properties.TryGetValue("SortAlphabetically", out string sortAlphabeticallyString) && sortAlphabeticallyString == "True";
        if (context.Source.Properties.TryGetValue("ChoiceItemsTitles", out string choiceTitles))
        {
            var choiceTitlesArray = choiceTitles.Split(';').ToList();
            var choices = new List<object>();
            if (sortAlphabetically)
            {
                choiceTitlesArray.Sort();
            }

            context.Source.Properties.TryGetValue("DefaultSelectedTitle", out string selectedTitle);

            foreach (var choiceTitle in choiceTitlesArray)
            {
                if (!string.IsNullOrEmpty(choiceTitle))
                {
                    choices.Add(new { Name = choiceTitle, Value = choiceTitle, Selected = selectedTitle == choiceTitle });
                }
            }
            migratedProperties.Add("Choices", JsonSerializer.Serialize(choices));
        }
    }

    protected void ConfigureColumnsNumber(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        context.Source.Properties.TryGetValue("FormControlColumnsMode", out string columnsMode);
        int columns = 1;
        this.ColumnsModeMap.TryGetValue(columnsMode, out columns);
        migratedProperties.Add("ColumnsNumber", columns.ToString(CultureInfo.InvariantCulture));
    }
}
