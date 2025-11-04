using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class TextBox : Paragraph
{
    protected override string SizePropertyName => "TextBoxSize";
    public override string FieldType => "ShortText";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        var propsToRead = context.Source.Properties;

        ConfigureTextField(propsToRead, migratedProperties);

        if(context.Source.Properties.TryGetValue("ValidatorDefinition-ExpectedFormat", out string expectedFormat) && expectedFormat == "EmailAddress")
        {
            migratedProperties["InputType"] = "Email";
        }

        return Task.FromResult(new MigratedWidget("SitefinityTextField", migratedProperties));
    }
}
