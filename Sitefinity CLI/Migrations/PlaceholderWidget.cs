using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;
internal class PlaceholderWidget : IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        await context.LogWarning($"Failed to find a mapping or custom migration for widget -> {context.Source.Name}. Using default Placeholder migrated widget.");
        return new MigratedWidget("Placeholder", context.Source.Properties);
    }
}
