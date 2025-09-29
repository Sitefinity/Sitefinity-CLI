using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class SubmitButton : FormMigrationBase
{
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Model-Label", "Label" },
        { "Model-CssClass", "CssClass" },
    };
    public override string FieldType => "SubmitButton";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, null, propertiesToRename);
        return Task.FromResult(new MigratedWidget("SitefinitySubmitButton", migratedProperties));
    }
}
