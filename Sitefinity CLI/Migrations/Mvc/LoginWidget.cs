using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class LoginWidget : MigrationBase, IWidgetMigration
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

        migratedProperties["PostLoginAction"] = "StayOnSamePage";
        if (propsToRead.TryGetValue("LoginRedirectPageId", out string loginRedirectPageId) && Guid.TryParse(loginRedirectPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [loginRedirectPageId], RestClientContentTypes.Pages, null, false);
            migratedProperties["PostLoginAction"] = "RedirectToPage";
            migratedProperties["PostLoginRedirectPage"] = mixedContentContext;
        }

        if (propsToRead.TryGetValue("RegisterRedirectPageId", out string registerRedirectPageId) && Guid.TryParse(registerRedirectPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [registerRedirectPageId], RestClientContentTypes.Pages, null, false);
            migratedProperties["RegistrationPage"] = mixedContentContext;
        }

        if (propsToRead.TryGetValue("SerializedExternalProviders", out string serializedExternalProviders) && !string.IsNullOrEmpty(serializedExternalProviders))
        {
            var externalProviders = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedExternalProviders);
            var externalProvidersToPersist = externalProviders.Select(x => x.Key).ToArray();
            migratedProperties["ExternalProviders"] = JsonSerializer.Serialize(externalProvidersToPersist);
        }

        return new MigratedWidget("SitefinityLoginForm", migratedProperties);
    }
}
