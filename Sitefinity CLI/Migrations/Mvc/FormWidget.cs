using System;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class FormWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass" };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("Model-FormId", out string formId) && Guid.TryParse(formId, out _))
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

        if (context.Source.Properties.TryGetValue("Model-CustomConfirmationMode", out string confirmationMode))
        {
            if (confirmationMode == "ShowMessageForSuccess")
            {
                migratedProperties.Add("FormSubmitAction", "Message");

                if (context.Source.Properties.TryGetValue("Model-CustomConfirmationMessage", out string confirmationMessage))
                {
                    migratedProperties.Add("SuccessMessage", confirmationMessage);
                }
            }
            else if (confirmationMode == "RedirectToAPage")
            {
                migratedProperties.Add("FormSubmitAction", "Redirect");

                if (context.Source.Properties.TryGetValue("Model-CustomConfirmationPageId", out string redirectPageId) && Guid.TryParse(redirectPageId, out _))
                {
                    var redirectPageValue = await GetSingleItemMixedContentValue(context, [redirectPageId], RestClientContentTypes.Pages, null, false);
                    migratedProperties.Add("RedirectPage", redirectPageValue);
                }
            }
        }

        return new MigratedWidget("SitefinityForm", migratedProperties);
    }
}
