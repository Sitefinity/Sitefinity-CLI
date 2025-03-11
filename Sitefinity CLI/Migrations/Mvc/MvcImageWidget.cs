using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Clients.Pages.Dto;
using Progress.Sitefinity.RestSdk.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class MvcImageWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);

        var propertiesToCopy = new[] { "Title", "CssClass", "AlternativeText" };
        var propertiesToRename = new Dictionary<string, string>() {};

        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        if (propsToRead.TryGetValue("Id", out string imageId) && propsToRead.TryGetValue("ProviderName", out string providerName) && providerName != null)
        {
            var response = await GetMasterIds(context, [imageId], "Telerik.Sitefinity.Libraries.Model.Image", providerName);

            var serializedItem = JsonSerializer.Serialize(new
            {
                Id = response[0],
                ProviderName = providerName,
            });

            migratedProperties.Add("Item", serializedItem);
        }

        if (propsToRead.TryGetValue("UseAsLink", out string useAsLinkString) && bool.TryParse(useAsLinkString, out bool useAsLink) && useAsLink)
        {
            if (propsToRead.TryGetValue("LinkedPageId", out string linkedPageIdString) && Guid.TryParse(linkedPageIdString, out Guid linkedPageId) && linkedPageId != Guid.Empty)
            {
                migratedProperties.Add("ClickAction", "OpenLink");

                var pageNode = await context.SourceClient.GetItem<PageNodeDto>(new GetItemArgs()
                {
                    Type = RestClientContentTypes.Pages,
                    Id = linkedPageIdString,
                });

                migratedProperties.Add("ActionLink", JsonSerializer.Serialize(new
                {
                    id = linkedPageIdString,
                    type = "Pages",
                    href = new Uri(pageNode?.ViewUrl ?? string.Empty, UriKind.RelativeOrAbsolute),
                    text = pageNode?.Title
                }));
            }
            else
            {
                migratedProperties.Add("ClickAction", "OpenOriginalSize");
            }
        }

        if (propsToRead.TryGetValue("DisplayMode", out string displayMode) && string.Equals(displayMode, "Custom", StringComparison.Ordinal))
        {
            migratedProperties.Add("ImageSize", "CustomSize");
            if (propsToRead.TryGetValue("CustomSize", out string customSize) && customSize != null)
            {
                var customSizeDictionary = JsonSerializer.Deserialize<Dictionary<string,object>>(customSize);
                migratedProperties.Add("CustomSize", JsonSerializer.Serialize(new
                {
                    Width = customSizeDictionary["Method"].ToString() == "ResizeFitToAreaArguments" ?  customSizeDictionary["MaxWidth"]: customSizeDictionary["Width"],
                    Height = customSizeDictionary["Method"].ToString() == "ResizeFitToAreaArguments" ? customSizeDictionary["MaxHeight"]: customSizeDictionary["Height"],
                    OriginalWidth = customSizeDictionary["Width"],
                    OriginalHeight = customSizeDictionary["Height"],
                    ConstrainToProportions = customSizeDictionary["ScaleUp"].ToString() == "True",
                }));
                migratedProperties.Add("FitToContainer", customSizeDictionary["ScaleUp"].ToString());
            }
        }
        if (propsToRead.TryGetValue("Responsive", out string responsiveString) && bool.TryParse(responsiveString, out bool responsive) && responsive)
        {
            migratedProperties.Add("ImageSize", "Responsive");
        }

        if (propsToRead.TryGetValue("ThumbnailName", out string thumbnailName) && thumbnailName != null)
        {
            migratedProperties.Add("ImageSize", "Thumbnail");
            migratedProperties.Add("Thumnail", JsonSerializer.Serialize(new
            {
                Name = thumbnailName,
            }));
        }

        if (!migratedProperties.ContainsKey("ImageSize"))
        {
            migratedProperties.Add("ImageSize", "OriginalSize");
        }

        return new MigratedWidget("SitefinityImage", migratedProperties);
    }
}
