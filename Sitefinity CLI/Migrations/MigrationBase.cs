using Progress.Sitefinity.RestSdk.Dto;
using Progress.Sitefinity.RestSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.ImageWidget;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using static System.Net.Mime.MediaTypeNames;
using Progress.Sitefinity.RestSdk.Filters;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Common;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;
internal abstract class MigrationBase
{
    protected string IsoFormat = "yyyy-MM-ddTHH:mm:ssZ";

    private JsonSerializerOptions jsonSerializerOptionsForContentFilterSerialization = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public MigrationBase()
    {
        this.jsonSerializerOptionsForContentFilterSerialization.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    internal static IDictionary<string, string> ProcessProperties(IDictionary<string, string> source, string[] propertiesToCopy, IDictionary<string, string> propertiesToRename)
    {
        var target = new Dictionary<string, string>();
        if (propertiesToCopy != null)
        {
            foreach (var property in propertiesToCopy)
            {
                if (source.TryGetValue(property, out string value) && value != null)
                {
                    target.Add(property, value);
                }
            }
        }

        if (propertiesToRename != null)
        {
            foreach (var property in propertiesToRename)
            {
                if (source.TryGetValue(property.Key, out string value) && value != null)
                {
                    target.Add(property.Value, value);
                }
            }
        }

        return target;
    }

    internal static async Task<string[]> GetMasterIds(WidgetMigrationContext context, string[] ids, string contentType, string providerName)
    {
        var response = await context.SourceClient.ExecuteBoundAction<ODataWrapper<List<IdPair>>>(new BoundActionArgs()
        {
            Provider = providerName,
            Name = "Default.GetMasterIds()",
            Data = new
            {
                ids = ids
            },
            Type = contentType,
        });

        return response.Value.Select(x => x.MasterId).ToArray();
    }

    internal async Task<string> GetMixedContentValue(WidgetMigrationContext context, string[] contentIds, string contentType, string contentProvider, bool convertIds)
    {
        var masterIds = contentIds;
        if (convertIds)
        {
            masterIds = await GetMasterIds(context, contentIds, contentType, contentProvider);

            // centralized hack for dynamic types
            if (masterIds.Length == 0 && contentType.StartsWith("Telerik.Sitefinity.DynamicTypes.Model", StringComparison.Ordinal))
            {
                masterIds = contentIds;
            }
        }

        var mixedContentValue = new
        {
            ItemIdsOrdered = masterIds,
            Content = new object[]
            {
                new
                {
                    Type = contentType,
                    Variations = new object[]
                    {
                        new
                        {
                            Source = contentProvider,
                            Filter = new
                            {
                                Key = "Ids",
                                Value =  string.Join( ",", masterIds) //list should be string
                            }
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(mixedContentValue, this.jsonSerializerOptionsForContentFilterSerialization);
    }

    internal async Task<string> GetSingleItemMixedContentValue(WidgetMigrationContext context, string[] contentIds, string contentType, string contentProvider, bool convertIds)
    {
        return await GetMixedContentValue(context, [contentIds[0]], contentType, contentProvider, convertIds);
    }

    internal string GetMixedContentValue(object filter, string contentType, string contentProvider, bool filterByParent = false)
    {
        var mixedContentValue = new
        {
            ItemIdsOrdered = Array.Empty<string>(),
            Content = new object[]
            {
                new
                {
                    Type = contentType,
                    Variations = new object[]
                    {
                        new
                        {
                            Source = contentProvider,
                            Filter = new
                            {
                                Key = "Complex",
                                Value = filter == null ? null : JsonSerializer.Serialize(filter, this.jsonSerializerOptionsForContentFilterSerialization)
                            },
                            DynamicFilterByParent = filterByParent
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(mixedContentValue, this.jsonSerializerOptionsForContentFilterSerialization);
    }

    internal static async Task<string> GetDefaultProvider(WidgetMigrationContext context, string contentType)
    {

        var availableProviders = await context.SourceClient.ExecuteBoundFunction<ODataWrapper<Provider[]>>(new BoundFunctionArgs() { Type = contentType, Name = "sfproviders" });
        var contentProvider = availableProviders.Value.FirstOrDefault(p => p.IsDefault)?.Name;

        return contentProvider;
    }
}
