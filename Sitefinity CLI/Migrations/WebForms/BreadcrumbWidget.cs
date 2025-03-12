using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class BreadcrumbWidget : MigrationBase, IWidgetMigration
{
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "ShowHomePage", "AddHomePageLinkAtBeginning" },
        { "CssClass", "WrapperCssClass" },
        { "ShowCurrentPage", "AddCurrentPageLinkAtTheEnd" },
        { "ShowGroupPages", "IncludeGroupPages" },
    };

    private static readonly string[] propertiesToCopy = new string[] { "AllowVirtualNodes" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);

        if (context.Source.Properties.TryGetValue("StartingNodeId", out string startingNodeId) && Guid.TryParse(startingNodeId, out _))
        {
            migratedProperties["BreadcrumbIncludeOption"] = "SpecificPagePath";
            migratedProperties["SelectedPage"] = await GetSingleItemMixedContentValue(context, new string[] { startingNodeId }, RestClientContentTypes.Pages, null, false);
        }

        return new MigratedWidget("SitefinityBreadcrumb", migratedProperties);
    }
}
