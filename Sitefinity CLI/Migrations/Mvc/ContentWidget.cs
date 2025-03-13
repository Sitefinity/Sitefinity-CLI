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
internal class ContentWidget : MigrationBase, IWidgetMigration
{
    protected virtual string RendererWidgetName { get { return "SitefinityContentList"; } }

    public virtual async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);

        var propertiesToCopy = new[] { "ContentViewDisplayMode", "PageTitleMode", "SelectionGroupLogicalOperator", "DisableCanonicalUrlMetaTag", "ShowDetailsViewOnChildDetailsView" };
        var propertiesToRename = new Dictionary<string, string>()
        {
            { "MetadataFields-MetaTitle", "MetaTitle" },
            { "MetadataFields-MetaDescription", "MetaDescription" },
            { "MetadataFields-OpenGraphTitle", "OpenGraphTitle" },
            { "MetadataFields-OpenGraphDescription", "OpenGraphDescription" },
            { "MetadataFields-OpenGraphType", "OpenGraphType" },
            { "MetadataFields-OpenGraphImage", "OpenGraphImage" },
            { "MetadataFields-OpenGraphVideo", "OpenGraphVideo" },
            { "MetadataFields-SEOEnabled", "SEOEnabled" },
            { "MetadataFields-PageTitleMode", "PageTitleMode" },
            { "MetadataFields-OpenGraphEnabled", "OpenGraphEnabled" },
            { "ShowListViewOnEmpyParentFilter", "ShowListViewOnEmptyParentFilter" }
        };

        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        var contentType = await GetContentType(context, propsToRead);

        if (contentType == null)
        {
            await context.LogWarning($"No property found for content type!");
            return null;
        }

        if (propsToRead.TryGetValue("UrlKeyPrefix", out string urlKeyPrefix) && !string.IsNullOrEmpty(urlKeyPrefix))
        {
            await context.LogWarning($"The UrlKeyPrefix was not migrated. You may need to make some manual changes in the widget.");
        }

        if (propsToRead.TryGetValue("HideListViewOnChildDetailsView", out string hideListViewOnChildDetailsViewString))
        {
            var showListViewOnChildDetailsView = !bool.Parse(hideListViewOnChildDetailsViewString);
            migratedProperties.Add("ShowListViewOnChildDetailsView", showListViewOnChildDetailsView.ToString());
        }

        var contentProvider = propsToRead["ProviderName"];
        if (string.IsNullOrEmpty(contentProvider))
        {
            if (contentType.StartsWith("Telerik.Sitefinity.DynamicTypes.Model", StringComparison.Ordinal))
            {
                contentProvider = "OpenAccessProvider";
            }
            else
            {
                contentProvider = "OpenAccessDataProvider";
            }
        }

        await MigrateItemInDetails(context, propsToRead, migratedProperties, contentType, contentProvider);
        await MigrateAdditionalFilter(context, propsToRead, migratedProperties, contentType, contentProvider);
        await MigrateSelectedItems(context, propsToRead, migratedProperties, contentType, contentProvider);
        if (!migratedProperties.ContainsKey("SelectedItems"))
        {
            var selectedItemsValue = GetMixedContentValue(null, contentType, contentProvider);
            migratedProperties.Add("SelectedItems", selectedItemsValue);
        }

        await MigrateDetailsPage(context, propsToRead, migratedProperties);

        await MigrateViews(context, propsToRead, migratedProperties, contentType);
        //MigrateMetaProperties(propsToRead, migratedProperties);
        MigrateCssClass(propsToRead, migratedProperties);
        MigrateUrlEvaluationMode(propsToRead, migratedProperties);
        MigratePaginationAndOrdering(propsToRead, migratedProperties, contentType);

        return new MigratedWidget(RendererWidgetName, migratedProperties);
    }

    protected virtual async Task MigrateViews(WidgetMigrationContext context, Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties, string contentType)
    {
        await context.LogWarning($"Defaulting to view ListWithSummary for content type {contentType}");

        migratedProperties.Add("SfViewName", "ListWithSummary");

        var fieldMappingList = new List<FieldMapping>()
        {
            new FieldMapping() { FriendlyName = "Title", Name = "Title" },
            new FieldMapping() { FriendlyName = "Text", Name = "Title" },
            new FieldMapping() { FriendlyName = "Publication date", Name = "LastModified" }
        };

        migratedProperties.Add("ListFieldMapping", JsonSerializer.Serialize(fieldMappingList));

        string migratedDetailsViewName = null;
        switch (contentType)
        {
            case RestClientContentTypes.News:
                migratedDetailsViewName = "Details.News.Default";
                break;
            case RestClientContentTypes.BlogPost:
                migratedDetailsViewName = "Details.Blogs.Default";
                break;
            case RestClientContentTypes.Events:
                migratedDetailsViewName = "Details.Events.Default";
                break;
            case RestClientContentTypes.ListItems:
                migratedDetailsViewName = "Details.ListItems.Default";
                break;
            default:
                migratedDetailsViewName = "Details.Dynamic.Default";
                break;
        }

        migratedProperties.Add("SfDetailViewName", migratedDetailsViewName);
    }

    private static async Task<string> GetContentType(WidgetMigrationContext context, Dictionary<string, string> propsToRead)
    {
        string contentType = null;

        if (!propsToRead.TryGetValue("ContentType", out contentType))
        {
            switch (context.Source.Name)
            {
                case "Telerik.Sitefinity.Frontend.Blogs.Mvc.Controllers.BlogPostController":
                    contentType = RestClientContentTypes.BlogPost;
                    break;
                case "Telerik.Sitefinity.Frontend.Lists.Mvc.Controllers.ListsController":
                    contentType = RestClientContentTypes.ListItems;
                    break;
                case "Telerik.Sitefinity.Frontend.News.Mvc.Controllers.NewsController":
                    contentType = RestClientContentTypes.News;
                    break;
                case "Telerik.Sitefinity.Frontend.Events.Mvc.Controllers.EventController":
                    contentType = RestClientContentTypes.Events;
                    break;
                case "Telerik.Sitefinity.Frontend.Media.Mvc.Controllers.DocumentsListController":
                    contentType = RestClientContentTypes.Documents;
                    break;
            }
        }
        if (string.IsNullOrEmpty(contentType))
        {
            var model = await context.SourceClient.ExecuteUnboundFunction<JObject>(new BoundFunctionArgs()
            {
                Name = "sfmeta",
            });
            var entitySet = (model["entityContainer"]["entitySets"] as JObject).Properties();
            var setName = context.Source.Caption.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
            var contentSet = entitySet.FirstOrDefault(set => string.Equals(set.Name, setName, StringComparison.OrdinalIgnoreCase));
            if (contentSet != null)
            {
                contentType = contentSet.Value["entityType"]["$ref"].ToString().Replace("#/definitions/", string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return contentType;
    }

    private static void MigrateUrlEvaluationMode(Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        var urlEvaluationMode = propsToRead.FirstOrDefault(x => x.Key.Equals("UrlEvaluationMode", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(urlEvaluationMode.Value))
        {
            if (urlEvaluationMode.Value == "UrlPath")
            {
                migratedProperties.Add("PagerMode", "URLSegments");
            }
            else if (urlEvaluationMode.Value == "QueryString")
            {
                migratedProperties.Add("PagerMode", "QueryParameter");
            }
        }
    }

    private static void MigrateCssClass(Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        var cssClasses = new List<object>();
        if (propsToRead.TryGetValue("ListCssClass", out string listCssClass))
        {
            cssClasses.Add(new { FieldName = "Content list", CssClass = listCssClass });
        }
        if (propsToRead.TryGetValue("DetailCssClass", out string detailsCssClass))
        {
            cssClasses.Add(new { FieldName = "Details view", CssClass = detailsCssClass });
        }

        if (cssClasses.Count != 0)
        {
            migratedProperties.Add("CssClasses", JsonSerializer.Serialize(cssClasses));
        }
    }

    private static void MigrateMetaProperties(Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        var metaTitleField = propsToRead.FirstOrDefault(x => x.Key.Equals("MetaTitle", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(metaTitleField.Value))
        {
            migratedProperties.Add("MetaTitle", metaTitleField.Value);
        }

        var metaDescriptionField = propsToRead.FirstOrDefault(x => x.Key.Equals("MetaDescription", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(metaDescriptionField.Value))
        {
            migratedProperties.Add("MetaDescription", metaTitleField.Value);
        }
    }

    private async Task MigrateDetailsPage(WidgetMigrationContext context, Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        var detailsPageId = propsToRead.FirstOrDefault(x => x.Key.EndsWith("DetailsPageId", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(detailsPageId.Value) && Guid.TryParse(detailsPageId.Value, out Guid result))
        {
            var selectedItemsValueForDetailsPage = await GetSingleItemMixedContentValue(context, [detailsPageId.Value], RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("DetailPage", selectedItemsValueForDetailsPage);
            migratedProperties.Add("DetailPageMode", "ExistingPage");
        }
    }

    private static void MigratePaginationAndOrdering(Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties, string contentType)
    {
        propsToRead.TryGetValue("DisplayMode", out string displayMode);
        propsToRead.TryGetValue("ItemsPerPage", out string itemsPerPage);
        propsToRead.TryGetValue("LimitCount", out string limitCount);

        var serializedPageValue = JsonSerializer.Serialize(new
        {
            ItemsPerPage = itemsPerPage ?? "20",
            LimitItemsCount = limitCount ?? "20",
            DisplayMode = displayMode ?? "Paging"
        });

        migratedProperties.Add("ListSettings", serializedPageValue);

        var sortProperty = propsToRead.FirstOrDefault(x => x.Key.EndsWith("SortExpression", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        var sortValue = sortProperty.Value;
        if (contentType == RestClientContentTypes.ListItems && string.IsNullOrEmpty(sortValue))
        {
            sortValue = "Ordinal ASC";
        }

        if (!string.IsNullOrEmpty(sortValue))
        {
            migratedProperties.Add("OrderBy", "Custom");
            migratedProperties.Add("SortExpression", sortValue);
        }
    }

    private async Task MigrateItemInDetails(WidgetMigrationContext context, Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties, string contentType, string contentProvider)
    {
        if (propsToRead.TryGetValue("ContentViewDisplayMode", out string contentViewDisplayMode))
        {
            if (contentViewDisplayMode == "Detail")
            {
                var contentIdProperty = propsToRead.FirstOrDefault(x => x.Key.EndsWith("DataItemId", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
                if (!string.IsNullOrEmpty(contentIdProperty.Value))
                {
                    var mixedContentValue = await GetSingleItemMixedContentValue(context, [contentIdProperty.Value], contentType, contentProvider, true);
                    migratedProperties.Add("SelectedItems", mixedContentValue);
                }
            }
        }
    }

    private async Task MigrateAdditionalFilter(WidgetMigrationContext context, Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties, string contentType, string contentProvider)
    {
        var additionalFilter = propsToRead.FirstOrDefault(x => x.Key.EndsWith("SerializedAdditionalFilters", StringComparison.Ordinal));
        var parentIdsFilter = propsToRead.FirstOrDefault(x => x.Key.EndsWith("SerializedSelectedParentsIds", StringComparison.Ordinal));
        propsToRead.TryGetValue("ParentFilterMode", out string parentFilterMode);
        if (!string.IsNullOrEmpty(additionalFilter.Value) || !string.IsNullOrEmpty(parentIdsFilter.Value) || parentFilterMode == "CurrentlyOpen")
        {
            try
            {
                propsToRead.TryGetValue("SelectionGroupLogicalOperator", out string logicalOperator);
                var queryData = string.IsNullOrEmpty(additionalFilter.Value) ? new QueryData() { QueryItems = [] } : JsonSerializer.Deserialize<QueryData>(additionalFilter.Value);
                var allItemsFilter = new CombinedFilter()
                {
                    Operator = logicalOperator == "AND" ? CombinedFilter.LogicalOperators.And : CombinedFilter.LogicalOperators.Or
                };

                var isDateGroup = false;
                var taxaValueDictionary = new Dictionary<string, List<string>>();
                foreach (var query in queryData.QueryItems)
                {
                    var fieldName = query.Name ?? query.Condition.FieldName;
                    if (query.Value != null && isDateGroup && (fieldName != null && (fieldName.Contains("Date", StringComparison.OrdinalIgnoreCase) || fieldName.Contains("Event", StringComparison.OrdinalIgnoreCase))))
                    {
                        AddDateFilter(allItemsFilter, query);

                        continue;
                    }

                    if (!query.IsGroup)
                    {
                        isDateGroup = false;
                        if (query.Condition.Operator == "Contains")
                        {
                            taxaValueDictionary.TryAdd(query.Condition.FieldName, new List<string>());
                            taxaValueDictionary[query.Condition.FieldName].Add(query.Value);
                        }
                        else
                        {
                            var childFilter = new FilterClause()
                            {
                                FieldName = query.Condition.FieldName,
                                Operator = FilterClause.Operators.Equal,
                                FieldValue = query.Value
                            };
                            allItemsFilter.ChildFilters.Add(childFilter);
                        }
                    }
                    else
                    {
                        if (query.Name.Contains("Date", StringComparison.OrdinalIgnoreCase) || query.Name == "Past" || query.Name == "Upcomming")
                        {
                            isDateGroup = true;

                            var groupFilter = new CombinedFilter();
                            groupFilter.Operator = CombinedFilter.LogicalOperators.And;
                            groupFilter.ChildFilters = new List<object>();
                            allItemsFilter.ChildFilters.Add(groupFilter);
                        }
                    }
                }

                foreach (var taxa in taxaValueDictionary) // this is need to visualize the taxons in the new designer. It works with ["1","2"] contains tag
                    //the old designer did different filter condition for each tag: (tag equals "1") OR (tag equals "2")
                {
                    var childFilter = new FilterClause()
                    {
                        FieldName = taxa.Key,
                        Operator = FilterClause.Operators.ContainsOr,
                        FieldValue = taxa.Value
                    };
                    AddToChildFilters(allItemsFilter, childFilter);
                }

                string selectedListIdsJson = null;
                if ((contentType == RestClientContentTypes.ListItems && propsToRead.TryGetValue("SerializedSelectedItemsIds", out selectedListIdsJson))
                    || (propsToRead.TryGetValue("SerializedSelectedParentsIds", out selectedListIdsJson) && parentFilterMode == "Selected"))
                {
                    var deserialized = JsonSerializer.Deserialize<string[]>(selectedListIdsJson);

                    if (deserialized.Length > 0)
                    {
                        var parentFilter = new FilterClause()
                        {
                            FieldName = "ParentId",
                            FieldValue = deserialized,
                            Operator = FilterClause.Operators.ContainsOr
                        };

                        AddToChildFilters(allItemsFilter, parentFilter);
                    }
                }

                object filterValue = allItemsFilter;
                if (allItemsFilter.ChildFilters.Count == 1)
                {
                    filterValue = allItemsFilter.ChildFilters[0];
                }

                var selectedItemsValue = GetMixedContentValue(filterValue, contentType, contentProvider, parentFilterMode == "CurrentlyOpen");
                migratedProperties.Add("SelectedItems", selectedItemsValue);
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                await context.LogWarning($"Cannot deserialize additional filter for widget {context.Source.Id}. Actual error: {ex.Message}");
            }
#pragma warning restore CA1031
        }
    }
    private static void AddToChildFilters(CombinedFilter allItemsFilter, object filter)
    {
        var groupParentFilter = new CombinedFilter();
        groupParentFilter.Operator = CombinedFilter.LogicalOperators.And;
        groupParentFilter.ChildFilters = [filter];

        if (allItemsFilter.ChildFilters.Count == 0)
        {
            allItemsFilter.ChildFilters.Add(groupParentFilter);
        }
        else
        {
            allItemsFilter.ChildFilters.Add(filter);
        }
    }

    private static void AddDateFilter(CombinedFilter allItemsFilter, QueryItem query)
    {
        var daysString = "DateTime.UtcNow.AddDays";
        var monthsString = "DateTime.UtcNow.AddMonths";
        var yearsString = "DateTime.UtcNow.AddYears";
        if (query.Value.StartsWith(daysString, StringComparison.Ordinal))
        {
            var substringValue = query.Value.Substring(daysString.Length + 1).Trim('(').Trim(')');
            if (int.TryParse(substringValue, out int days))
            {
                var dateFilter = new DateOffsetPeriod()
                {
                    DateFieldName = query.Condition.FieldName,
                    OffsetType = DateOffsetType.Days,
                    OffsetValue = (int)Math.Abs(days),
                };

                AddToChildFilters(allItemsFilter, dateFilter);
            }
        }
        else if (query.Value.StartsWith(monthsString, StringComparison.Ordinal))
        {
            var substringValue = query.Value.Substring(monthsString.Length + 1).Trim('(').Trim(')');
            if (int.TryParse(substringValue, out int months))
            {
                var dateFilter = new DateOffsetPeriod()
                {
                    DateFieldName = query.Condition.FieldName,
                    OffsetType = DateOffsetType.Months,
                    OffsetValue = (int)Math.Abs(months),
                };

                AddToChildFilters(allItemsFilter, dateFilter);
            }
        }
        else if (query.Value.StartsWith(yearsString, StringComparison.Ordinal))
        {
            var substringValue = query.Value.Substring(yearsString.Length + 1).Trim('(').Trim(')');
            if (int.TryParse(substringValue, out int years))
            {
                var dateFilter = new DateOffsetPeriod()
                {
                    DateFieldName = query.Condition.FieldName,
                    OffsetType = DateOffsetType.Years,
                    OffsetValue = (int)Math.Abs(years),
                };
                AddToChildFilters(allItemsFilter, dateFilter);
            }
        }
        else
        {
            var operatorMap = new Dictionary<string, string>() {
                                { ">", FilterClause.Operators.GreaterThan},
                                { ">=", FilterClause.Operators.GreaterThan},
                                { "<", FilterClause.Operators.LessThan},
                                { "<=", FilterClause.Operators.LessThan},
                            };
            operatorMap.TryGetValue(query.Condition.Operator, out string operatorValue);
            if (DateTime.TryParse(query.Value, out DateTime dateTime))
            {
                var childFilter = new FilterClause()
                {
                    FieldName = query.Condition.FieldName,
                    Operator = operatorValue,
                    FieldValue = dateTime.ToString("O", CultureInfo.InvariantCulture)
                };

                (allItemsFilter.ChildFilters.Last() as CombinedFilter).ChildFilters.Add(childFilter);
            }
        }
    }

    private async Task MigrateSelectedItems(WidgetMigrationContext context, Dictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties, string contentType, string contentProvider)
    {
        if (propsToRead.TryGetValue("SerializedSelectedItemsIds", out string selectedListIdsJson) && selectedListIdsJson != null && !migratedProperties.ContainsKey("SelectedItems"))
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<string[]>(selectedListIdsJson);

                var selectedItemsValue = await GetMixedContentValue(context, deserialized, contentType, contentProvider, true);
                migratedProperties.Add("SelectedItems", selectedItemsValue);
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                await context.LogWarning($"Cannot deserialize selected item IDs for widget {context.Source.Id}. Actual error: {ex.Message}");
            }
#pragma warning restore CA1031
        }
    }

    private class FieldMapping
    {
        public string FriendlyName { get; set; }

        public string Name { get; set; }
    }


    private class QueryData
    {
        public QueryItem[] QueryItems { get; set; }
    }

    private class QueryItem
    {
        public bool IsGroup { get; set; }

        public string Join { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public Condition Condition { get; set; }
    }

    private class Condition
    {
        public string FieldName { get; set; }

        public string FieldType { get; set; }

        public string Operator { get; set; }
    }
}
