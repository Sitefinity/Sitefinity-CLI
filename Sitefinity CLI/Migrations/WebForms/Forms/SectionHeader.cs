using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WebForms.Forms;
internal class SectionHeader : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "WrapperTag", "TagName" },
        { "Title", "Content" },
        { "CssClass", "WrapperCssClass" }
    };

    public override string FieldType => "SectionHeader";

    public override Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var migratedProperties = ProcessProperties(context.Source.Properties, null, propertiesToRename);

        return Task.FromResult(new MigratedWidget("SitefinityContentBlock", migratedProperties));
    }
}
