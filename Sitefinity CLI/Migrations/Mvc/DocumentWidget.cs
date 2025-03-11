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

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class DocumentWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        await context.LogWarning("Now we use the SitefinityDocumentList widget for displaying single document. You may need to add new List view to use when dispaling single document.");

        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);

        var migratedProperties = ProcessProperties(propsToRead, null, null);

        var contentType = RestClientContentTypes.Documents;
        var contentProvider = propsToRead["ProviderName"] ?? "OpenAccessDataProvider";

        if (propsToRead.TryGetValue("Id", out string id))
        {
            var mixedContentValue = await GetSingleItemMixedContentValue(context, [id], contentType, contentProvider, true);
            migratedProperties.Add("SelectedItems", mixedContentValue);
        }
        else
        {
            var mixedContentValue = await GetSingleItemMixedContentValue(context, null, contentType, contentProvider, true);
            migratedProperties.Add("SelectedItems", mixedContentValue);
        }

        migratedProperties.Add("SfViewName", "DocumentList");
        migratedProperties.Add("SfDetailViewName", "Details.DocumentDetails");

        MigrateCssClass(propsToRead, migratedProperties);

        return new MigratedWidget("SitefinityDocumentList", migratedProperties);
    }

    private static void MigrateCssClass(Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        var cssClasses = new List<object>();
        if (propsToRead.TryGetValue("CssClass", out string cssClass))
        {
            cssClasses.Add(new { FieldName = "Document list", CssClass = cssClass });
        }

        if (cssClasses.Count != 0)
        {
            migratedProperties.Add("CssClasses", JsonSerializer.Serialize(cssClasses));
        }
    }
}
