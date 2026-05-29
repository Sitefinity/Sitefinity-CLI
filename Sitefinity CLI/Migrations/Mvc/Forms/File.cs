using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc.Forms;
internal class File : FormMigrationBase
{
    protected static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "MetaField-Title", "Label" },
        { "MetaField-Description", "InstructionalText" },
        { "MetaField-DefaultValue", "PredefinedValue" },
        { "Model-AllowMultipleFiles", "AllowMultipleFiles" },
        { "ValidatorDefinition-RequiredViolationMessage", "RequiredErrorMessage" },
        { "ValidatorDefinition-Required", "Required" },
        { "Model-FileSizeViolationMessage", "FileSizeViolationMessage" },
        { "Model-FileTypeViolationMessage", "FileTypeViolationMessage" },
        { "Model-Hidden", "Hidden" },
        { "Model-CssClass", "CssClass" },
    };

    public override string FieldType => "File";

    public override async Task<MigratedWidget> MigrateFormWidget(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties;

        var migratedProperties = ProcessProperties(propsToRead, null, propertiesToRename);
        ConfigureFileSizeRange(propsToRead, migratedProperties);
        ConfigureAllowedTypes(propsToRead, migratedProperties);

        return await Task.FromResult(new MigratedWidget("SitefinityFileField", migratedProperties));
    }

    private static void ConfigureAllowedTypes(IDictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        if (propsToRead.TryGetValue("Model-AllowedFileTypes", out string allowFileTypesString) && allowFileTypesString != "All")
        {
            string other = string.Empty;

            if (propsToRead.TryGetValue("Model-OtherFileTypes", out string otherFileTypes) && !string.IsNullOrEmpty(otherFileTypes))
            {
                other = otherFileTypes.Trim(';').Replace(';', ',');
            }

            migratedProperties.Add("FileTypes", JsonSerializer.Serialize(new {Type = allowFileTypesString, Other = other}));
        }
    }

    private static void ConfigureFileSizeRange(IDictionary<string, string> propsToRead, IDictionary<string, string> migratedProperties)
    {
        int? minFileSize = null;
        int? maxFileSize = null;
        if (propsToRead.TryGetValue("Model-MaxFileSizeInMb", out string maxFileSizeString) && int.TryParse(maxFileSizeString, out int maxFileSizeInt))
        {
            maxFileSize = maxFileSizeInt;
        }

        if (propsToRead.TryGetValue("Model-MinFileSizeInMb", out string minFileSizeString) && int.TryParse(minFileSizeString, out int minFileSizeInt))
        {
            minFileSize = minFileSizeInt;
        }

        migratedProperties.Add("Range", JsonSerializer.Serialize(new {Min = minFileSize, Max = maxFileSize}));
    }
}
