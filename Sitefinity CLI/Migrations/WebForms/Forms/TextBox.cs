using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class TextBox : FormMigrationBase
{
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Title", "Label" },
        { "Example", "InstructionalText" },
        { "DefaultValue", "PredefinedValue" },
        { "ValidatorDefinition-MaxLengthViolationMessage", "TextLengthViolationMessage" }
    };

    private static readonly string[] propertiesToCopy = new string[] { "CssClass" };

    public override string FieldType => "ShortText";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);

        context.Source.Properties.TryGetValue("ValidatorDefinition-MaxLength", out string maxLength);
        context.Source.Properties.TryGetValue("ValidatorDefinition-MinLength", out string minLength);

        if (maxLength != "0" || minLength != "0")
        {
            int.TryParse(minLength, CultureInfo.InvariantCulture, out int min);
            int.TryParse(maxLength, CultureInfo.InvariantCulture, out int max);

            migratedProperties.Add("Range", JsonSerializer.Serialize(new
            {
                Min = min,
                Max = max
            }));
        }

        if (context.Source.Properties.TryGetValue("TextBoxSize", out string size))
        {
            switch (size)
            {
                case "Small":
                    migratedProperties.Add("FieldSize", "S");
                    break;
                case "Medium":
                    migratedProperties.Add("FieldSize", "M");
                    break;
                case "Large":
                    migratedProperties.Add("FieldSize", "L");
                    break;
                default:
                    migratedProperties.Add("FieldSize", "None");
                    break;
            }
        }

        return Task.FromResult(new MigratedWidget("SitefinityTextField", migratedProperties));
    }
}
