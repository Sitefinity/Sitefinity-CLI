using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class SubmitButton : FormMigrationBase
{
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Text", "Label" },
    };
    private static readonly string[] propertiesToCopy = new string[]
    {
        "CssClass"
    };
    public override string FieldType => "SubmitButton";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        return Task.FromResult(new MigratedWidget("SitefinitySubmitButton", migratedProperties));
    }
}
