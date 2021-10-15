using EnvDTE;
using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Commands.Validators;
using Sitefinity_CLI.Exceptions;
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
using System.Xml;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.CreateCommandName, Description = "Create a new Sitefinity project.")]
    [AdminRightsValidator]
    internal class CreateCommand
    {
        [Argument(0, Description = Constants.SolutionFolderTargetDescription)]
        [Required(ErrorMessage = "You must specify a folder for the soloution.")]
        public string TargetFolder { get; set; }

        [Argument(1, Description = Constants.VersionCreateOptionDescription)]
        [Required(ErrorMessage = "You must specify the Sitefinity version to create.")]
        public string Version { get; set; }

        [Argument(2, Description = Constants.SqlServerDescription)]
        [Required(ErrorMessage = "You must specify the SQL Server to deploy the database to.")]
        public string SqlServer { get; set; }

        [Option(Constants.SolutionName, Description = Constants.SolutionNameDescription)]
        public string SolutionName { get; set; } = Constants.DefaultName;

        [Option(Constants.ProjectName, Description = Constants.ProjectNameDescription)]
        public string ProjectName { get; set; } = Constants.DefaultName;

        [Option(Constants.DatabaseName, Description = Constants.DatabaseNameDescription)]
        public string DatabaseName { get; set; } = Constants.DefaultName;

        [Option(Constants.CustomSourcePath, Description = Constants.CustomSourcePathDescription)]
        public string CustomSourcePath { get; set; }

        [Option(Constants.SkipDbRestore, Description = Constants.SkipDbRestoreDescription)]
        public bool SkipDbRestore { get; set; }

        [Option(Constants.LicenseFilePath, Description = Constants.LicenseFilePathDescription)]
        public string LicenseFilePath { get; set; }

        [Option(Constants.PackageSources, Description = Constants.PackageSourcesDescription)]
        public string PackageSources { get; set; }

        public CreateCommand(
            IPromptService promptService,
            ISitefinityPackageManager sitefinityPackageManager,
            ICsProjectFileEditor csProjectFileEditor,
            ILogger<UpgradeCommand> logger,
            IProjectConfigFileEditor projectConfigFileEditor,
            IVisualStudioWorker visualStudioWorker)
        {
            this.promptService = promptService;
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.csProjectFileEditor = csProjectFileEditor;
            this.logger = logger;
            this.visualStudioWorker = visualStudioWorker;
            this.projectConfigFileEditor = projectConfigFileEditor;
            this.processedPackagesPerProjectCache = new Dictionary<string, HashSet<string>>();
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
            logger.LogInformation("Starting");
            if (!Directory.Exists(TargetFolder))
            {
                throw new DirectoryNotFoundException(string.Format(Constants.FolderNotFoundMessage, TargetFolder));
            }
            if(!Regex.IsMatch(ProjectName, @"^[^0-9][a-zA-Z0-9_.-]+$"))
            {
                throw new ArgumentException(string.Format(Constants.InvalidProjectNameMessage, ProjectName));
            }

            if(!String.IsNullOrEmpty(LicenseFilePath) && !File.Exists(LicenseFilePath))
            {
                throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, LicenseFilePath));
            }
            if(!SkipDbRestore && !File.Exists(Constants.SqlPackageFile))
            {
                logger.LogWarning("Unabel to locate SqlPackage.exe for the DB Restore.");
                logger.LogWarning("Setting SkipDbRestore to True.");
                SkipDbRestore = true;
            }

            string sourceFolder = GetSourceFolders(Version, CustomSourcePath);
            string solutionZipFile = String.Format(Constants.SolutionZipFileFormat, Version);


            logger.LogInformation("Cleaning out all files and directories in the target folder.");
            logger.LogInformation(TargetFolder);
            CleanTargetDirectory(TargetFolder);
            logger.LogInformation("Completed.");

            logger.LogInformation("Extract solution zip file to target.");
            logger.LogInformation(Path.Join(sourceFolder, solutionZipFile));
            ZipFile.ExtractToDirectory(Path.Join(sourceFolder, solutionZipFile), TargetFolder);

            logger.LogInformation("Processing each 'templated' file.");
            string[] templateFiles = Directory.GetFiles(TargetFolder, "*.Template", SearchOption.AllDirectories);

            string solutionProjectLocation = String.Format("{0}\\{0}.csproj", ProjectName);
            var data = new
            {
                SolutionName = SolutionName,
                ProjectName = ProjectName,
                DatabaseName = DatabaseName,
                SqlServer = SqlServer,
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
            if(this.ProjectName != Constants.DefaultName)
            {
                Directory.Move(Path.Join(this.TargetFolder, Constants.DefaultName), Path.Join(this.TargetFolder, this.ProjectName));
                logger.LogInformation("Renamed project folder to " + ProjectName);
                File.Move(Path.Join(this.TargetFolder, this.ProjectName, Constants.DefaultName + ".csproj"), Path.Join(this.TargetFolder, this.ProjectName, this.ProjectName + ".csproj"));
                logger.LogInformation("Renamed project to " + ProjectName + ".csproj");
            }

            // Use alternate solution name
            if (SolutionName != Constants.DefaultName)
            {
                File.Move(Path.Join(this.TargetFolder, Constants.DefaultName + ".sln"), Path.Join(this.TargetFolder, this.SolutionName + ".sln"));
                logger.LogInformation("Renamed solution to " + SolutionName + ".sln");
            }

            // add optional license file
            if (!String.IsNullOrEmpty(LicenseFilePath))
            {
                File.Copy(LicenseFilePath, Path.Join(TargetFolder, ProjectName, "App_Data/Sitefinity/Sitefinity.lic"));
                logger.LogInformation("Copied license file to " + Path.Join(TargetFolder, ProjectName, "App_Data/Sitefinity/Sitefinity.lic"));
            }

            // restore database
            if(!SkipDbRestore)
            {
                logger.LogInformation("Starting DB Restore.");
                string dbSourceFile = Path.Join(TargetFolder, "SitefinityWebApp.bacpac");

                this.RunProcess($"/a:Import /sf:\"{dbSourceFile}\" /tsn:{SqlServer} /tdn:\"{DatabaseName}\"");
            }
            else
            {
                logger.LogInformation("Skipping DB Restore.");
            }

            // Restore Nuget packages
            var packageSources = this.GetNugetPackageSources();
            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(this.Version, packageSources);
            this.sitefinityPackageManager.Restore(Path.Join(this.TargetFolder, SolutionName + ".sln"));

            this.logger.LogInformation("Finsihed.");

            
            
            return;
            
        }

        private string GetSourceFolders(string version, string customSourcePath)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sourcePath = (String.IsNullOrEmpty(customSourcePath) ? Path.Combine(currentPath, Constants.ProjectsFolderName) : customSourcePath);

            this.logger.LogInformation(sourcePath);
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
                throw new Exception("Trouble deleting files in teh target location. " + ex.Message);
            }
            
        }

        private IEnumerable<string> GetNugetPackageSources()
        {
            if (string.IsNullOrEmpty(this.PackageSources))
            {
                return this.sitefinityPackageManager.DefaultPackageSource;
            }

            var packageSources = this.PackageSources.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ps => ps.Trim());
            return packageSources;
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

        private readonly ISitefinityPackageManager sitefinityPackageManager;

        private readonly ICsProjectFileEditor csProjectFileEditor;

        private readonly IProjectConfigFileEditor projectConfigFileEditor;

        private readonly ILogger<object> logger;

        private readonly IVisualStudioWorker visualStudioWorker;

        private readonly IDictionary<string, HashSet<string>> processedPackagesPerProjectCache;
    }
}
