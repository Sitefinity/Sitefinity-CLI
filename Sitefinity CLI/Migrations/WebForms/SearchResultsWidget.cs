using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class SearchResultsWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass", "SearchFields", "HighlightedFields"];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("AllowPaging", out string allowPaging))
        {
            string displayMode;
            if (bool.TryParse(allowPaging, out bool allowPagingParsed) && allowPagingParsed)
            {
                displayMode = "Paging";
            }
            else
            {
                displayMode = "Limit";
            }

            context.Source.Properties.TryGetValue("ItemsPerPage", out string itemsPerPage);

            if (!int.TryParse(itemsPerPage, out int itemsPerPageParsed))
            {
                itemsPerPageParsed = 20;
            }

            var serializedPageValue = JsonSerializer.Serialize(new
            {
                ItemsPerPage = itemsPerPageParsed,
                LimitItemsCount = itemsPerPageParsed,
                DisplayMode = displayMode
            });

            migratedProperties.Add("ListSettings", serializedPageValue);
        }

        return Task.FromResult(new MigratedWidget("SitefinitySearchResults", migratedProperties));
    }
}
