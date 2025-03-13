using Newtonsoft.Json.Linq;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class DocumentListWidget : ContentWidget
{
    protected override string RendererWidgetName { get { return "SitefinityDocumentList";  } }

    protected override async Task MigrateViews(WidgetMigrationContext context, IDictionary<string, string> migratedProperties, string contentType)
    {
        await context.LogWarning($"Defaulting to view DocumentList for content type {contentType}");
        context.Source.Properties.TryGetValue("MasterViewName", out string listViewName);

        string rendererListViewName;
        switch (listViewName)
        {
            case "MasterListView":
                rendererListViewName = "DocumentList";
                break;
            case "MasterTableView":
                rendererListViewName = "DocumentTable";
                break;
            default:
                rendererListViewName = "DocumentList";
                break;
        }
        migratedProperties.Add("SfViewName", rendererListViewName);

        migratedProperties.Add("SfDetailViewName", "Details.DocumentDetails");
    }
}
