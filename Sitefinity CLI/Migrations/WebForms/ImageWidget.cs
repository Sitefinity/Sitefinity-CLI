using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class ImageWidget : MigrationBase, IWidgetMigration
{
    internal class IdPair
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string MasterId { get; set; }
    }

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propertiesToCopy = new[] { "Title", "CssClass" };
        var propertiesToRename = new Dictionary<string, string>()
        {
            { "AlternateText", "AlternativeText" }
        };

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);

        string masterImageId = null;
        string providerName = null;

        if (context.Source.Properties.TryGetValue("ImageId", out string imageId) && context.Source.Properties.TryGetValue("ProviderName", out providerName) && providerName != null)
        {
            var response = await GetMasterIds(context, [imageId], "Telerik.Sitefinity.Libraries.Model.Image", providerName);
            masterImageId = response[0];

            var serializedItem = JsonSerializer.Serialize(new
            {
                Id = masterImageId,
                Provider = providerName,
            });

            migratedProperties.Add("Item", serializedItem);
        }

        if (context.Source.Properties.TryGetValue("OpenOriginalImageOnClick", out string openOriginalImageOnClickString) && bool.TryParse(openOriginalImageOnClickString, out bool openOriginalImageOnClick) && openOriginalImageOnClick)
        {
            migratedProperties.Add("ClickAction", "OpenOriginalSize");
        }

        if (context.Source.Properties.TryGetValue("DisplayMode", out string displayMode) && string.Equals(displayMode, "Custom", StringComparison.Ordinal))
        {
            await context.LogWarning("Cannot migrate images with display mode 'Custom'");
        }

        if (context.Source.Properties.TryGetValue("ThumbnailName", out string thumbnailName) && thumbnailName != null && masterImageId != null && providerName != null)
        {
            var item = await context.SourceClient.GetItem<SdkItem>(new GetItemArgs()
            {
                Type = RestClientContentTypes.Images,
                Id = masterImageId,
                Provider = providerName
            });

            migratedProperties.Add("ImageSize", "Thumbnail");
            migratedProperties.Add("Thumnail", JsonSerializer.Serialize(new
            {
                Name = thumbnailName,
                OriginalUrl = item?.GetValue<string>("Url")
            }));
        }

        return new MigratedWidget("SitefinityImage", migratedProperties);
    }
}
