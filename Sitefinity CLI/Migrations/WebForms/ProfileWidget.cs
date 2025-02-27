using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class ProfileWidget : MigrationBase, IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propertiesToCopy = new string[]
        {
            "CssClass"
        };

        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, null);

        if (context.Source.Properties.TryGetValue("ProfileViewMode", out string profileViewMode))
        {
            var mappedProfileViewModel = profileViewMode;
            switch (profileViewMode)
            {
                case "Write":
                    mappedProfileViewModel = "Edit";
                    break;
                default:
                    mappedProfileViewModel = profileViewMode;
                    break;
            }

            var readWriteModeFlag = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("Read-ShowAdditionalModesLinks", System.StringComparison.Ordinal));
            if (readWriteModeFlag.Key != null && bool.TryParse(readWriteModeFlag.Value, out bool result) && result)
            {
                mappedProfileViewModel = "ReadEdit";
            }

            migratedProperties.Add("ProfileViewMode", mappedProfileViewModel);
            migratedProperties.Add("ViewMode", mappedProfileViewModel);

            var redirectPageId = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("Write-RedirectOnSubmitPageId", System.StringComparison.Ordinal));
            if (redirectPageId.Key != null)
            {
                var redirectPageInEditMode = await GetSingleItemMixedContentValue(context, [redirectPageId.Value], RestClientContentTypes.Pages, null, false);
                var migratedPropertyName = "EditModeRedirectPage";
                if (mappedProfileViewModel == "ReadWrite")
                {
                    migratedPropertyName = "ReadEditModeRedirectPage";
                }

                migratedProperties.Add(migratedPropertyName, redirectPageInEditMode);
            }

            if (mappedProfileViewModel == "ReadEdit" || mappedProfileViewModel == "Edit")
            {
                string propertyActionName = null;
                switch (mappedProfileViewModel)
                {
                    case "ReadEdit":
                        propertyActionName = "ReadEditModeAction";
                        break;
                    case "Edit":
                        propertyActionName = "EditModeAction";
                        break;
                }

                var submitUserProfileSuccessAction = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("Write-SubmittingUserProfileSuccessAction", System.StringComparison.Ordinal));
                if (submitUserProfileSuccessAction.Key != null)
                {
                    switch (submitUserProfileSuccessAction.Value)
                    {
                        case "ShowMessage":
                            migratedProperties.Add(propertyActionName, "ViewMessage");
                            // Write-SubmitSuccessMessage
                            var successMessage = context.Source.Properties.FirstOrDefault(x => x.Key.EndsWith("Write-SubmitSuccessMessage", System.StringComparison.Ordinal));
                            if (successMessage.Key != null)
                            {
                                migratedProperties.Add("SuccessNotification", successMessage.Value);
                            }
                            break;
                        default:
                            migratedProperties.Add(propertyActionName, submitUserProfileSuccessAction.Value);
                            break;
                    }
                }
            }
        }

        return new MigratedWidget("SitefinityProfile", migratedProperties);
    }
}
