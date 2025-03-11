using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class SearchResultsWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass", "SearchFields", "HighlightedFields"];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, null);

        propsToRead.TryGetValue("DisplayMode", out string displayModeParsed);
        propsToRead.TryGetValue("ItemsPerPage", out string itemsPerPageParsed);
        propsToRead.TryGetValue("LimitCount", out string limitCount);

        var serializedPageValue = JsonSerializer.Serialize(new
        {
            DisplayMode = displayModeParsed ?? "Paging",
            ItemsPerPage = itemsPerPageParsed ?? "20",
            LimitItemsCount = limitCount ?? "20"
        });

        migratedProperties.Add("ListSettings", serializedPageValue);

        if (propsToRead.TryGetValue("OrderBy", out string orderBy))
        {
            var sorting = "MostRelevantOnTop";
            switch (orderBy)
            {
                case "Newest":
                    sorting = "NewestFirst";
                    break;
                case "Oldest":
                    sorting = "OldestFirst";
                    break;
                default:
                    break;
            }

            migratedProperties.Add("Sorting", sorting);
        }

        migratedProperties["AllowUsersToSortResults"] = "1";
        if (propsToRead.TryGetValue("AllowSorting", out string allowSorting) && bool.TryParse(allowSorting, out bool allowSortingBool))
        {
            if (!allowSortingBool)
            {
                migratedProperties["AllowUsersToSortResults"] = "0";
            }
        }

        return Task.FromResult(new MigratedWidget("SitefinitySearchResults", migratedProperties));
    }
}
