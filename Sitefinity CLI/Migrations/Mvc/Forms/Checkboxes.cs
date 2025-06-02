using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class Checkboxes : MultipleChoice
{
    public override string FieldType => "Checkboxes";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties;

        var migratedProperties = ProcessProperties(context.Source.Properties, null, propertiesToRename);
        ConfigureChoices(propsToRead, migratedProperties);

        return await Task.FromResult(new MigratedWidget("SitefinityCheckboxes", migratedProperties));
    }
}
