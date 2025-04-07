using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class RegistrationWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass"];
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, null);

        if(propsToRead.TryGetValue("SuccessfulRegistrationAction", out string registrationAction))
        {
            string postRegistrationAction = "RedirectToPage";
            if (registrationAction == "ShowMessage")
            {
                postRegistrationAction = "ViewMessage";
            }

            migratedProperties["PostRegistrationAction"] = postRegistrationAction;
        }

        if (propsToRead.TryGetValue("LoginPageId", out string loginPageId) && Guid.TryParse(loginPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [loginPageId], RestClientContentTypes.Pages, null, false);
            migratedProperties["LoginPage"] = mixedContentContext;
        }

        if (propsToRead.TryGetValue("SerializedExternalProviders", out string serializedExternalProviders) && !string.IsNullOrEmpty(serializedExternalProviders))
        {
            var externalProviders = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedExternalProviders);
            var externalProvidersToPersist = externalProviders.Select(x => x.Key).ToArray();
            migratedProperties["ExternalProviders"] = JsonSerializer.Serialize(externalProvidersToPersist);
        }

        if (propsToRead.TryGetValue("SuccessfulRegistrationPageId", out string successfulRegistrationPageId) && Guid.TryParse(successfulRegistrationPageId, out _))
        {
            var mixedContentContext = await GetSingleItemMixedContentValue(context, [successfulRegistrationPageId], RestClientContentTypes.Pages, null, false);
            migratedProperties["PostRegistrationRedirectPage"] = mixedContentContext;
        }

        return new MigratedWidget("SitefinityRegistration", migratedProperties);
    }
}
