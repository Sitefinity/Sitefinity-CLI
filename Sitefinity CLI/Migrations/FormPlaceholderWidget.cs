using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;
internal class FormPlaceholderWidget : IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var message = $"Failed to find a mapping or custom migration for widget -> {context.Source.Caption ?? context.Source.Name}. Using default Placeholder migrated widget.";
        await context.LogWarning(message);

        return new MigratedWidget("SitefinityFormContentBlock", new Dictionary<string, string>
        {
            ["Content"] = $"<h4 style=\"color:orange;\">{message}</h4>"
        });
    }
}
