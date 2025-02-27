using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            context.Source.Properties["BreadcrumbIncludeOption"] = "SpecificPagePath";
            context.Source.Properties["SelectedPage"] = await GetSingleItemMixedContentValue(context, new string[] { startingNodeId }, RestClientContentTypes.Pages, null, false);
        }

        return new MigratedWidget("SitefinityBreadcrumb", context.Source.Properties);
    }
}
