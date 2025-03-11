using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class SearchBoxWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass", "SuggestionFields" };
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "IndexCatalogue", "SearchIndex" },
        { "BackgroundHint", "SearchBoxPlaceholder" },
        { "ScoringProfiles-ScoringProfile", "ScoringProfile" }
    };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        if (propsToRead.TryGetValue("ResultsPageId", out string resultsPageId) && Guid.TryParse(resultsPageId, out _))
        {
            migratedProperties.Add("SearchResultsPage", await GetSingleItemMixedContentValue(context, new string[] { resultsPageId }, RestClientContentTypes.Pages, null, false));
        }

        migratedProperties.Add("SuggestionsTriggerCharCount", "0");
        if (propsToRead.TryGetValue("DisableSuggestions", out string disableSuggestions) && bool.TryParse(disableSuggestions, out bool disableSuggestionsBool))
        {
            if (!disableSuggestionsBool)
            {
                migratedProperties["SuggestionsTriggerCharCount"] = "3";
            }
        }

        if (propsToRead.TryGetValue("ScoringProfiles-ScoringParameters", out string scoringParameters))
        {
            // testparam1:value1,value2;testparam2:value1,value2
            var scoringParametersList = new List<string>();
            var paramsSplitted = scoringParameters.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var paramSplitted in paramsSplitted)
            {
                var paramNameAndValues = paramSplitted.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var paramName = paramNameAndValues[0];
                var values = paramNameAndValues[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    scoringParametersList.Add($"{paramName}:{value}");
                }
            }

            migratedProperties.Add("ScoringParameters", JsonSerializer.Serialize(scoringParametersList));
        }

        // 0 is "As set for the system"
        migratedProperties["ShowResultsForAllIndexedSites"] = "0";
        if (propsToRead.TryGetValue("SearchInAllSitesInTheIndex", out string searchInAllSitesInTheIndex) && bool.TryParse(searchInAllSitesInTheIndex, out bool searchInAllSitesInTheIndexBool))
        {
            if (searchInAllSitesInTheIndexBool)
            {
                migratedProperties["ShowResultsForAllIndexedSites"] = "1";
            }
        }

        return new MigratedWidget("SitefinitySearchBox", migratedProperties);
    }
}
