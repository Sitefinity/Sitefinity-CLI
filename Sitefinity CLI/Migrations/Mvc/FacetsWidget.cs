﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class FacetsWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["IndexCatalogue", "SelectedFacets", "SortType", "DisplayItemCount", "IsShowMoreLessButtonActive", "WidgetCssClass", "SearchFields", "FilterResultsLabel", "AppliedFiltersLabel", "ClearAllLabel", "ShowMoreLabel", "ShowLessLabel"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "TemplateName", "SfViewName" },
    };

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        return Task.FromResult(new MigratedWidget("SitefinityFacets", migratedProperties));
    }
}
