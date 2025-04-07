using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class NativeChatWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = ["BotId", "Nickname", "BotAvatar", "Proactive", "UserMessage", "ConversationId", "ChatMode", "OpeningChatIcon", "ClosingChatIcon", "ContainerId", "Placeholder", "ShowPickers", "LocationPickerLabel", "GoogleApiKey", "DefaultLocation", "CustomCss", "Locale"];
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "WidgetCssClass", "CssClass" },
    };

    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        return Task.FromResult(new MigratedWidget("SitefinityNativeChat", migratedProperties));
    }
}
