using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class LanguageSelectorWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass"];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, new Dictionary<string, string>());

        if (propsToRead.TryGetValue("MissingTranslationAction", out string missingTranslationAction))
        {
            if (missingTranslationAction == "RedirectToPage")
            {
                migratedProperties["LanguageSelectorLinkAction"] = "RedirectToHomePage";
            }
            else
            {
                migratedProperties["LanguageSelectorLinkAction"] = "HideLink";
            }
        }

        return Task.FromResult(new MigratedWidget("SitefinityLanguageSelector", migratedProperties));
    }
}
