using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class NavigationWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["SelectionMode", "ShowParentPage"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "CssClass", "WrapperCssClass" }
    };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);
        migratedProperties["LevelsToInclude"] = null;

        if (propsToRead.TryGetValue("LevelsToInclude", out string levelsToInclude))
        {
            if (int.TryParse(levelsToInclude, out int levelsToIncludeInt) && levelsToIncludeInt >= 0)
            {
                migratedProperties["LevelsToInclude"] = levelsToIncludeInt.ToString(CultureInfo.InvariantCulture);
            }
        }

        if (propsToRead.TryGetValue("SelectedPageId", out string value) && Guid.TryParse(value, out _))
        {
            var mixedContentValue = await GetSingleItemMixedContentValue(context, [value], RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("SelectedPage", mixedContentValue);
        }

        if (propsToRead.TryGetValue("SerializedSelectedPages", out string customSelectedPagesString) && !string.IsNullOrEmpty(customSelectedPagesString))
        {
            var selectedPagesParsed = JsonSerializer.Deserialize<SerializedSelectedPage[]>(customSelectedPagesString);
            var filteredPageIds = new List<string>(selectedPagesParsed.Length);
            var otherPages = new List<SerializedSelectedPage>(selectedPagesParsed.Length);
            foreach (var page in selectedPagesParsed)
            {
                if (!page.IsExternal || page.UrlName != null)
                {
                    filteredPageIds.Add(page.Id);
                }
                else
                {
                    otherPages.Add(page);
                }
            }

            if (otherPages.Count > 0)
            {
                var titleUrlMap = string.Join(',', otherPages.Select(p => $"{p.UrlName ?? p.Id}"));
                var widgetTitle = $"{context.Source.Caption ?? context.Source.Name} ({context.Source.Id})";
                await context.LogWarning($"Migration of external pages is not supported. Affected widget -> {widgetTitle}. Affected pages -> {titleUrlMap}. A possible workaround is to make redirect pages in the page sitemap structure.");
            }

            if (filteredPageIds.Count > 0)
            {
                var mixedContentValue = await GetMixedContentValue(context, filteredPageIds.ToArray(), RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("CustomSelectedPages", mixedContentValue);
            }

        }

        await TryMigrateViewName(context, migratedProperties);

        return new MigratedWidget("SitefinityNavigation", migratedProperties);
    }

    private static async Task TryMigrateViewName(WidgetMigrationContext context, IDictionary<string, string> properties)
    {
        string mappedView = null;
        if (context.Source.Properties.TryGetValue("TemplateName", out string viewName))
        {
            var knownTemplatesNames = context.Framework == Core.RendererFramework.NetCore? new string[] { "Horizontal", "Tabs", "Vertical" }: new string[] { "Horizontal", "Tabs", "Vertical", "Accordion" };
            if (knownTemplatesNames.Contains(viewName))
            {
                mappedView = viewName;
            }
        }

        if (mappedView == null)
        {
            mappedView = "Horizontal";
            await context.LogWarning($"Failed to map view name for widget {context.Source.Caption ?? context.Source.Name}({context.Source.Id}). Defaulted to 'Horizontal'.");
        }

        properties.Add("SfViewName", mappedView);
    }

    private class SerializedSelectedPage
    {
        public string Id { get; set; }

        public bool IsExternal { get; set; }

        public string UrlName { get; set; }
    }
}
