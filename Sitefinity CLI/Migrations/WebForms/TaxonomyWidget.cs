using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class TaxonomyWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass", "ShowItemCount", "SortExpression"];
    internal static readonly char[] separator = [','];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("TaxonomyId", out string taxonomyId))
        {
            var classificationSettings = new Dictionary<string, object>
            {
                { "selectedTaxonomyId", taxonomyId }
            };

            var taxonomy = (context.SourceClient as RestClient).ServiceMetadata.GetTaxonomies().FirstOrDefault(x => x.Id == taxonomyId);
            if (taxonomy != null)
            {
                classificationSettings.Add("selectedTaxonomyName", taxonomy.Name);
                classificationSettings.Add("selectedTaxonomyTitle", taxonomy.Title);
                classificationSettings.Add("selectedTaxonomyUrl", taxonomy.TaxaUrl);
            }

            if (context.Source.Properties.TryGetValue("SelectedTaxaIds", out string selectedTaxaIds) && selectedTaxaIds != null)
            {
                var splitTaxaIds = selectedTaxaIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                classificationSettings.Add("selectedTaxaIds", splitTaxaIds);
                classificationSettings.Add("selectionMode", "Selected");
            }

            if (context.Source.Properties.TryGetValue("RootTaxonId", out string rootTaxonId) && rootTaxonId != null)
            {
                classificationSettings.Add("selectionMode", "UnderParent");
                classificationSettings.Add("selectedTaxaIds", rootTaxonId);
            }

            if (!classificationSettings.ContainsKey("selectionMode"))
            {
                if (context.Source.Properties.TryGetValue("TaxaToDisplay", out string selectionMode) && selectionMode != null)
                {
                    classificationSettings.Add("selectionMode", selectionMode);
                }

                if (!classificationSettings.ContainsKey("selectionMode"))
                {
                    classificationSettings.Add("selectionMode", "All");
                }
            }

            if (!classificationSettings.ContainsKey("selectedTaxaIds"))
            {
                classificationSettings.Add("selectedTaxaIds", Array.Empty<string>());
            }

            migratedProperties.Add("ClassificationSettings", JsonSerializer.Serialize(classificationSettings));
        }

        return Task.FromResult(new MigratedWidget("SitefinityClassification", migratedProperties));
    }
}
