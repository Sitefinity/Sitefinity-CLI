using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
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
        else if (context.Source.ViewName == "4-columns")
        {
            migratedProperties.Add("ColumnsCount", "4");
            migratedProperties.Add("ColumnProportionsInfo", "[3,3,3,3]");
        }
        else if (context.Source.ViewName == "5-columns")
        {
            migratedProperties.Add("ColumnsCount", "5");
            migratedProperties.Add("ColumnProportionsInfo", "[3,2,3,2,3]");
        }
        else
        {
            var columnCssList = context.Source.Properties.Where(p => p.Key.StartsWith("Column", StringComparison.OrdinalIgnoreCase) && p.Key.EndsWith("_Css", StringComparison.OrdinalIgnoreCase)).ToList();
            if (columnCssList.Count > 0)
            {
                context.Source.Properties.TryGetValue("Row_Css", out string rowCss);
                migratedProperties.Add("ColumnsCount", columnCssList.Count.ToString(CultureInfo.InvariantCulture));
                var proportionsmap = new Dictionary<string, string>() {
                { "grid-2+3+2+3+2", "[2,3,2,3,2]" },
                { "grid-3+3+3+3", "[3,3,3,3]" },
                { "grid-3+6+3", "[3,6,3]" },
                { "grid-3+9", "[3,9]" },
                { "grid-4+4+4", "[4,4,4]" },
                { "grid-4+8", "[4,8]" },
                { "grid-6+6", "[6,6]" },
                { "grid-8+4", "[8,4]" },
                { "grid-9+3", "[9,3]" },
                { "grid-12", "[12]" },
            };
                var proportions = proportionsmap.FirstOrDefault(p => context.Source.Properties["Layout"].Contains(p.Key, StringComparison.InvariantCulture)).Value;
                if (proportions == null)
                {
                    proportions = JsonSerializer.Serialize(Enumerable.Repeat<int>(0, columnCssList.Count));
                }

                migratedProperties.Add("ColumnProportionsInfo", proportions);
                var cssClasses = new Dictionary<string, object>()
            {
                { "Section", new { Class = rowCss} },
            };
                foreach (var column in columnCssList)
                {
                    cssClasses.Add(column.Key.Replace("_Css", string.Empty, StringComparison.InvariantCulture), new { Class = column.Value });
                }

                migratedProperties.Add("CustomCssClass", JsonSerializer.Serialize(cssClasses));

                PopulateLabels(context, migratedProperties);
            }
        }
        var layout = context.Source.Properties["Layout"];
        if (layout != null && layout.Contains("Column1TemplateHeader", StringComparison.OrdinalIgnoreCase))
        {
            var labels = new Dictionary<string, object>()
            {
                { "Column1", new { Label = "Header"} },
            };

            migratedProperties.Add("Labels", JsonSerializer.Serialize(labels));
        }
        else if (layout != null && layout.Contains("Column1TemplateFooter", StringComparison.OrdinalIgnoreCase))
        {
            var labels = new Dictionary<string, object>()
            {
                { "Column1", new { Label = "Footer"} },
            };

            migratedProperties.Add("Labels", JsonSerializer.Serialize(labels));
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

    private static void PopulateLabels(WidgetMigrationContext context, Dictionary<string, string> migratedProperties)
    {
        context.Source.Properties.TryGetValue("Row_Label", out string rowLabel);
        var columnLabels = context.Source.Properties.Where(p => p.Key.StartsWith("Column", StringComparison.OrdinalIgnoreCase) && p.Key.EndsWith("_Label", StringComparison.OrdinalIgnoreCase)).ToList();
        var labels = new Dictionary<string, object>()
            {
                { "Section", new { Label = rowLabel} },
            };
        foreach (var column in columnLabels)
        {
            labels.Add(column.Key.Replace("_Label", string.Empty, StringComparison.InvariantCulture), new { Label = column.Value });
        }

        migratedProperties.Add("Labels", JsonSerializer.Serialize(labels));
    }
}
