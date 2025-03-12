using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1031

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class LoginWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propertiesToRename = new Dictionary<string, string>
        {
            { "MembershipProvider", "MembershipProviderName" },
            { "UsernameLabel", "EmailLabel" },
            { "LoginButtonLabel", "SubmitButtonLabel" },
            { "LoginHeadingLabel", "Header" },
            { "IncorrectLoginMessage", "ErrorMessage" },
            { "RegisterUserLabel", "RegisterLinkText" },
        };

        var propertiesToCopy = new[] { "PasswordLabel", "CssClass" };

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, propertiesToRename);
        migratedProperties["PostLoginAction"] = "StayOnSamePage";

        if (context.Source.Properties.TryGetValue("DestinationPageUrl", out string destinationPageUrl))
        {
            try
            {
                var pageModel = await context.SourceClient.LayoutService().GetPageModel(destinationPageUrl.TrimStart('~'));
                var pageWithMixedValue = await GetSingleItemMixedContentValue(context, new string[] { pageModel.Id.ToString() }, RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("PostLoginRedirectPage", pageWithMixedValue);
                migratedProperties["PostLoginAction"] = "RedirectToPage";
            }
            catch (System.Exception)
            {
                await context.LogWarning($"Could not find post login page with url '{destinationPageUrl}'.");
            }
        }

        if (context.Source.Properties.TryGetValue("RegisterUserPageUrl", out string registrationPageUrl))
        {
            try
            {
                var pageModel = await context.SourceClient.LayoutService().GetPageModel(registrationPageUrl.TrimStart('~'));
                var pageWithMixedValue = await GetSingleItemMixedContentValue(context, new string[] { pageModel.Id.ToString() }, RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("RegistrationPage", pageWithMixedValue);
            }
            catch (System.Exception)
            {
                await context.LogWarning($"Could not find registration page with url '{registrationPageUrl}'.");
            }
        }

        if (context.Source.Properties.TryGetValue("ChangePasswordPageUrl", out string changePasswordPage))
        {
            try
            {
                var pageModel = await context.SourceClient.LayoutService().GetPageModel(changePasswordPage.TrimStart('~'));
                var pageWithMixedValue = await GetSingleItemMixedContentValue(context, new string[] { pageModel.Id.ToString() }, RestClientContentTypes.Pages, null, false);
                migratedProperties.Add("ResetPasswordPage", pageWithMixedValue);

                await context.LogWarning("Change password page setting moved to reset password page.");
            }
            catch (System.Exception)
            {
                await context.LogWarning($"Could not find change passowrd page with url '{changePasswordPage}'.");
            }
        }


        return new MigratedWidget("SitefinityLoginForm", migratedProperties);
    }
}

#pragma warning restore CA1031
