using System;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class SearchWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass", "SuggestionFields" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("ResultsPageId", out string resultsPageId) && Guid.TryParse(resultsPageId, out _))
        {
            migratedProperties.Add("SearchResultsPage", await GetSingleItemMixedContentValue(context, new string[] { resultsPageId }, RestClientContentTypes.Pages, null, false));
        }

        if (context.Source.Properties.TryGetValue("IndexCatalogue", out string searchIndexName))
        {
            migratedProperties.Add("SearchIndex", searchIndexName);
        }

        migratedProperties.Add("SuggestionsTriggerCharCount", "0");
        if (context.Source.Properties.TryGetValue("DisableSuggestions", out string disableSuggestions) && bool.TryParse(disableSuggestions, out bool disableSuggestionsBool))
        {
            if (!disableSuggestionsBool)
            {
                migratedProperties["SuggestionsTriggerCharCount"] = "3";
            }
        }

        return new MigratedWidget("SitefinitySearchBox", migratedProperties);
    }
}
