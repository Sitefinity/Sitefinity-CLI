using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1031

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class RegistrationWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propertiesToRename = new Dictionary<string, string>
        {
            { "ConfirmationText", "SuccessLabel" }
        };

        var propertiesToCopy = new[] { "CssClass" };

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        if (context.Source.Properties.TryGetValue("RegistratingUserSuccessAction", out string registratingUserSuccessAction))
        {
            string correspondingAction = null;
            switch (registratingUserSuccessAction)
            {
                case "ShowMessage":
                    correspondingAction = "ViewMessage";
                    break;
                default:
                    correspondingAction = registratingUserSuccessAction;
                    break;
            }

            migratedProperties.Add("PostRegistrationAction", correspondingAction);
        }

        if (context.Source.Properties.TryGetValue("RedirectOnSubmitPageId", out string redirectPageIdString) && Guid.TryParse(redirectPageIdString, out var redirectPageId))
        {
            var pageContentValue = await GetSingleItemMixedContentValue(context, new string[] { redirectPageId.ToString() }, RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("PostRegistrationRedirectPage", pageContentValue);
        }

        await context.LogWarning("Registration form settings for account activation are now centralized in Sitefinity/Administration/Settings/Advanced -> User Registration Settings.");
        return new MigratedWidget("SitefinityRegistration", migratedProperties);
    }
}

#pragma warning restore CA1031
