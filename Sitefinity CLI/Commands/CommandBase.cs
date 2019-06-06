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
        [Argument(0, Description = Constants.NameArgumentDescription)]
        [Required(ErrorMessage = "You must specify the name of the resource!")]
        public string Name { get; set; }

        [Option("-r|--root", Constants.ProjectRoothPathOptionDescription, CommandOptionType.SingleValue)]
        public string ProjectRootPath { get; set; }

        public abstract string TemplateName { get; set; }

        [Option("-v|--version", Constants.VersionOptionDescription, CommandOptionType.SingleValue)]
        public string Version { get; set; }

        protected string Sign { get; set; }

        protected string CurrentPath { get; set; }

        protected string AssemblyVersion { get; set; }

        protected bool IsSitefinityProject { get; set; } = true;

        public CommandBase()
        {
            this.CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
            if (this.ProjectRootPath == null)
            {
                this.ProjectRootPath = Environment.CurrentDirectory;
            }

            var assemblyName = Path.GetFileName(this.ProjectRootPath);
            var assemblyPath = Path.Combine(this.ProjectRootPath, "bin", "Telerik.Sitefinity.dll");
            if (!File.Exists(assemblyPath))
            {
                var proceed = Prompt.GetYesNo(Constants.SitefinityNotRecognizedMessage, false, promptColor: ConsoleColor.Yellow);
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
                var promptMessage = string.Format(Constants.SourceTemplatePromptMessage, config.FullName);
                var templateNameProp = this.GetType().GetProperties().Where(prop => prop.Name == "TemplateName").FirstOrDefault();
                var defaultValueAttr = templateNameProp.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                var defaultValue = defaultValueAttr.Value.ToString();
                this.TemplateName = Prompt.GetString(promptMessage, promptColor: ConsoleColor.Yellow, defaultValue: defaultValue);
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

        protected virtual int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (File.Exists(filePath))
            {
                Utils.WriteLine(string.Format(Constants.FileExistsMessage, Path.GetFileName(filePath), filePath), ConsoleColor.Red);
                return 1;
            }

            if (!File.Exists(templatePath))
            {
                Utils.WriteLine(string.Format(Constants.TemplateNotFoundMessage, resourceFullName, templatePath), ConsoleColor.Red);
                return 1;
            }

            var templateSource = File.ReadAllText(templatePath);
            var template = Handlebars.Compile(templateSource);
            var result = template(data);

            File.WriteAllText(filePath, result);
            Utils.WriteLine(string.Format(Constants.FileCreatedMessage, Path.GetFileName(filePath), filePath), ConsoleColor.Green);
            AddFileToCsProj(filePath);

            return 0;
        }

        private string GetLatestTemplatesVersion()
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

        private void AddFileToCsProj(string filePath)
        {
            CsProjModifier csProjModifier = CreateCsProjModifier();

            if (csProjModifier != null)
            {
                csProjModifier.AddFileToCsproj(filePath);
                csProjModifier.SaveDocument();
            }
        }

        private CsProjModifier CreateCsProjModifier()
        {
            string csprojFilePath = Directory.GetFiles(this.ProjectRootPath, $"*{Constants.CsprojFileExtension}").FirstOrDefault();

            return string.IsNullOrEmpty(csprojFilePath) ? null : new CsProjModifier(csprojFilePath);
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
    }
}
