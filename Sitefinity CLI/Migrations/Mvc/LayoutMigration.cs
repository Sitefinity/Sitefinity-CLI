using Progress.Sitefinity.MigrationTool.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;

internal class LayoutMigration : IWidgetMigration
{
    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        string migratedName = "SitefinitySection";
        string migratedPlaceholder = context.Source.PlaceHolder;
        var migratedProperties = context.Source.Properties.ToDictionary(x => x.Key, x => x.Value);
        if (context.ParentId == null)
        {
            migratedPlaceholder = "Body";
        }

        await MigrateColumnProportions(context, migratedProperties);
        MigrateEditedMvcGridWidget(context, migratedProperties);
        AddWebFormsPredefinedLabels(context, migratedProperties);

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

        return new MigratedWidget(migratedName, migratedProperties);
    }

    private static void AddWebFormsPredefinedLabels(WidgetMigrationContext context, Dictionary<string, string> migratedProperties)
    {
        var layout = context.Source.Properties["Layout"];
        if (layout == null)
            return;
        var labels = new Dictionary<string, object>();
        if (layout.Contains("Column1TemplateHeader", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Header" });
        }
        else if (layout.Contains("Column1TemplateFooter", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Footer"});
        }
        else if (layout.Contains("Labeled.Column1Template", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Content" });
        }
        else if (layout.Contains("Labeled.Column2Template1", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Left Sidebar" });
            labels.Add("Column2", new { Label = "Content" });
        }
        else if (layout.Contains("Labeled.Column2Template3", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Column 1" });
            labels.Add("Column2", new { Label = "Column 2" });
        }
        else if (layout.Contains("Labeled.Column2Template5", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Content" });
            labels.Add("Column2", new { Label = "Right Sidebar" });
        }
        else if (layout.Contains("Labeled.Column3Template1", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Column 1" });
            labels.Add("Column2", new { Label = "Column 2" });
            labels.Add("Column3", new { Label = "Column 3" });
        }
        else if (layout.Contains("Labeled.Column3Template2", StringComparison.OrdinalIgnoreCase))
        {
            labels.Add("Column1", new { Label = "Left Sidebar" });
            labels.Add("Column2", new { Label = "Content" });
            labels.Add("Column3", new { Label = "Right Sidebar" });
        }

        if (labels.Count > 0)
        {
            migratedProperties.Add("Labels", JsonSerializer.Serialize(labels));
        }
    }

    private static async Task MigrateColumnProportions(WidgetMigrationContext context, Dictionary<string, string> migratedProperties)
    {
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

        var proportions = MvcProportionsMap.FirstOrDefault(p => context.Source.Properties["Layout"].Contains(p.Key, StringComparison.InvariantCulture)).Value;

        if (proportions != null) //get defaut proportions for layout/grid template
        {
            migratedProperties["ColumnProportionsInfo"] = proportions;
        }
        else //calculate proportions from css classes
        {
            var calculatedProportions = new List<int>();
            var columnCssList = context.Source.Properties.Where(p => p.Key.StartsWith("Column", StringComparison.OrdinalIgnoreCase) && p.Key.EndsWith("_Css", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var column in columnCssList)
            {
                var match = Regex.Match(column.Value, "col-lg-(.+) (.*)");
                if (!match.Success)
                {
                    match = Regex.Match(column.Value, "col-lg-(.+)");
                }
                if (match.Success && int.TryParse(match.Groups[1].Value, out int colSize))
                {
                    calculatedProportions.Add(colSize);
                }
            }

            if (columnCssList.Count!= 0 && calculatedProportions.Count == columnCssList.Count)
            {
                migratedProperties["ColumnProportionsInfo"] = JsonSerializer.Serialize(calculatedProportions);
            }
            else
            {
                await context.LogWarning($"The layout widget {context.Source.ViewName} may not have the correct styles and proportions");
            }
        }
    }

    private static void MigrateEditedMvcGridWidget(WidgetMigrationContext context, Dictionary<string, string> migratedProperties)
    {
        var columnCssList = context.Source.Properties.Where(p => p.Key.StartsWith("Column", StringComparison.OrdinalIgnoreCase) && p.Key.EndsWith("_Css", StringComparison.OrdinalIgnoreCase)).ToList();
        if (columnCssList.Count == 0)
            return;

        context.Source.Properties.TryGetValue("Row_Css", out string rowCss);
        migratedProperties["ColumnsCount"] = columnCssList.Count.ToString(CultureInfo.InvariantCulture);
        
        var cssClasses = new Dictionary<string, object>()
            {
                { "Section", new { Class = rowCss} },
            };

        var proportions = new List<int>();
        foreach (var column in columnCssList)
        {
            cssClasses.Add(column.Key.Replace("_Css", string.Empty, StringComparison.InvariantCulture), new { Class = column.Value });
        }

        migratedProperties.Add("CustomCssClass", JsonSerializer.Serialize(cssClasses));

        PopulateMvcColumnLabels(context, migratedProperties);
    }

    private static void PopulateMvcColumnLabels(WidgetMigrationContext context, Dictionary<string, string> migratedProperties)
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

    private static Dictionary<string, string> MvcProportionsMap = new Dictionary<string, string>() {
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
        { "container", "[12]" },
        {"Column1TemplateHeader", "[12]" },
        {"Column1TemplateFooter", "[12]" },
        { "Column1Template.ascx" , "[12]"},
        { "Column2Template1.ascx", "[3,9]" },
        { "Column2Template2.ascx", "[4,8]" },
        { "Column2Template3.ascx", "[6,6]" },
        { "Column2Template4.ascx", "[8,4]" },
        { "Column2Template5.ascx", "[9,3]" },
        { "Column3Template1.ascx", "[4,4,4]" },
        { "Column3Template2.ascx", "[3,6,3]" },
        { "Column4Template1.ascx", "[3,3,3,3]" },
        { "Column5Template1.ascx", "[2,3,2,3,2]" },
    };

}
