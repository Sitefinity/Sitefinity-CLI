using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class TextBox : Paragraph
{
    public override string FieldType => "ShortText";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, null, propertiesToRename);
        var propsToRead = context.Source.Properties;

        ConfigureTextField(propsToRead, migratedProperties);

        if (context.Source.Properties.TryGetValue("Model-InputType", out string inputType))
        {
            var resultInputType = "Text";
            switch (inputType.ToLowerInvariant())
            {
                case "text":
                    resultInputType = "Text";
                    break;
                case "email":
                    resultInputType = "Email";
                    break;
                case "tel":
                    resultInputType = "Phone";
                    break;
                case "url":
                    resultInputType = "Url";
                    break;
                default:
                    resultInputType = "Text";
                    await context.LogWarning($"InputType '{inputType}' is not supported. Defaulting to 'Text'.");
                    break;
            }

            migratedProperties.Add("InputType", resultInputType);
        }

        return new MigratedWidget("SitefinityTextField", migratedProperties);
    }
}
