using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;

internal class LayoutMigration : IWidgetMigration
{
    public Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        string migratedName = "SitefinitySection";
        string migratedPlaceholder = context.Source.PlaceHolder;
        var migratedProperties = context.Source.Properties.ToDictionary(x => x.Key, x => x.Value);
        if (context.ParentId == null)
        {
            migratedPlaceholder = "Body";
        }

        if (context.Source.ViewName == "1-column")
        {
            migratedProperties.Add("ColumnProportionsInfo", "[12]");
        }
        else if (context.Source.ViewName == "2-columns")
        {
            migratedProperties.Add("ColumnsCount", "2");
            migratedProperties.Add("ColumnProportionsInfo", "[6,6]");
        }
        else if (context.Source.ViewName == "3-columns")
        {
            migratedProperties.Add("ColumnsCount", "3");
            migratedProperties.Add("ColumnProportionsInfo", "[4,4,4]");
        }

        if (context.Source.Children != null && context.Source.Children.Count > 0)
        {
            foreach (var child in context.Source.Children)
            {
                if (child.PlaceHolder == "Container")
                {
                    child.PlaceHolder = "Column1";
                }

                if (child.PlaceHolder.Contains("_Col", StringComparison.Ordinal))
                {
                    var colStringUnderscoreIndex = child.PlaceHolder.IndexOf("_Col", StringComparison.Ordinal) + 4;
                    var colIndexString = child.PlaceHolder.Substring(colStringUnderscoreIndex);

                    if (int.TryParse(colIndexString, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int result))
                    {
                        child.PlaceHolder = "Column" + (result + 1);
                    }
                }
            }
        }

        return Task.FromResult(new MigratedWidget(migratedName, migratedProperties));
    }
}
