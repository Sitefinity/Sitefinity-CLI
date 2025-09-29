using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class Paragraph : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Title", "Label" },
        { "Example", "InstructionalText" },
        { "DefaultValue", "PredefinedValue" },
        { "ValidatorDefinition-MaxLengthViolationMessage", "TextLengthViolationMessage" },
        { "ValidatorDefinition-Required", "Required" },
        { "ValidatorDefinition-RequiredViolationMessage", "RequiredErrorMessage" }
    };

    protected static readonly string[] propertiesToCopy = new string[] { "CssClass" };

    protected virtual string SizePropertyName => "ParagraphTextBoxSize";
    public override string FieldType => "Paragraph";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        var propsToRead = context.Source.Properties;

        ConfigureTextField(propsToRead, migratedProperties);

        return Task.FromResult(new MigratedWidget("SitefinityParagraph", migratedProperties));
    }

    protected void ConfigureTextField(IDictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        propsToRead.TryGetValue("ValidatorDefinition-MaxLength", out string maxLength);
        propsToRead.TryGetValue("ValidatorDefinition-MinLength", out string minLength);

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

        if (propsToRead.TryGetValue(this.SizePropertyName, out string size))
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
    }
}
