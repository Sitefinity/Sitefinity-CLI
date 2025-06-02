using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class Paragraph : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Model-CssClass", "CssClass" },
        { "MetaField-Title", "Label" },
        { "MetaField-Description", "InstructionalText" },
        { "MetaField-DefaultValue", "PredefinedValue" },
        { "Model-PlaceholderText", "PlaceholderText" },
        { "Model-Hidden", "Hidden" },
        { "Model-ValidatorDefinition-Required", "Required" },
        { "Model-ValidatorDefinition-RequiredViolationMessage", "RequiredErrorMessage" },
    };
    public override string FieldType => "Paragraph";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, null, propertiesToRename);
        var propsToRead = context.Source.Properties;

        ConfigureTextField(propsToRead, migratedProperties);

        return Task.FromResult(new MigratedWidget("SitefinityParagraph", migratedProperties));
    }

    protected static void ConfigureTextField(IDictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        int? minLength = null;
        int? maxLength = null;
        if (propsToRead.TryGetValue("Model-ValidatorDefinition-MinLength", out string minLengthStr) && int.TryParse(minLengthStr, out int min))
        {
            minLength = min;
        }

        if (propsToRead.TryGetValue("Model-ValidatorDefinition-MaxLength", out string maxLengthStr) && int.TryParse(maxLengthStr, out int max))
        {
            maxLength = max == 0 ? 255 : max;
        }

        if (minLength.HasValue || maxLength.HasValue)
        {
            migratedProperties.Add("Range", JsonSerializer.Serialize(new
            {
                Min = minLength,
                Max = maxLength
            }));
        }

        if (propsToRead.TryGetValue("Model-ValidatorDefinition-MaxLengthViolationMessage", out string maxLengthViolationMessage))
        {
            // The following format with 2 or more placeholders is not supported: {0} field is too long. Maximum length is {1} characters
            // Only 1 placeholder is, so we need to replace it in order not to get an error
            maxLengthViolationMessage = Regex.Replace(maxLengthViolationMessage, "\\{[1-9]\\}", maxLengthStr ?? "");
            migratedProperties.Add("TextLengthViolationMessage", maxLengthViolationMessage);
        }
    }
}
