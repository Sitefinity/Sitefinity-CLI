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
        { "ContentBlock", new WidgetMigrationArgs("SitefinityContentBlock")
            {
                Whitelist = ["Content", "ProviderName", "SharedContentID", "WrapperCssClass"],
                Rename = new Dictionary<string, string>()
                {
                    { "Html", "Content" },
                }
            }
        },
        { "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock", new WidgetMigrationArgs("SitefinityContentBlock")
            {
                Whitelist = ["Html", "ProviderName", "SharedContentID"],
                Rename = new Dictionary<string, string>()
                {
                    { "Html", "Content" },
                }
            }
        },
        { "Telerik.Sitefinity.Security.Web.UI.UserChangePasswordWidget", new WidgetMigrationArgs("SitefinityChangePassword") { Whitelist = ["CssClass"] } },
    });

    public static ReadOnlyDictionary<string, IWidgetMigration> CustomMigrations = new ReadOnlyDictionary<string, IWidgetMigration>(new Dictionary<string, IWidgetMigration>()
    {
        { "Layout", new Mvc.LayoutMigration() },
        { "Telerik.Sitefinity.Frontend.Taxonomies.Mvc.Controllers.FlatTaxonomyController", new Mvc.TaxonomyWidget() },
        { "Telerik.Sitefinity.Frontend.Taxonomies.Mvc.Controllers.HierarchicalTaxonomyController", new Mvc.TaxonomyWidget() },
        { "Telerik.Sitefinity.Frontend.Navigation.Mvc.Controllers.BreadcrumbController", new Mvc.BreadcrumbWidget() },
        { "Telerik.Sitefinity.Frontend.Navigation.Mvc.Controllers.NavigationController", new Mvc.NavigationWidget() },
        { "Telerik.Sitefinity.Frontend.Search.Mvc.Controllers.SearchBoxController", new Mvc.SearchBoxWidget() },
        { "Telerik.Sitefinity.Frontend.Search.Mvc.Controllers.SearchResultsController", new Mvc.SearchResultsWidget() },
        { "Telerik.Sitefinity.Frontend.Search.Mvc.Controllers.FacetsController", new Mvc.FacetsWidget() },
        { "Telerik.Sitefinity.Frontend.Recommendations.Mvc.Controllers.RecommendationsController", new Mvc.RecommendationsWidget() },
        { "Telerik.Sitefinity.NativeChatConnector.Mvc.Controllers.NativeChatController", new Mvc.NativeChatWidget() },
        { "Telerik.Sitefinity.Frontend.Identity.Mvc.Controllers.ChangePasswordController", new Mvc.ChangePasswordWidget() },
        { "Telerik.Sitefinity.Frontend.Identity.Mvc.Controllers.LoginFormController", new Mvc.LoginWidget() },
        { "Telerik.Sitefinity.Frontend.Identity.Mvc.Controllers.ProfileController", new Mvc.ProfileWidget() },
        { "Telerik.Sitefinity.Frontend.Identity.Mvc.Controllers.RegistrationController", new Mvc.RegistrationWidget() },
        { "Telerik.Sitefinity.Frontend.Media.Mvc.Controllers.ImageController", new Mvc.ImageWidget() },
        { "Telerik.Sitefinity.Frontend.DynamicContent.Mvc.Controllers.DynamicContentController", new Mvc.ContentWidget() },
        { "Telerik.Sitefinity.Frontend.News.Mvc.Controllers.NewsController", new Mvc.ContentWidget() },
        { "Telerik.Sitefinity.Frontend.Blogs.Mvc.Controllers.BlogPostController", new Mvc.ContentWidget() },
        { "Telerik.Sitefinity.Frontend.Events.Mvc.Controllers.EventController", new Mvc.ContentWidget() },
        { "Telerik.Sitefinity.Frontend.Lists.Mvc.Controllers.ListsController", new Mvc.ContentWidget() },
        { "Telerik.Sitefinity.Frontend.Media.Mvc.Controllers.DocumentController", new Mvc.DocumentWidget() },
        { "Telerik.Sitefinity.Frontend.Media.Mvc.Controllers.DocumentsListController", new Mvc.DocumentListWidget() },
        { "Telerik.Sitefinity.DynamicModules.Web.UI.Frontend.DynamicContentView", new ContentWidget() },
        { "Telerik.Sitefinity.Web.UI.PublicControls.ImageControl", new ImageWidget() },
        { "Telerik.Sitefinity.Modules.Libraries.Web.UI.Documents.DocumentLink", new DocumentWidget() },
        { "Telerik.Sitefinity.Modules.Libraries.Web.UI.Documents.DownloadListView", new DocumentListWidget() },
        { "Telerik.Sitefinity.Modules.News.Web.UI.NewsView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Blogs.Web.UI.BlogPostView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Events.Web.UI.EventsView", new ContentWidget() },
        { "Telerik.Sitefinity.Modules.Lists.Web.UI.ListView", new ContentWidget() },
        { "Telerik.Sitefinity.Web.UI.NavigationControls.LightNavigationControl", new WebForms.NavigationWidget() },
        { "Telerik.Sitefinity.Web.UI.NavigationControls.Breadcrumb.Breadcrumb", new WebForms.BreadcrumbWidget() },
        { "Telerik.Sitefinity.Security.Web.UI.UserProfileView", new ProfileWidget() },
        { "Telerik.Sitefinity.Web.UI.PublicControls.LoginWidget", new LoginWidget() },
        { "Telerik.Sitefinity.Security.Web.UI.RegistrationForm", new RegistrationWidget() },
        { "Telerik.Sitefinity.Services.Search.Web.UI.Public.SearchBox", new SearchWidget() },
        { "Telerik.Sitefinity.Services.Search.Web.UI.Public.SearchResults", new SearchResultsWidget() },
        { "Telerik.Sitefinity.Web.UI.PublicControls.TaxonomyControl", new WebForms.TaxonomyWidget() },
        { "Telerik.Sitefinity.Modules.Forms.Web.UI.FormsControl", new WebForms.FormWidget() }
    });

    public static ReadOnlyDictionary<string, IWidgetMigration> CustomFormMigrations = new ReadOnlyDictionary<string, IWidgetMigration>(new Dictionary<string, IWidgetMigration>()
    {
        { "Layout", new Mvc.LayoutMigration() },
        { "Telerik.Sitefinity.Modules.Forms.Web.UI.Fields.FormTextBox", new TextBox() }
    });
}
#pragma warning restore CA2211
