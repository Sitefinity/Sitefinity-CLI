using System;
using System.Threading.Tasks;
using System.Web;
using Progress.Sitefinity.Clients.LayoutService.Dto;
using Progress.Sitefinity.MigrationTool.Core;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Exceptions;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class FormWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass", "SuggestionFields" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("FormId", out string formId) && Guid.TryParse(formId, out _))
        {
            var migratedFormMap = await Migrator.MigrateForms(new FormMigrationArgs([formId], context.SourceCmsUrl, context.SourceCmsToken, context.WidgetsMigrationMap ?? WidgetMigrationDefaults.MigrationMap, WidgetMigrationDefaults.CustomFormMigrations)
            {
                Recreate = false,
                SiteId = context.SiteId,
                Framework = context.Framework
            });

            if (migratedFormMap.TryGetValue(formId, out string migratedFormId))
            {
                var selectedItemsValueForDetailsPage = await GetSingleItemMixedContentValue(context, [migratedFormId], RestClientContentTypes.Forms, null, false);
                migratedProperties.Add("SelectedItems", selectedItemsValueForDetailsPage);
            }
            else
            {
                await context.LogWarning($"Form with ID '{formId}' was not migrated for the Form widget. Most probable reason is that the original form was deleted or unpublished.");
            }
        }

        if (context.Source.Properties.TryGetValue("SubmitAction", out string submitAction))
        {
            if (submitAction == "TextMessage")
            {
                migratedProperties.Add("FormSubmitAction", "Message");
            }
            else if (submitAction == "PageRedirect")
            {
                migratedProperties.Add("FormSubmitAction", "Redirect");
            }
        }

        if (context.Source.Properties.TryGetValue("RedirectPageUrl", out string redirectPageUrl) && !string.IsNullOrEmpty(redirectPageUrl))
        {
            redirectPageUrl = redirectPageUrl.TrimStart('~');
            try
            {
                var resultModel = await context.SourceClient.ExecuteBoundFunction<PageModelDto>(new BoundFunctionArgs()
                {
                    Type = RestClientContentTypes.Pages,
                    Name = $"Default.Model(url=@param)?@param='{HttpUtility.UrlEncode(redirectPageUrl)}'",
                });

                if (resultModel != null)
                {
                    var redirectPageValue = await GetSingleItemMixedContentValue(context, [resultModel.Id.ToString()], RestClientContentTypes.Pages, null, false);
                    migratedProperties.Add("RedirectPage", redirectPageValue);
                }
            }
            catch (ErrorCodeException err)
            {
                if (err.Code == "NotFound")
                {
                    await context.LogWarning($"Page with URL {redirectPageUrl} not found. Switching to submit action TextMessage. As an alternative set the redirect URL through the Forms' 'Title & Properties' screen");
                    if (migratedProperties.TryGetValue("FormSubmitAction", out string formSubmitAction) && formSubmitAction == "Redirect")
                    {
                        migratedProperties.Remove("FormSubmitAction");
                    }
                }
            }
        }

        if (context.Source.Properties.TryGetValue("SuccessMessage", out string successMessage))
        {
            migratedProperties.Add("SuccessMessage", successMessage);
        }

        return new MigratedWidget("SitefinityForm", migratedProperties);
    }
}
