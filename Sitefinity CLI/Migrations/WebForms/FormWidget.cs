using Progress.Sitefinity.Clients.LayoutService.Dto;
using Progress.Sitefinity.MigrationTool.Core;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Dto;
using Progress.Sitefinity.RestSdk.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;

internal class FormWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass", "SuggestionFields" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("FormId", out string formId) && Guid.TryParse(formId, out _))
        {
            var migratedFormMap = await Migrator.MigrateForms(new FormMigrationArgs([formId], context.SourceCmsUrl, context.SourceCmsToken, null, WidgetMigrationDefaults.CustomFormMigrations)
            {
                Recreate = true,
            });

            var migratedFormId = migratedFormMap[formId];

            var selectedItemsValueForDetailsPage = await GetSingleItemMixedContentValue(context, [migratedFormId], RestClientContentTypes.Forms, null, false);
            migratedProperties.Add("SelectedItems", selectedItemsValueForDetailsPage);
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
