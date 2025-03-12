using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk.Client;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class TaxonomyWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass", "ShowItemCount", "SortExpression"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "ShowEmptyTaxa", "ShowEmpty" },
    };
    internal static readonly char[] separator = [','];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        if (propsToRead.TryGetValue("TaxonomyId", out string taxonomyId))
        {
            var classificationSettings = new Dictionary<string, object>
            {
                { "selectedTaxonomyId", taxonomyId },
                { "selectionMode", "All" },
                { "selectedTaxaIds", Array.Empty<string>() }
            };

            var taxonomy = (context.SourceClient as RestClient).ServiceMetadata.GetTaxonomies().FirstOrDefault(x => x.Id == taxonomyId);
            if (taxonomy != null)
            {
                classificationSettings.Add("selectedTaxonomyName", taxonomy.Name);
                classificationSettings.Add("selectedTaxonomyTitle", taxonomy.Title);
                classificationSettings.Add("selectedTaxonomyUrl", taxonomy.TaxaUrl);
            }

            if (propsToRead.TryGetValue("TaxaToDisplay", out string taxaToDisplay))
            {
                if (!string.IsNullOrEmpty(taxaToDisplay))
                {
                    switch (taxaToDisplay)
                    {
                        case "All":
                        case "TopLevel":
                            classificationSettings["selectionMode"] = taxaToDisplay;
                            break;
                        case "UnderParticularTaxon":
                            classificationSettings["selectionMode"] = "UnderParent";
                            classificationSettings["selectedTaxaIds"] = new string[] { propsToRead["RootTaxonId"] };
                            break;
                        case "Selected":
                            var taxaIds = JsonSerializer.Deserialize<string[]>(propsToRead["SerializedSelectedTaxaIds"]);
                            classificationSettings["selectedTaxaIds"] = taxaIds;
                            classificationSettings["selectionMode"] = "Selected";
                            break;
                        case "UsedByContentType":
                            var contentTypeName = propsToRead["ContentTypeName"] ?? propsToRead["DynamicContentTypeName"];
                            classificationSettings["selectionMode"] = "ByContentType";
                            classificationSettings["byContentType"] = contentTypeName;
                            break;
                        default:
                            break;
                    }
                }
            }

            migratedProperties.TryGetValue("SortExpression", out string sortExpression);
            migratedProperties["OrderBy"] = "Custom";
            if (sortExpression.Equals("title asc", StringComparison.OrdinalIgnoreCase))
            {
                migratedProperties["OrderBy"] = "Title asc";
            }
            else if (sortExpression.Equals("title desc", StringComparison.OrdinalIgnoreCase))
            {
                migratedProperties["OrderBy"] = "Title desc";
            }
            else if (sortExpression.Equals("AsSetManually", StringComparison.OrdinalIgnoreCase))
            {
                migratedProperties["OrderBy"] = "Manually";
            }

            migratedProperties.Add("ClassificationSettings", JsonSerializer.Serialize(classificationSettings));
        }

        return Task.FromResult(new MigratedWidget("SitefinityClassification", migratedProperties));
    }
}
