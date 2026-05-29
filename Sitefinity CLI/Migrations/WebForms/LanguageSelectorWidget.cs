using System.Collections.Generic;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms;
internal class LanguageSelectorWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["CssClass"];

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, propertiesToCopy, new Dictionary<string, string>());

        if (context.Source.Properties.TryGetValue("MissingTranslationAction", out string missingTranslationAction))
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
