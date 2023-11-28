using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        private string _projectRoothPath = string.Empty;

        [Argument(0, Description = Constants.NameArgumentDescription)]
        [Required(ErrorMessage = "You must specify the name of the resource!")]
        public string Name { get; set; }

        [Option("-r|--root", Constants.ProjectRoothPathOptionDescription, CommandOptionType.SingleValue)]
        public string ProjectRootPath
        {
            get
            {
                if (string.IsNullOrEmpty(this._projectRoothPath))
                {
                    this._projectRoothPath = Environment.CurrentDirectory;
                }

                return this._projectRoothPath;
            }
            set
            {
                this._projectRoothPath = value;
            }
        }

        public abstract string TemplateName { get; set; }

        [Option("-v|--version", Constants.VersionOptionDescription, CommandOptionType.SingleValue)]
        public string Version { get; set; }

        protected string Sign { get; set; }

        protected string CurrentPath { get; set; }

        protected string AssemblyVersion { get; set; }

        protected bool IsSitefinityProject { get; set; } = true;

        protected ILogger<object> Logger { get; set; }

        public CommandBase(ILogger<object> logger)
        {
            this.CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Logger = logger;

            var data = new
            {
                toolName = Constants.CLIName,
                version = this.AssemblyVersion
            };

            var templateSource = File.ReadAllText(Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, "Sign.Template"));
            var template = Handlebars.Compile(templateSource);
            this.Sign = template(data);
            Handlebars.RegisterTemplate("sign", templateSource);
        }

        public virtual int OnExecute(CommandLineApplication config)
        {
            var assemblyPath = this.GetAssemblyPath();
            if (!File.Exists(assemblyPath))
            {
                var proceed = Prompt.GetYesNo(Constants.SitefinityNotRecognizedMessage, false, promptColor: ConsoleColor.Yellow);
                if (proceed)
                {
                    this.IsSitefinityProject = false;
                }
                else
                {
                    return (int)ExitCode.GeneralError;
                }
            }

            if (this.Version == null)
            {
                var latestTemplatesVersion = this.GetLatestTemplatesVersion();
                if (!this.IsSitefinityProject)
                {
                    Utils.WriteLine(string.Format(Constants.ProducedFilesVersionMessage, latestTemplatesVersion), ConsoleColor.Yellow);
                    this.Version = latestTemplatesVersion;
                }
                else
                {
                    Assembly assmebly = Assembly.LoadFile(assemblyPath);
                    var version = assmebly.GetName().Version;
                    this.Version = string.Format("{0}.{1}", version.Major, version.Minor);

                    // if current Sitefinity version is higher than latest templates version - fallback to latest
                    if (this.Version.CompareTo(latestTemplatesVersion) == 1)
                    {
                        Utils.WriteLine(string.Format(Constants.HigherSitefinityVersionMessage, this.Version, latestTemplatesVersion), ConsoleColor.Yellow);
                        this.Version = latestTemplatesVersion;
                    }
                }
            }

            if (config.Options.First(x => x.LongName == "template").Value() == null)
            {
                var templateNameProp = this.GetType().GetProperties().Where(prop => prop.Name == "TemplateName").FirstOrDefault();
                var defaultValueAttr = templateNameProp.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                var defaultAttributeValue = defaultValueAttr.Value.ToString();

                var defaultValue = defaultAttributeValue.StartsWith("Bootstrap") ? this.GetDefaultTemplateName(this.Version) : defaultAttributeValue;

                var promptMessage = string.Format(Constants.SourceTemplatePromptMessage, config.FullName);
                this.TemplateName = Prompt.GetString(promptMessage, promptColor: ConsoleColor.Yellow, defaultValue: defaultValue);
            }

            return (int)ExitCode.OK;
        }

        protected virtual string GetAssemblyPath()
        {
            return Path.Combine(this.ProjectRootPath, "bin", "Telerik.Sitefinity.dll");
        }

        protected void AddSignToFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var fileExtension = Path.GetExtension(filePath);
            string commentedSign;
            switch (fileExtension)
            {
                case ".html":
                    commentedSign = string.Format("<!-- {0} -->{1}", this.Sign, Environment.NewLine);
                    break;
                case ".cshtml":
                    commentedSign = string.Format("@* {0} *@{1}", this.Sign, Environment.NewLine);
                    break;
                case ".cs":
                case ".js":
                default:
                    commentedSign = string.Format("/* {0} */{1}", this.Sign, Environment.NewLine);
                    break;
            }

            File.WriteAllText(filePath, string.Format("{0}{1}", commentedSign, content));
        }

        protected virtual int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (File.Exists(filePath))
            {
                this.Logger.LogError(string.Format(Constants.FileExistsMessage, Path.GetFileName(filePath), filePath));
                return (int)ExitCode.GeneralError;
            }

            if (!File.Exists(templatePath))
            {
                this.Logger.LogError(string.Format(Constants.TemplateNotFoundMessage, resourceFullName, templatePath));
                return (int)ExitCode.GeneralError;
            }

            var templateSource = File.ReadAllText(templatePath);
            var template = Handlebars.Compile(templateSource);
            var result = template(data);

            File.WriteAllText(filePath, result);
            Utils.WriteLine(string.Format(Constants.FileCreatedMessage, Path.GetFileName(filePath), filePath), ConsoleColor.Green);

            return (int)ExitCode.OK;
        }

        protected string GetLatestTemplatesVersion()
        {
            var templatesFolderPath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName);
            var directoryNames = Directory.GetDirectories(templatesFolderPath);
            List<float> versions = new List<float>();
            CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            foreach (var name in directoryNames)
            {
                float version;
                if (float.TryParse(Path.GetFileName(name), NumberStyles.Any, cultureInfo, out version))
                {
                    versions.Add(version);
                }
            }

            versions.Sort();
            return versions.Last().ToString("n1", cultureInfo);
        }

        protected IDictionary<string, string> GetTemplateData(string templatePath)
        {
            var data = new Dictionary<string, string>();
            var configPath = Path.Combine(templatePath, string.Format("{0}.config.json", this.TemplateName));
            if (File.Exists(configPath))
            {
                List<string> templateParams = new List<string>();
                using (StreamReader reader = new StreamReader(configPath))
                {
                    string content = reader.ReadToEnd();
                    templateParams = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(content);
                }

                foreach (var parameter in templateParams)
                {
                    data[parameter] = Prompt.GetString(string.Format("Please enter {0}:", parameter), promptColor: ConsoleColor.Yellow);
                }
            }

            return data;
        }

        protected virtual string GetDefaultTemplateName(string version)
        {
            CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            var versionValue = float.Parse(version, cultureInfo.NumberFormat);
            if (versionValue < 14.1)
            {
                return Constants.DefaultResourcePackageName_VersionsBefore14_1;
            }
            else
            {
                return Constants.DefaultResourcePackageName;
            }
        }
    }
}
