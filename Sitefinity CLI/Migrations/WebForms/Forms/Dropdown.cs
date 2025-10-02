using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class Dropdown : MultipleChoice
{
    public override string FieldType => "Dropdown";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        ConfigureChoices(context, migratedProperties);
        ConfigureDropDownSize(context, migratedProperties);

        return Task.FromResult(new MigratedWidget("SitefinityDropdown", migratedProperties));
    }

    protected static void ConfigureDropDownSize(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        if (context.Source.Properties.TryGetValue("DropDownListSize", out string size))
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
