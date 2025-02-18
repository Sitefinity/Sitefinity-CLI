using Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
using Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.MigrationTool.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;

#pragma warning disable CA2211

public static class WidgetMigrationDefaults
{
    public static ReadOnlyDictionary<string, WidgetMigrationArgs> MigrationMap = new ReadOnlyDictionary<string, WidgetMigrationArgs>(new Dictionary<string, WidgetMigrationArgs>()
    {
        { "ContentBlock", new WidgetMigrationArgs("SitefinityContentBlock") { Whitelist = ["Content"] } },
        { "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock", new WidgetMigrationArgs("SitefinityContentBlock")
        {
            Whitelist = ["Html"],
            Rename = new Dictionary<string, string>()
            {
                { "Html", "Content" }
            }
        }
    } });

    public static ReadOnlyDictionary<string, IWidgetMigration> CustomMigrations = new ReadOnlyDictionary<string, IWidgetMigration>(new Dictionary<string, IWidgetMigration>()
    {
        { "Layout", new LayoutMigration() },
        { "Telerik.Sitefinity.Web.UI.PublicControls.ImageControl", new ImageWidget() },
        { "Telerik.Sitefinity.Modules.News.Web.UI.NewsView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Blogs.Web.UI.BlogPostView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Events.Web.UI.EventsView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Lists.Web.UI.ListView", new ContentWidget() },
        { "Telerik.Sitefinity.Web.UI.NavigationControls.LightNavigationControl", new NavigationWidget() }
    });
}
#pragma warning restore CA2211
