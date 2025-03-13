using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class NavigationWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["SelectionMode", "ShowParentPage", "LevelsToInclude"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "CssClass", "WrapperCssClass" },
    };
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);

        if (!migratedProperties.ContainsKey("LevelsToInclude"))
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
                var widgetTitle = $"{context.Source.Caption ?? context.Source.Name} ({context.Source.Id})";
                await context.LogWarning($"Migration of external pages is not supported. Affected widget -> {widgetTitle}. Affected pages -> {titleUrlMap}. A possible workaround is to make redirect pages in the page sitemap structure.");
            }

            if (filteredPageIds.Count > 0)
            {
                var mixedContentValue = await GetMixedContentValue(context, filteredPageIds.ToArray(), RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("CustomSelectedPages", mixedContentValue);
            }
        }

        await MigrateViewName(context, migratedProperties);

        return new MigratedWidget("SitefinityNavigation", migratedProperties);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "")]
    private static async Task MigrateViewName(WidgetMigrationContext context, IDictionary<string, string> migratedProperties)
    {
        string mappedView = null;
        try
        {
            context.Source.Properties.TryGetValue("ViewName", out string viewName);
            if (string.IsNullOrEmpty(viewName))
            {
                if (context.Source.Properties.TryGetValue("TemplateKey", out string templateKey) && !string.IsNullOrEmpty(templateKey))
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("X-SF-Access-Key", context.SourceCmsToken);
                        var responseMessage = await httpClient.GetAsync($"{context.SourceCmsUrl}/Sitefinity/Services/ControlTemplates/ControlTemplateService.svc/{templateKey}/");
                        var responseAsString = await responseMessage.Content.ReadAsStringAsync();
                        var controlTemplate = JsonSerializer.Deserialize<ControlTemplateDto>(responseAsString);
                        viewName = controlTemplate.Item.EmbeddedTemplateName;
                    }
                }
            }

            if (!string.IsNullOrEmpty(viewName))
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
        }
        catch (Exception) { }

        if (mappedView == null)
        {
            mappedView = "Horizontal";
            await context.LogWarning($"Failed to map view name for widget {context.Source.Caption ?? context.Source.Name}({context.Source.Id}). Defaulted to 'Horizontal'.");
        }

        migratedProperties.Add("SfViewName", mappedView);
    }

    public class SelectedPage
    {
        public string Title { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public bool IsExternal { get; set; }
    }

    public class ControlTemplateDto
    {
        public ControlTemplateItemDto Item { get; set; }
    }

    public class ControlTemplateItemDto
    {
        public string EmbeddedTemplateName { get; set; }
    }
}
