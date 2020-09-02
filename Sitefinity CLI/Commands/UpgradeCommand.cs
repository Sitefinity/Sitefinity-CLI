using EnvDTE;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Commands.Validators;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.UpgradeCommandName, Description = "Upgrade Sitefinity project/s to a newer version of Sitefinity.")]
    internal class UpgradeCommand
    {
        [Argument(0, Description = Constants.ProjectOrSolutionPathOptionDescription)]
        [Required(ErrorMessage = "You must specify a path to a solution file.")]
        public string SolutionPath { get; set; }

        [Argument(1, Description = Constants.VersionToOptionDescription)]
        [Required(ErrorMessage = "You must specify the Sitefinity version to upgrade to.")]
        [UpgradeVersionValidator]
        public string Version { get; set; }

        [Option(Constants.SkipPrompts, Description = Constants.SkipPromptsDescription)]
        public bool SkipPrompts { get; set; }

        [Option(Constants.AcceptLicense, Description = Constants.AcceptLicenseOptionDescription)]
        public bool AcceptLicense { get; set; }

        [Option(Constants.PackageSources, Description = Constants.PackageSourcesDescription)]
        public string PackageSources { get; set; }

        public UpgradeCommand(
            ISitefinityPackageManager sitefinityPackageManager,
            ICsProjectFileEditor csProjectFileEditor,
            ILogger<object> logger,
            IProjectConfigFileEditor projectConfigFileEditor,
            IVisualStudioWorker visualStudioWorker)
        {
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
                await this.ExecuteUpgrade();

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

        private async Task ExecuteUpgrade()
        {
            if (!File.Exists(this.SolutionPath))
            {
                throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, this.SolutionPath));
            }

            if (!this.SkipPrompts && !Prompt.GetYesNo(Constants.UpgradeWarning, false))
            {
                this.logger.LogInformation(Constants.UpgradeWasCanceled);
                return;
            }

            this.logger.LogInformation("Searching the provided project/s for Sitefinity references...");

            IEnumerable<string> sitefinityProjectFilePaths = this.GetProjectsPathsFromSolution(this.SolutionPath, true);
            IEnumerable<string> projectsWithouthSitefinityPaths = this.GetProjectsPathsFromSolution(this.SolutionPath).Except(sitefinityProjectFilePaths);

            Dictionary<string, string> configsWithoutSitefinity = this.GetConfigsForProjectsWithoutSitefinity(projectsWithouthSitefinityPaths);

            if (!sitefinityProjectFilePaths.Any())
            {
                Utils.WriteLine(Constants.NoProjectsFoundToUpgradeWarningMessage, ConsoleColor.Yellow);
            }

            this.logger.LogInformation(string.Format(Constants.NumberOfProjectsWithSitefinityReferencesFoundSuccessMessage, sitefinityProjectFilePaths.Count()));
            this.logger.LogInformation(string.Format("Collecting Sitefinity NuGet package tree for version \"{0}\"...", this.Version));
            var packageSources = this.GetNugetPackageSources();

            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(this.Version, packageSources);

            if (newSitefinityPackage == null)
            {
                this.logger.LogError(string.Format(Constants.VersionNotFound, this.Version));
                return;
            }

            this.sitefinityPackageManager.Restore(this.SolutionPath);
            this.sitefinityPackageManager.Install(newSitefinityPackage.Id, newSitefinityPackage.Version, this.SolutionPath, packageSources);

            if (!this.AcceptLicense)
            {
                var licenseContent = await GetLicenseContent(newSitefinityPackage);
                this.logger.LogInformation($"{Environment.NewLine}{licenseContent}{Environment.NewLine}");
                var hasUserAcceptedEULA = Prompt.GetYesNo(Constants.AcceptLicenseNotification, false);

                if (!hasUserAcceptedEULA)
                {
                    this.logger.LogInformation(Constants.UpgradeWasCanceled);
                    return;
                }
            }

            await this.GeneratePowershellConfig(sitefinityProjectFilePaths, newSitefinityPackage);

            var updaterPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, PowershellFolderName, "Updater.ps1");
            this.visualStudioWorker.Initialize(this.SolutionPath);
            this.visualStudioWorker.ExecuteScript(updaterPath);
            this.EnsureOperationSuccess();

            this.visualStudioWorker.Dispose();

            // revert config changes caused by nuget for projects we are not upgrading
            this.RestoreConfigValuesForNoSfProjects(configsWithoutSitefinity);

            this.SyncProjectReferencesWithPackages(sitefinityProjectFilePaths, Path.GetDirectoryName(this.SolutionPath));

            this.logger.LogInformation(string.Format(Constants.UpgradeSuccessMessage, this.SolutionPath, this.Version));
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

        private void RestoreConfigValuesForNoSfProjects(Dictionary<string, string> configsWithoutSitefinity)
        {
            foreach (var item in configsWithoutSitefinity)
            {
                File.WriteAllText(item.Key, item.Value);
            }
        }

        private Dictionary<string, string> GetConfigsForProjectsWithoutSitefinity(IEnumerable<string> projectsWithouthSfreferencePaths)
        {
            var configsWithoutSitefinity = new Dictionary<string, string>();
            foreach (var project in projectsWithouthSfreferencePaths)
            {
                var porjectConfigPath = this.projectConfigFileEditor.GetProjectConfigPath(project);
                if (!string.IsNullOrEmpty(porjectConfigPath))
                {
                    var configValue = File.ReadAllText(porjectConfigPath);
                    configsWithoutSitefinity.Add(porjectConfigPath, configValue);
                }
            }

            return configsWithoutSitefinity;
        }

        private async Task<string> GetLicenseContent(NuGetPackage newSitefinityPackage)
        {
            var pathToPackagesFolder = Path.Combine(Path.GetDirectoryName(this.SolutionPath), Constants.PackagesFolderName);
            var pathToTheLicense = Path.Combine(pathToPackagesFolder, $"{newSitefinityPackage.Id}.{newSitefinityPackage.Version}", Constants.LicenseAgreementsFolderName, "License.txt");
            var licenseContent = await File.ReadAllTextAsync(pathToTheLicense);

            return licenseContent;
        }

        private string DetectSitefinityVersion(string sitefinityProjectPath)
        {
            CsProjectFileReference sitefinityReference = this.csProjectFileEditor.GetReferences(sitefinityProjectPath)
                .FirstOrDefault(r => this.IsSitefinityReference(r));

            if (sitefinityReference != null)
            {
                string versionPattern = @"Version=(.*?),";
                Match versionMatch = Regex.Match(sitefinityReference.Include, versionPattern);

                if (versionMatch.Success)
                {
                    return versionMatch.Groups[1].Value;
                }
            }

            return null;
        }

        private bool ContainsSitefinityRefKeyword(CsProjectFileReference projectReference)
        {
            return projectReference.Include.Contains(TelerikSitefinityReferenceKeyWords) || projectReference.Include.Contains(ProgressSitefinityReferenceKeyWords);
        }

        private void EnsureOperationSuccess()
        {
            this.logger.LogInformation("Waiting for operation to complete...");

            var resultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PowershellFolderName, "result.log");
            var progressFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PowershellFolderName, "progress.log");
            File.Delete(resultFile);
            int waitStep = 500;
            int iterations = 0;
            var lastProgressUpdate = string.Empty;
            while (true)
            {
                if (!File.Exists(resultFile))
                {
                    if (iterations % 2 == 0 && File.Exists(progressFile))
                    {
                        try
                        {
                            var progressInfo = this.ReadAllTextFromFile(progressFile).Replace("\r\n", string.Empty);
                            if (lastProgressUpdate != progressInfo)
                            {
                                lastProgressUpdate = progressInfo;
                                this.logger.LogInformation(progressInfo);
                            }
                        }
                        catch { }
                    }

                    iterations++;

                    // Last operation is still not executed and the file is not created.
                    System.Threading.Thread.Sleep(waitStep);
                    continue;
                }

                System.Threading.Thread.Sleep(waitStep);
                var result = this.ReadAllTextFromFile(resultFile);
                if (result != "success")
                {
                    this.logger.LogError(string.Format("Powershell upgrade failed. {0}", result));
                    throw new Exception("Powershell upgrade failed");
                }

                break;
            }

            this.logger.LogInformation("Operation completed successfully!");
        }

        private string ReadAllTextFromFile(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                return textReader.ReadToEnd();
            }
        }

        private async Task GeneratePowershellConfig(IEnumerable<string> projectFilePaths, NuGetPackage newSitefinityPackage)
        {
            this.logger.LogInformation("Exporting powershell config...");

            this.packagesPerProject = new Dictionary<string, List<NuGetPackage>>();

            var powerShellXmlConfig = new XmlDocument();
            var powerShellXmlConfigNode = powerShellXmlConfig.CreateElement("config");
            powerShellXmlConfig.AppendChild(powerShellXmlConfigNode);

            foreach (string projectFilePath in projectFilePaths)
            {
                var projectNode = powerShellXmlConfig.CreateElement("project");
                var projectNameAttr = powerShellXmlConfig.CreateAttribute("name");
                projectNameAttr.Value = projectFilePath.Split(new string[] { "\\", Constants.CsprojFileExtension, Constants.VBProjFileExtension }, StringSplitOptions.RemoveEmptyEntries).Last();
                projectNode.Attributes.Append(projectNameAttr);
                powerShellXmlConfigNode.AppendChild(projectNode);

                packagesPerProject[projectFilePath] = new List<NuGetPackage>();

                var currentSitefinityVersion = this.DetectSitefinityVersion(projectFilePath);

                if (string.IsNullOrEmpty(currentSitefinityVersion))
                {
                    this.logger.LogInformation(string.Format("Skip upgrade for project: \"{0}\". Current Sitefinity version was not detected.", projectFilePath));
                    continue;
                }

                // todo add validation if from version is greater than the to version

                this.logger.LogInformation(string.Format("Detected sitefinity version for \"{0}\" - \"{1}\"", projectFilePath, currentSitefinityVersion));

                this.logger.LogInformation(string.Format("Collecting Sitefinity NuGet package tree for \"{0}\"...", projectFilePath));
                NuGetPackage currentSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(currentSitefinityVersion);

                this.IteratePackages(projectFilePath, currentSitefinityPackage, newSitefinityPackage, (package) =>
                {
                    var packageNode = powerShellXmlConfig.CreateElement("package");
                    var nameAttr = powerShellXmlConfig.CreateAttribute("name");
                    nameAttr.Value = package.Id;
                    var versionAttr = powerShellXmlConfig.CreateAttribute("version");
                    versionAttr.Value = package.Version;
                    packageNode.Attributes.Append(nameAttr);
                    packageNode.Attributes.Append(versionAttr);
                    projectNode.AppendChild(packageNode);

                    packagesPerProject[projectFilePath].Add(package);
                });
            }

            powerShellXmlConfig.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PowershellFolderName, "config.xml"));

            this.logger.LogInformation("Successfully exported powershell config!");
        }

        private void IteratePackages(string projectFilePath, NuGetPackage currentSitefinityPackage, NuGetPackage newSitefinityPackage, Action<NuGetPackage> action)
        {
            this.processedPackagesPerProjectCache[projectFilePath] = new HashSet<string>();
            var packagesQueue = new Queue<NuGetPackage>();
            packagesQueue.Enqueue(newSitefinityPackage);

            while (packagesQueue.Any())
            {
                var package = packagesQueue.Dequeue();

                bool isPackageAlreadyProcessed = this.processedPackagesPerProjectCache[projectFilePath].Contains(package.Id);
                if (isPackageAlreadyProcessed)
                {
                    continue;
                }

                this.processedPackagesPerProjectCache[projectFilePath].Add(package.Id);

                bool packageExists = this.sitefinityPackageManager.PackageExists(package.Id, projectFilePath);
                if (packageExists)
                {
                    action(package);
                }

                if (package.Dependencies != null)
                {
                    var dependenciesOfDependency = new List<NuGetPackage>();
                    foreach (NuGetPackage nuGetPackageDependency in package.Dependencies)
                    {
                        // Package manager can't update packages that are dependencies of other packages.
                        var parentDependencies = currentSitefinityPackage.Dependencies.Where(directDependency => directDependency.Dependencies.Any(d => d.Id == nuGetPackageDependency.Id));
                        if (!parentDependencies.Any())
                        {
                            packagesQueue.Enqueue(nuGetPackageDependency);
                        }
                        else
                        {
                            dependenciesOfDependency.Add(nuGetPackageDependency);
                        }
                    }

                    // Finally include the dependencies that are also dependencies in other packages
                    foreach (var dependencyOfDependency in dependenciesOfDependency)
                    {
                        packagesQueue.Enqueue(dependencyOfDependency);
                    }
                }
            }
        }

        private void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder)
        {
            foreach (string projectFilePath in projectFilePaths)
            {
                var packages = new List<NuGetPackage>(this.packagesPerProject[projectFilePath]);
                packages.Reverse();
                this.sitefinityPackageManager.SyncReferencesWithPackages(projectFilePath, solutionFolder, packages, this.Version);
            }
        }

        private IEnumerable<string> GetProjectsPathsFromSolution(string solutionPath, bool onlySitefinityProjects = false)
        {
            if (!solutionPath.EndsWith(Constants.SlnFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format(Constants.FileIsNotSolutionMessage, solutionPath));
            }

            IEnumerable<string> projectFilesAbsolutePaths = SolutionFileEditor.GetProjects(solutionPath)
                .Select(sp => sp.AbsolutePath)
                .Where(ap => (ap.EndsWith(Constants.CsprojFileExtension, StringComparison.InvariantCultureIgnoreCase) || ap.EndsWith(Constants.VBProjFileExtension, StringComparison.InvariantCultureIgnoreCase)));

            if (onlySitefinityProjects)
            {
                projectFilesAbsolutePaths = projectFilesAbsolutePaths.Where(ap => this.HasSitefinityReferences(ap));
            }

            return projectFilesAbsolutePaths;
        }

        private bool HasSitefinityReferences(string projectFilePath)
        {
            IEnumerable<CsProjectFileReference> references = this.csProjectFileEditor.GetReferences(projectFilePath);
            bool hasSitefinityReferences = references.Any(r => this.IsSitefinityReference(r));

            return hasSitefinityReferences;
        }

        private bool IsSitefinityReference(CsProjectFileReference reference)
        {
            return this.ContainsSitefinityRefKeyword(reference) && reference.Include.Contains($"PublicKeyToken={SitefinityPublicKeyToken}");
        }

        private readonly ISitefinityPackageManager sitefinityPackageManager;

        private readonly ICsProjectFileEditor csProjectFileEditor;

        private readonly IProjectConfigFileEditor projectConfigFileEditor;

        private readonly ILogger<object> logger;

        private readonly IVisualStudioWorker visualStudioWorker;

        private readonly IDictionary<string, HashSet<string>> processedPackagesPerProjectCache;

        private Dictionary<string, List<NuGetPackage>> packagesPerProject;

        private const string TelerikSitefinityReferenceKeyWords = "Telerik.Sitefinity";

        private const string ProgressSitefinityReferenceKeyWords = "Progress.Sitefinity";

        private const string PowershellFolderName = "PowerShell";

        private const string SitefinityPublicKeyToken = "b28c218413bdf563";
    }
}
