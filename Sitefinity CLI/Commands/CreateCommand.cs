using EnvDTE;
using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Commands.Validators;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.CreateCommandName, Description = "Create a new Sitefinity project.")]
    [AdminRightsValidator]
    internal class CreateCommand
    {
        [Argument(0, Description = Constants.JsonConfigDescription)]
        [Required(ErrorMessage = "You must specify a path to json configuration file.")]
        public string JsonConfig { get; set; }

        public CreateModel Config { get; set; }


        public CreateCommand(
            IPromptService promptService,
            ISitefinityPackageManager sitefinityPackageManager,
            ICsProjectFileEditor csProjectFileEditor,
            ILogger<UpgradeCommand> logger,
            IVisualStudioWorker visualStudioWorker)
        {
            this.promptService = promptService;
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.csProjectFileEditor = csProjectFileEditor;
            this.logger = logger;
            this.visualStudioWorker = visualStudioWorker;
        }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                await this.ExecuteCreate();

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);

                return 1;
            }
            finally
            {
                this.visualStudioWorker.Dispose();
            }
        }

        protected virtual async Task ExecuteCreate()
        {
            logger.LogInformation("Sitefinity Solution Create Command");
            if (!String.IsNullOrEmpty(JsonConfig))
            {
                if(!File.Exists(JsonConfig))
                {
                    throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, JsonConfig));
                }

                using (StreamReader file = File.OpenText(JsonConfig))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Config = (CreateModel)serializer.Deserialize(file, typeof(CreateModel));
                }
            }

            try
            {
                Directory.CreateDirectory(Config.TargetFolder);
            }
            catch(Exception ex)
            {
                throw ex;
            }
                
                
            
            if(!Regex.IsMatch(Config.ProjectName, @"^[^0-9][a-zA-Z0-9_.-]+$"))
            {
                throw new ArgumentException(string.Format(Constants.InvalidProjectNameMessage, Config.ProjectName));
            }

            if(!String.IsNullOrEmpty(Config.LicenseFilePath) && !File.Exists(Config.LicenseFilePath))
            {
                throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, Config.LicenseFilePath));
            }
            if(!Config.SkipDbRestore && !File.Exists(Constants.SqlPackageFile))
            {
                logger.LogWarning("Unable to locate SqlPackage.exe for the DB Restore.");
                logger.LogWarning("Setting SkipDbRestore to True.");
                Config.SkipDbRestore = true;
            }

            var packageSources = this.GetNugetPackageSources();
            string sourceFolder = GetSourceFolders(Config.ProjectTemplatePath);
            string solutionZipFile = String.Format(Constants.SolutionZipFileFormat, Config.Version);
            string solutionProjectLocation = String.Format("{0}\\{0}.csproj", Config.ProjectName);

            this.logger.LogInformation("");
            this.logger.LogInformation("Source Template     : " + Path.Join(sourceFolder, solutionZipFile));
            this.logger.LogInformation("Target Folder       : " + Config.TargetFolder);
            this.logger.LogInformation("Solution Name       : " + Config.SolutionName);
            this.logger.LogInformation("Project Name        : " + Config.ProjectName);
            this.logger.LogInformation("Skip DB Restore     : " + Config.SkipDbRestore);
            this.logger.LogInformation("Skip Confirmation   : " + Config.SkipConfirmation);
            if (!Config.SkipDbRestore)
            {
                this.logger.LogInformation("Database Name       : " + Config.DatabaseName);
                this.logger.LogInformation("Database Server     : " + Config.SqlServer);

                if(String.IsNullOrEmpty(Config.DatabaseRestoreUserName))
                {
                    this.logger.LogInformation("Database Restore As : Current User");
                }
                else
                {
                    this.logger.LogInformation("Database Restore As : " + Config.DatabaseRestoreUserName);
                    this.logger.LogInformation("Database Restore Pwd: " + Config.DatabaseRestorePassword);
                }
            }
            if(!String.IsNullOrEmpty(Config.LicenseFilePath))
            {
                this.logger.LogInformation("SF Licence          : " + Config.LicenseFilePath);
            }
            this.logger.LogInformation("Nuget Package Sources");
            foreach(var source in packageSources)
            {
                this.logger.LogInformation("      " + source);
            }

            if(!Config.SkipConfirmation)
            {
                if (!this.promptService.PromptYesNo("Does everything look correct? Ready to go?"))
                {
                    this.logger.LogInformation(Constants.CreateWasCanceled);
                    return;
                }
            }
            

            logger.LogInformation("Cleaning out all files and directories in the target folder.");
            logger.LogInformation(Config.TargetFolder);
            CleanTargetDirectory(Config.TargetFolder);
            logger.LogInformation("Completed.");

            logger.LogInformation("Extract solution zip file to target.");
            logger.LogInformation(Path.Join(sourceFolder, solutionZipFile));
            ZipFile.ExtractToDirectory(Path.Join(sourceFolder, solutionZipFile), Config.TargetFolder);

            logger.LogInformation("Processing each 'templated' file.");
            string[] templateFiles = Directory.GetFiles(Config.TargetFolder, "*.Template", SearchOption.AllDirectories);

            
            var data = new
            {
                SolutionName = Config.SolutionName,
                ProjectName = Config.ProjectName,
                DatabaseName = Config.DatabaseName,
                SqlServer = Config.SqlServer,
                SolutionProjectLocation = solutionProjectLocation
            };

            foreach (string templateFile in templateFiles)
            {
                var templateSource = File.ReadAllText(templateFile);
                var template = Handlebars.Compile(templateSource);
                var result = template(data);

                File.WriteAllText(templateFile.Replace(".Template", String.Empty), result);
                File.Delete(templateFile);

                this.logger.LogInformation("Edited : " + templateFile.Replace(".Template", String.Empty));
            }

            // Use alternate project name
            if(Config.ProjectName != Constants.DefaultName)
            {
                Directory.Move(Path.Join(Config.TargetFolder, Constants.DefaultName), Path.Join(Config.TargetFolder, Config.ProjectName));
                logger.LogInformation("Renamed project folder to " + Config.ProjectName);
                File.Move(Path.Join(Config.TargetFolder, Config.ProjectName, Constants.DefaultName + ".csproj"), Path.Join(Config.TargetFolder, Config.ProjectName, Config.ProjectName + ".csproj"));
                logger.LogInformation("Renamed project to " + Config.ProjectName + ".csproj");
            }

            // Use alternate solution name
            if (Config.SolutionName != Constants.DefaultName)
            {
                File.Move(Path.Join(Config.TargetFolder, Constants.DefaultName + ".sln"), Path.Join(Config.TargetFolder, Config.SolutionName + ".sln"));
                logger.LogInformation("Renamed solution to " + Config.SolutionName + ".sln");
            }

            // add optional license file
            if (!String.IsNullOrEmpty(Config.LicenseFilePath))
            {
                string licenseFileDestination = Path.Join(Config.TargetFolder, Config.ProjectName, "App_Data/Sitefinity/Sitefinity.lic");
                File.Copy(Config.LicenseFilePath, licenseFileDestination);

                csProjectFileEditor.AddFiles(Path.Join(Config.TargetFolder, solutionProjectLocation), new List<string>() { licenseFileDestination });

                logger.LogInformation("Copied license file to " + Path.Join(Config.TargetFolder, Config.ProjectName, "App_Data/Sitefinity/Sitefinity.lic"));
            }

            // restore database
            if(!Config.SkipDbRestore)
            {
                logger.LogInformation("Starting DB Restore.");
                string dbSourceFile = Path.Join(Config.TargetFolder, "SitefinityWebApp.bacpac");

                if(String.IsNullOrEmpty(Config.DatabaseRestoreUserName))
                {
                    this.RunProcess($"/a:Import /sf:\"{dbSourceFile}\" /tsn:{Config.SqlServer} /tdn:\"{Config.DatabaseName}\"");
                }
                else
                {
                    this.RunProcess($"/a:Import /sf:\"{dbSourceFile}\" /tsn:{Config.SqlServer} /tdn:\"{Config.DatabaseName}\" /tu:\"{Config.DatabaseRestoreUserName}\" /tp:\"{Config.DatabaseRestorePassword}\"");
                }
                
            }
            else
            {
                logger.LogInformation("Skipping DB Restore.");
            }

            // Restore Nuget packages
            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(Config.Version, packageSources);
            this.sitefinityPackageManager.Restore(Path.Join(Config.TargetFolder, Config.SolutionName + ".sln"));
            this.logger.LogInformation("Finsihed.");
            return;
            
        }

        private string GetSourceFolders(string customSourcePath)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sourcePath = (String.IsNullOrEmpty(customSourcePath) ? Path.Combine(currentPath, Constants.ProjectsFolderName) : customSourcePath);

            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException(string.Format(Constants.FolderNotFoundMessage, sourcePath));
            }

            return sourcePath;
        }

        private void CleanTargetDirectory(string targetDirectory)
        {
            DirectoryInfo di = new DirectoryInfo(targetDirectory);

            try
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Trouble deleting files in the target location. " + ex.Message);
            }
            
        }

        private IEnumerable<string> GetNugetPackageSources()
        {

            if (Config.PackageSources.Count() < 1)
            {
                return this.sitefinityPackageManager.DefaultPackageSource;
            }

            return Config.PackageSources;
        }

        private void RunProcess(string arguments)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = Constants.SqlPackageFile,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    logger.LogInformation(line);
                }
            }
        }


        private readonly IPromptService promptService;

        private readonly ICsProjectFileEditor csProjectFileEditor;

        private readonly ISitefinityPackageManager sitefinityPackageManager;

        private readonly ILogger<object> logger;

        private readonly IVisualStudioWorker visualStudioWorker;
    }
}
