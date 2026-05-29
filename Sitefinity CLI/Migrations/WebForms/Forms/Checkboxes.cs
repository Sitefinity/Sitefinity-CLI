using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class Checkboxes : MultipleChoice
{
    public override string FieldType => "Checkboxes";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        ConfigureColumnsNumber(context, migratedProperties);
        ConfigureChoices(context, migratedProperties);

        return await Task.FromResult(new MigratedWidget("SitefinityCheckboxes", migratedProperties));
    }
}
