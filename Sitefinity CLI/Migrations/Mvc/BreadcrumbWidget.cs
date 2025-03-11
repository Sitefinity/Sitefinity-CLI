using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class BreadcrumbWidget : MigrationBase, IWidgetMigration
{
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "ShowHomePageLink", "AddHomePageLinkAtBeginning" },
        { "CssClass", "WrapperCssClass" },
        { "ShowCurrentPageInTheEnd", "AddCurrentPageLinkAtTheEnd" },
        { "ShowGroupPages", "IncludeGroupPages" },
    };

    private static readonly string[] propertiesToCopy = new string[] { "AllowVirtualNodes" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);

        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        if (propsToRead.TryGetValue("StartingPageId", out string startingNodeId) && Guid.TryParse(startingNodeId, out _))
        {
            migratedProperties["BreadcrumbIncludeOption"] = "SpecificPagePath";
            migratedProperties["SelectedPage"] = await GetSingleItemMixedContentValue(context, new string[] { startingNodeId }, RestClientContentTypes.Pages, null, false);
        }

        return new MigratedWidget("SitefinityBreadcrumb", migratedProperties);
    }
}
