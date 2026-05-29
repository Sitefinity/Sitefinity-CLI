using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class Dropdown : MultipleChoice
{
    public override string FieldType => "Dropdown";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties;

        var migratedProperties = ProcessProperties(propsToRead, null, propertiesToRename);
        ConfigureChoices(propsToRead, migratedProperties);

        return Task.FromResult(new MigratedWidget("SitefinityDropdown", migratedProperties));
    }
}

