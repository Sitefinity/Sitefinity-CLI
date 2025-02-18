using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class NavigationWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        //var propertiesToRename = new Dictionary<string, string>()
        //{
        //    { "LevelsToExpand", "LevelsToInclude" },
        //    { "CssClass", "WrapperCssClass" },
        //};

        bool levelsToIncludeParsed = false;
        var propertiesToCopy = new List<string>() { "SelectionMode", "ShowParentPage", "CssClass" };
        if (context.Source.Properties.TryGetValue("LevelsToInclude", out string levelsToInclude))
        {
            if (int.TryParse(levelsToInclude, out int levelsToIncludeInt))
            {
                propertiesToCopy.Add("LevelsToInclude");
                levelsToIncludeParsed = true;
            }
        }

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy.ToArray(), null);
        if (!levelsToIncludeParsed)
        {
            migratedProperties.Add("LevelsToInclude", null);
        }

        if (context.Source.Properties.TryGetValue("StartingNodeUrl", out string value) && Guid.TryParse(value, out _))
        {
            var mixedContentValue = await GetSingleItemMixedContentValue(context, [value], RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("SelectedPage", mixedContentValue);
        }

        if (context.Source.Properties.TryGetValue("CustomSelectedPages", out string customSelectedPagesString))
        {
            var selectedPagesParsed = JsonSerializer.Deserialize<SelectedPage[]>(customSelectedPagesString);
            var filteredPageIds = new List<string>(selectedPagesParsed.Length);
            var otherPages = new List<SelectedPage>(selectedPagesParsed.Length);
            foreach (var page in selectedPagesParsed)
            {
                if (!page.IsExternal)
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
                var titleUrlMap = string.Join(',', otherPages.Select(p => $"{p.Title} ({p.Url})"));
                var widgetTitle = $"{context.Source.Caption ?? context.Source.Name} ({context.Source.Id})" ;
                await context.LogWarning($"Migration of external pages is not supported. Affected widget -> {widgetTitle}. Affected pages -> {titleUrlMap}. A possible workaround is to make redirect pages in the page sitemap structure.");
            }

            if (filteredPageIds.Count > 0)
            {
                var mixedContentValue = await GetSingleItemMixedContentValue(context, filteredPageIds.ToArray(), RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("CustomSelectedPages", mixedContentValue);
            }
        }

        return new MigratedWidget("SitefinityNavigation", migratedProperties);
    }

    private static async Task MigrateViewName(WidgetMigrationContext context, IDictionary<string, string> properties)
    {
        string mappedView = null;
        if (context.Source.Properties.TryGetValue("ViewName", out string viewName))
        {
            if (viewName.EndsWith("HorizontalList.ascx", StringComparison.Ordinal) || viewName.EndsWith("HorizontalWithDropDownMenusList.ascx", StringComparison.Ordinal))
            {
                mappedView = "Horizontal";
            }
            else if (viewName.EndsWith("HorizontalWithTabsList.ascx", StringComparison.Ordinal))
            {
                mappedView = "Tabs";
            }
            else if (viewName.EndsWith("VerticalList.ascx", StringComparison.Ordinal) || viewName.EndsWith("VerticalWithSubLevelsList.ascx", StringComparison.Ordinal))
            {
                mappedView = "Vertical";
            }
        }

        if (mappedView == null)
        {
            mappedView = "Horizontal";
            await context.LogWarning($"Failed to map view name for widget {context.Source.Caption ?? context.Source.Name}({context.Source.Id}). Defaulted to 'Horizontal'.");
        }

        properties.Add("SfViewName", mappedView);
    }

    public class SelectedPage
    {
        public string Title { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public bool IsExternal { get; set; }
    }
}
