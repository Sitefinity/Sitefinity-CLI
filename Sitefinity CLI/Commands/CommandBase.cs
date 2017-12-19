using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
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
        [Argument(0, Description = "The name of the resource to be created.")]
        [Required(ErrorMessage = "You must specify the name of the resource to be created!")]
        public string Name { get; set; }

        [Option("-r|--root", "The path to the root of the project upon the command should be executed.", CommandOptionType.SingleValue)]
        public string ProjectRootPath { get; set; }

        [Option("-t|--template", "The name of the template to be used for resource creation. Default value: " + Constants.DefaultTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultTemplateName)]
        public string TemplateName { get; set; } = Constants.DefaultTemplateName;

        [Option("-v|--version", "The template version to be used for resource creation", CommandOptionType.SingleValue)]
        public string Version { get; set; }

        protected string Sign { get; set; }

        protected string CurrentPath { get; set; }

        protected bool IsSitefinityProject { get; set; } = true;

        public CommandBase()
        {
            this.CurrentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var data = new
            {
                toolName = Constants.CLIName,
                version = Constants.Version
            };

            var templateSource = File.ReadAllText(Path.Combine(this.CurrentPath, "Templates\\Sign.Template"));
            var template = Handlebars.Compile(templateSource);
            this.Sign = template(data);
            Handlebars.RegisterTemplate("sign", templateSource);
        }

        public virtual int OnExecute(CommandLineApplication config)
        {
            if (this.ProjectRootPath == null)
            {
                this.ProjectRootPath = Environment.CurrentDirectory;
            }

            var assemblyName = Path.GetFileName(this.ProjectRootPath);
            var assemblyPath = Path.Combine(this.ProjectRootPath, "bin", "Telerik.Sitefinity.dll");
            if (!File.Exists(assemblyPath))
            {
                var proceed = Prompt.GetYesNo("Cannot recognize project as Sitefinity. Do you wish to proceed?", false);
                if (proceed)
                {
                    this.IsSitefinityProject = false;
                }
                else
                {
                    return 1;
                }
            }

            if (this.Version == null)
            {
                var latestTemplatesVersion = this.GetLatestTemplatesVersion();
                if (!this.IsSitefinityProject)
                {
                    this.WriteLine(string.Format("Templates for version {1} will be used", latestTemplatesVersion), ConsoleColor.Yellow);
                    this.Version = latestTemplatesVersion;
                    return 0;
                }

                Assembly assmebly = Assembly.LoadFile(assemblyPath);
                var version = assmebly.GetName().Version;
                this.Version = string.Format("{0}.{1}", version.Major, version.Minor);

                // if current Sitefinity version is higher than latest templates version - fallback to latest
                if (this.Version.CompareTo(latestTemplatesVersion) == 1)
                {
                    this.WriteLine(string.Format("No templates found for Sitefinity version {0}. Templates for {1} will be used", this.Version, latestTemplatesVersion), ConsoleColor.Yellow);
                    this.Version = latestTemplatesVersion;
                }
            }
            
            if (config.Options.First(x => x.LongName == Constants.OptionTemplateName).Value() == null)
            {
                this.TemplateName = Prompt.GetString("Please enter template name", promptColor: ConsoleColor.Yellow, defaultValue: Constants.DefaultTemplateName);
            }

            return 0;
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

        protected virtual int CreateFileFromTemplate(string filePath, string templatePath, object data)
        {
            if (File.Exists(filePath))
            {
                this.WriteLine(string.Format(Constants.FileExistsMessage, filePath), ConsoleColor.Red);
                return 1;
            }

            if (!File.Exists(templatePath))
            {
                this.WriteLine(string.Format(Constants.FileNotFoundMessage, templatePath), ConsoleColor.Red);
                return 1;
            }

            var templateSource = File.ReadAllText(templatePath);
            var template = Handlebars.Compile(templateSource);
            var result = template(data);

            File.WriteAllText(filePath, result);
            this.WriteLine(string.Format("File \"{0}\" created! Path: \"{1}\"", Path.GetFileName(filePath), filePath), ConsoleColor.Green);
            return 0;
        }

        private string GetLatestTemplatesVersion()
        {
            var templatesFolderPath = Path.Combine(this.CurrentPath, "Templates");
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

        protected void WriteLine(string message, ConsoleColor? foregroundColor = null)
        {
            if (foregroundColor.HasValue)
            {
                Console.ForegroundColor = foregroundColor.Value;
            }

            Console.WriteLine(message);

            if (foregroundColor.HasValue)
            {
                Console.ResetColor();
            }
        }
    }
}
