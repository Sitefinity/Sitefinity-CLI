using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;
internal abstract class FormMigrationBase : MigrationBase, IWidgetMigration
{
    public abstract string FieldType { get; }

    public abstract Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context);

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedWidget = await MigrateFormWidget(context);
        if (migratedWidget != null)
        {
            if (!migratedWidget.MigratedProperties.TryGetValue("SfFieldType", out string fieldType))
            {
                migratedWidget.MigratedProperties.Add("SfFieldType", this.FieldType);
            }

            if (!migratedWidget.MigratedProperties.TryGetValue("SfFieldName", out _))
            {
                if (context.Source.Properties.TryGetValue("MetaField-FieldName", out string fieldName))
                {
                    migratedWidget.MigratedProperties.Add("SfFieldName", fieldName);
                }
            }
        }

        return migratedWidget;
    }
}
