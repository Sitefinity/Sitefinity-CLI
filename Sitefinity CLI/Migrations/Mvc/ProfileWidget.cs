using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class ProfileWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "MembershipProvider", "MembershipProviderName" },
        { "ShowRememberMe", "RememberMe" },
    };
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        if (propsToRead.TryGetValue("Mode", out string mode))
        {
            string modeToPersist = string.Empty;
            if (mode == "EditOnly")
            {
                modeToPersist = "Edit";
            }
            else if (mode == "ReadOnly")
            {
                modeToPersist = "Read";
            }
            else if (mode == "Both")
            {
                modeToPersist = "ReadEdit";
            }

            migratedProperties["ViewMode"] = modeToPersist;
        }

        if (propsToRead.TryGetValue("SaveChangesAction", out string saveChangesAction))
        {
            var readEditModeAction = "SwitchToReadMode";
            if (saveChangesAction == "ShowMessage")
            {
                readEditModeAction = "ViewMessage";
            }
            else if (saveChangesAction == "ShowPage")
            {
                readEditModeAction = "RedirectToPage";
            }

            if (mode == "EditOnly")
            {
                migratedProperties["EditModeAction"] = readEditModeAction;
            }
            else if (mode == "Both")
            {
                migratedProperties["ReadEditModeAction"] = readEditModeAction;
            }
        }

        if (propsToRead.TryGetValue("ProfileSavedPageId", out string redirectPageId) && Guid.TryParse(redirectPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [redirectPageId], RestClientContentTypes.Pages, null, false);
            if (mode == "EditOnly")
            {
                migratedProperties["EditModeRedirectPage"] = mixedContentContext;
            }
            else if (mode == "Both")
            {
                migratedProperties["ReadEditModeRedirectPage"] = mixedContentContext;
            }
        }

        return new MigratedWidget("SitefinityProfile", migratedProperties);
    }
}
