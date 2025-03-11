using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class ChangePasswordWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass"];

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, null);

        if (propsToRead.TryGetValue("ChangePasswordCompleteAction", out string changePasswordCompleteAction))
        {
            if (changePasswordCompleteAction == "ShowMessage")
            {
                changePasswordCompleteAction = "ShowAMessage";
            }

            migratedProperties.Add("PostPasswordChangeAction", changePasswordCompleteAction);
        }

        if (propsToRead.TryGetValue("ChangePasswordRedirectPageId", out string changePasswordRedirectPageId) && Guid.TryParse(changePasswordRedirectPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [changePasswordRedirectPageId], RestClientContentTypes.Pages, null, false);
            migratedProperties.Add("PostPasswordChangeRedirectPage", mixedContentContext);
        }

        return new MigratedWidget("SitefinityChangePassword", migratedProperties);
    }
}
