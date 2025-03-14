using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class ContentWidget : MigrationBase, IWidgetMigration
{
    protected virtual string RendererWidgetName { get { return "SitefinityContentList"; } }

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propertiesToCopy = new[] { "ContentViewDisplayMode", "PageTitleMode" };

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);
        var contentType = context.Source.Properties["ControlDefinition-ContentType"];
        var contentProvider = context.Source.Properties["ControlDefinition-ProviderName"];
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

        await MigrateItemInDetails(context, migratedProperties, contentType, contentProvider);
        await MigrateAdditionalFilter(context, migratedProperties, contentType, contentProvider);
        if (!migratedProperties.ContainsKey("SelectedItems"))
        {
            var selectedItemsValue = GetMixedContentValue(null, contentType, contentProvider);
            migratedProperties.Add("SelectedItems", selectedItemsValue);
        }

        await MigrateDetailsPage(context, migratedProperties);

        await MigrateViews(context, migratedProperties, contentType);
        MigrateMetaProperties(context, migratedProperties);
        MigrateCssClass(context, migratedProperties);
        MigrateUrlEvaluationMode(context, migratedProperties);
        MigratePaginationAndOrdering(context, migratedProperties, contentType);

        return new MigratedWidget(RendererWidgetName, migratedProperties);
    }

    protected virtual async Task MigrateViews(WidgetMigrationContext context, IDictionary<string, string> migratedProperties, string contentType)
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

    private static void MigrateUrlEvaluationMode(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        var urlEvaluationMode = context.Source.Properties.FirstOrDefault(x => x.Key.Equals("UrlEvaluationMode", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
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

    private static void MigrateCssClass(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        var cssClass = context.Source.Properties.FirstOrDefault(x => x.Key.Equals("CssClass", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(cssClass.Value))
        {
            migratedProperties.Add("CssClasses", JsonSerializer.Serialize(new object[] { new { FieldName = "Content list", CssClass = cssClass.Value } }));
        }
    }

    private static void MigrateMetaProperties(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        var metaTitleField = context.Source.Properties.FirstOrDefault(x => x.Key.Equals("MetaTitleField", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(metaTitleField.Value))
        {
            migratedProperties.Add("MetaTitle", metaTitleField.Value);
        }

        var metaDescriptionField = context.Source.Properties.FirstOrDefault(x => x.Key.Equals("MetaDescriptionField", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(metaDescriptionField.Value))
        {
            migratedProperties.Add("MetaDescription", metaTitleField.Value);
        }
    }

    private async Task MigrateDetailsPage(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        var detailsPageId = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("-DetailsPageId", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(detailsPageId.Value) && Guid.TryParse(detailsPageId.Value, out Guid result))
        {
            var selectedItemsValueForDetailsPage = await GetSingleItemMixedContentValue(context, [detailsPageId.Value], RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("DetailPage", selectedItemsValueForDetailsPage);
            migratedProperties.Add("DetailPageMode", "ExistingPage");
        }
    }

    private static void MigratePaginationAndOrdering(WidgetMigrationContext context, IDictionary<string, string> migratedProperties, string contentType)
    {
        context.Source.Properties.TryGetValue("MasterViewName", out string listViewName);
        string displayMode = "Paging";
        var allowPaging = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith(listViewName + "-AllowPaging", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (string.Equals(allowPaging.Value, bool.FalseString, StringComparison.Ordinal))
        {
            displayMode = "Limit";
        }

        var itemsPerPage = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith(listViewName + "-ItemsPerPage", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
        if (!string.IsNullOrEmpty(itemsPerPage.Value))
        {
            var serializedPageValue = JsonSerializer.Serialize(new
            {
                ItemsPerPage = itemsPerPage.Value,
                LimitItemsCount = itemsPerPage.Value,
                DisplayMode = displayMode
            });

            migratedProperties.Add("ListSettings", serializedPageValue);
        }

        var sortProperty = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("SortExpression", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
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

    private async Task MigrateItemInDetails(WidgetMigrationContext context, IDictionary<string, string> migratedProperties, string contentType, string contentProvider)
    {
        if (context.Source.Properties.TryGetValue("ContentViewDisplayMode", out string contentViewDisplayMode))
        {
            if (contentViewDisplayMode == "Detail")
            {
                if (!migratedProperties.ContainsKey("SelectedItems"))
                {
                    var contentIdProperty = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("DataItemId", StringComparison.Ordinal) && !string.IsNullOrEmpty(x.Value));
                    if (!string.IsNullOrEmpty(contentIdProperty.Value))
                    {
                        var mixedContentValue = await GetSingleItemMixedContentValue(context, [contentIdProperty.Value], contentType, contentProvider, true);
                        migratedProperties.Add("SelectedItems", mixedContentValue);
                    }
                }
            }
        }
    }

    private async Task MigrateAdditionalFilter(WidgetMigrationContext context, IDictionary<string, string> migratedProperties, string contentType, string contentProvider)
    {
        if (migratedProperties.ContainsKey("SelectedItems"))
            return;

        var propsToRead = context.Source.Properties;
        var additionalFilter = propsToRead.FirstOrDefault(x => (x.Key.EndsWith("-AdditionalFilter", StringComparison.Ordinal) || x.Key.EndsWith("AdditionalFilters", StringComparison.Ordinal)) && !string.IsNullOrEmpty(x.Value));
        var parentIdsFilter = propsToRead.FirstOrDefault(x => x.Key.EndsWith("ItemsParentsIds", StringComparison.Ordinal));
        var selectedListIds = propsToRead.FirstOrDefault(x => x.Key.EndsWith("SelectedListIds", StringComparison.Ordinal));
        if (!string.IsNullOrEmpty(additionalFilter.Value) || !string.IsNullOrEmpty(parentIdsFilter.Value) || !string.IsNullOrEmpty(selectedListIds.Value))
        {
            try
            {
                var queryData = string.IsNullOrEmpty(additionalFilter.Value) ? new QueryData() { QueryItems = [] } : JsonSerializer.Deserialize<QueryData>(additionalFilter.Value);
                var allItemsFilter = new CombinedFilter()
                {
                    Operator = CombinedFilter.LogicalOperators.And
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
                    allItemsFilter.ChildFilters.Add(childFilter);
                }

                var deserialized = Array.Empty<string>();
                if (contentType == RestClientContentTypes.ListItems && propsToRead.TryGetValue("SelectedListIds", out string selectedListIdsJson))
                {
                    deserialized = JsonSerializer.Deserialize<string[]>(selectedListIdsJson);
                }
                else if (!string.IsNullOrEmpty(parentIdsFilter.Value))
                {
                    deserialized = JsonSerializer.Deserialize<string[]>(parentIdsFilter.Value);
                }

                if (deserialized.Length > 0)
                {
                    var parentFilter = new FilterClause()
                    {
                        FieldName = "ParentId",
                        FieldValue = deserialized,
                        Operator = FilterClause.Operators.ContainsOr
                    };
                    allItemsFilter.ChildFilters.Add(parentFilter);
                }

                object filterValue = allItemsFilter;

                propsToRead.TryGetValue("FilterByParentUrl", out string filterByParentUrlString);
                var filterByCurrentParent = filterByParentUrlString == "True";
                var selectedItemsValue = GetMixedContentValue(filterValue, contentType, contentProvider, filterByCurrentParent);
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

    private static void AddToChildFilters(CombinedFilter allItemsFilter, object filter, bool addInInnerGroup = false)
    {
        var groupParentFilter = new CombinedFilter();
        groupParentFilter.Operator = CombinedFilter.LogicalOperators.And;
        groupParentFilter.ChildFilters = [filter];

        if (allItemsFilter.ChildFilters.Count == 0)
        {
            allItemsFilter.ChildFilters.Add(groupParentFilter);
        }
        else if (addInInnerGroup)
        {
            (allItemsFilter.ChildFilters.Last() as CombinedFilter).ChildFilters.Add(filter);
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
        if (query.Value != null && query.Value.StartsWith(daysString, StringComparison.Ordinal))
        {
            var substringValue = query.Value.Substring(daysString.Length + 1).Trim('(').Trim(')');
            if (double.TryParse(substringValue, out double days))
            {
                var dateFilter = new DateOffsetPeriod()
                {
                    DateFieldName = query.Condition.FieldName,
                    OffsetType = DateOffsetType.Days,
                    OffsetValue = (int)Math.Abs(days),
                };

                allItemsFilter.ChildFilters.Add(dateFilter);
            }
        }
        else if (query.Value != null && query.Value.StartsWith(monthsString, StringComparison.Ordinal))
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
                allItemsFilter.ChildFilters.Add(dateFilter);
            }
        }
        else if (query.Value != null && query.Value.StartsWith(yearsString, StringComparison.Ordinal))
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
                allItemsFilter.ChildFilters.Add(dateFilter);
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
                AddToChildFilters(allItemsFilter, childFilter, true);
            }
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
