using Microsoft.Extensions.Logging;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Sitefinity_CLI.PackageManagement
{
    internal class SitefinityPackageManager : ISitefinityPackageManager
    {
        public SitefinityPackageManager(
            INuGetApiClient nuGetApiClient,
            INuGetCliClient nuGetCliClient,
            IPackagesConfigFileEditor packagesConfigFileEditor,
            IProjectConfigFileEditor projectConfigFileEditor,
            ILogger<SitefinityPackageManager> logger)
        {
            this.nuGetApiClient = nuGetApiClient;
            this.nuGetCliClient = nuGetCliClient;
            this.packagesConfigFileEditor = packagesConfigFileEditor;
            this.projectConfigFileEditor = projectConfigFileEditor;
            this.logger = logger;
            this.defaultSources = new List<string>() { SitefinityPublicNuGetSource, PublicNuGetSource };
            this.supportedFrameworksRegex = new Regex("^net[0-9]*$", RegexOptions.Compiled);
        }

        public void Install(string packageId, string version, string solutionFilePath, IEnumerable<string> nugetPackageSources)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath);

            this.logger.LogInformation(string.Format("[{0}] Installing package \"{1}\"...", solutionDirectory, packageId));
            var sourcesUsed = string.Join(',', nugetPackageSources);
            this.logger.LogInformation(string.Format("Package sources used: {0}", sourcesUsed));

            this.nuGetCliClient.InstallPackage(packageId, version, solutionDirectory, nugetPackageSources);

            this.logger.LogInformation(string.Format("[{0}] Install for package \"{1}\" is complete", solutionDirectory, packageId));
        }

        public void Install(string packageId, string version, string solutionFilePath)
        {
            this.Install(packageId, version, solutionFilePath, this.defaultSources);
        }

        public void Restore(string solutionFilePath)
        {
            this.logger.LogInformation(string.Format("[{0}] Restoring packages started...", solutionFilePath));

            this.nuGetCliClient.Restore(solutionFilePath);

            this.logger.LogInformation(string.Format("[{0}] Restoring packages is complete", solutionFilePath));
        }

        public bool PackageExists(string packageId, string projectFilePath)
        {
            string packagesConfigFilePath = this.GetPackagesConfigFilePathForProject(projectFilePath);

            NuGetPackage nuGetPackage = this.packagesConfigFileEditor.FindPackage(packagesConfigFilePath, packageId);
            if (nuGetPackage != null)
            {
                return true;
            }

            return false;
        }

        public async Task<NuGetPackage> GetSitefinityPackageTree(string version)
        {
            return await this.GetSitefinityPackageTree(version, this.defaultSources);
        }

        public async Task<NuGetPackage> GetSitefinityPackageTree(string version, IEnumerable<string> nugetPackageSources)
        {
            var sourcesUsed = string.Join(',', nugetPackageSources);
            this.logger.LogInformation(string.Format("Package sources used: {0}", sourcesUsed));

            return await nuGetApiClient.GetPackageWithFullDependencyTree(Constants.SitefinityAllNuGetPackageId, version, nugetPackageSources, this.supportedFrameworksRegex);
        }

        public void SyncReferencesWithPackages(string projectFilePath, string solutionDir)
        {
            this.logger.LogInformation(string.Format("Synchronizing packages and references for project '{0}'", projectFilePath));

            string packagesConfigFilePath = this.GetPackagesConfigFilePathForProject(projectFilePath);
            IEnumerable<NuGetPackage> packages = this.packagesConfigFileEditor.GetPackages(packagesConfigFilePath);

            XmlDocument projectFileXmlDocument = new XmlDocument();
            projectFileXmlDocument.Load(projectFilePath);

            var processedAssemblies = new HashSet<string>();
            var projectDir = Path.GetDirectoryName(projectFilePath);

            var projectConfigPath = this.projectConfigFileEditor.GetProjectConfigPath(projectDir);
            XmlNodeList bindingRedirectNodes = null;
            XmlDocument projectConfig = null;
            if (!string.IsNullOrEmpty(projectConfigPath))
            {
                projectConfig = new XmlDocument();
                projectConfig.Load(projectConfigPath);
                bindingRedirectNodes = projectConfig.GetElementsByTagName("dependentAssembly");
            }

            XmlNodeList references = projectFileXmlDocument.GetElementsByTagName(Constants.ReferenceElem);
            string targetFramework = this.GetTargetFramework(projectFileXmlDocument);

            IEnumerable<AssemblyReference> assemblyReferences = this.AssemblyReferencesFromNuGetPackages(packages, targetFramework, projectDir, solutionDir);
            IEnumerable<IGrouping<string, AssemblyReference>> assemblyReferenceGroups = assemblyReferences
                .Where(ar => ar.Version != null)
                .GroupBy(ar => ar.Name);

            // Foreach package installed for this project, check if all DLLs are included. If not - include missing ones. Fix binding redirects in web.config if necessary.
            foreach (IGrouping<string, AssemblyReference> assemblyReferenceGroup in assemblyReferenceGroups)
            {
                string assemblyName = assemblyReferenceGroup.Key;
                AssemblyReference assemblyReferenceWithNewestVersion = assemblyReferenceGroup.OrderByDescending(ar => ar.Version).First();

                bool assemblyReferenceFound = false;
                for (int i = 0; i < references.Count; i++)0
                {
                    var referenceElement = references[i];
                    var includeAttr = referenceElement.Attributes[Constants.IncludeAttribute];
                    var includeAttrValue = includeAttr.Value;

                    if (includeAttrValue.StartsWith(assemblyName + ",", StringComparison.OrdinalIgnoreCase) || includeAttrValue == assemblyName)
                    {
                        Version currentAssemblyVersion = this.ExtractPackageVersionFromIncludeAttribute(includeAttrValue);

                        if (currentAssemblyVersion > assemblyReferenceWithNewestVersion.Version)
                        {
                            this.logger.LogInformation(string.Format("The assembly reference '{0}' is on version '{1}'. It won't be downgraded to '{2}'.", assemblyName, currentAssemblyVersion, assemblyReferenceWithNewestVersion.Version));
                            assemblyReferenceFound = true;
                            break;
                        }

                        var proccesorArchitecture = includeAttrValue.Split(',').FirstOrDefault(x => x.Contains(ProcessorArchitectureAttribute));
                        var includeAttributeNewValue = string.IsNullOrEmpty(proccesorArchitecture) ? assemblyReferenceWithNewestVersion.FullName : $"{assemblyReferenceWithNewestVersion.FullName},{proccesorArchitecture}";

                        if (!includeAttr.Value.Equals(includeAttributeNewValue, StringComparison.OrdinalIgnoreCase))
                        {
                            this.logger.LogInformation(string.Format("The include attribue '{0}' is updated to '{1}'.", includeAttr.Value, includeAttributeNewValue));
                            includeAttr.Value = includeAttributeNewValue;
                        }

                        var childNodes = referenceElement.ChildNodes;
                        XmlNode hintPathNode = null;
                        for (int j = 0; j < childNodes.Count; j++)
                        {
                            var childNode = childNodes[j];
                            if (childNode.Name == Constants.HintPathElem)
                            {
                                hintPathNode = childNode;
                                break;
                            }
                        }

                        // Hint path missing, so we add it
                        if (hintPathNode == null)
                        {
                            this.logger.LogInformation(string.Format("Added hint path for reference assembly '{0}'.", assemblyReferenceWithNewestVersion.FullName));

                            hintPathNode = projectFileXmlDocument.CreateElement(Constants.HintPathElem, projectFileXmlDocument.DocumentElement.NamespaceURI);
                            referenceElement.AppendChild(hintPathNode);
                            hintPathNode.InnerText = assemblyReferenceWithNewestVersion.HintPath;
                        }
                        else if (!assemblyReferenceGroup.Any(ar => ar.HintPath.Equals(hintPathNode.InnerText, StringComparison.OrdinalIgnoreCase)))
                        {
                            // TODO: we can load the currently referenced assembly and replace the hint path only if the assemblie version is different. There are cases when one dll is located in multiple packages
                            this.logger.LogInformation(string.Format("Fixing broken hint path for reference assembly '{0}' from '{1}' to '{2}'.", assemblyReferenceWithNewestVersion.FullName, hintPathNode.InnerText, assemblyReferenceWithNewestVersion.HintPath));

                            hintPathNode.InnerText = assemblyReferenceWithNewestVersion.HintPath;
                        }

                        assemblyReferenceFound = true;
                        break;
                    }
                }

                // DLL reference is missing, so we add it.
                if (!assemblyReferenceFound)
                {
                    this.logger.LogInformation(string.Format("Added missing assembly reference '{0}' with path '{1}'.", assemblyReferenceWithNewestVersion.FullName, assemblyReferenceWithNewestVersion.HintPath));

                    XmlNode referencesGroup = projectFileXmlDocument.GetElementsByTagName(Constants.ItemGroupElem)[0];
                    XmlElement referenceNode = projectFileXmlDocument.CreateElement(Constants.ReferenceElem, projectFileXmlDocument.DocumentElement.NamespaceURI);

                    XmlAttribute includeAttr = projectFileXmlDocument.CreateAttribute(Constants.IncludeAttribute);
                    includeAttr.Value = assemblyReferenceWithNewestVersion.FullName;
                    referenceNode.Attributes.Append(includeAttr);

                    XmlElement hintPathNode = projectFileXmlDocument.CreateElement(Constants.HintPathElem, projectFileXmlDocument.DocumentElement.NamespaceURI);
                    hintPathNode.InnerText = assemblyReferenceWithNewestVersion.HintPath;

                    referenceNode.AppendChild(hintPathNode);
                    referencesGroup.AppendChild(referenceNode);
                }

                this.SyncBindingRedirects(projectConfig, bindingRedirectNodes, assemblyName, assemblyReferenceWithNewestVersion.Version.ToString());
            }

            projectFileXmlDocument.Save(projectFilePath);
            projectConfig?.Save(projectConfigPath);

            this.logger.LogInformation(string.Format("Synchronization completed for project '{0}'", projectFilePath));
        }

        private IEnumerable<AssemblyReference> AssemblyReferencesFromNuGetPackages(IEnumerable<NuGetPackage> nuGetPackages, string targetFramework, string projectDir, string solutionDir)
        {
            List<string> dllFilePaths = new List<string>();
            foreach (NuGetPackage nuGetPackage in nuGetPackages)
            {
                string packageDir = string.Format("{0}\\{1}\\{2}.{3}", solutionDir, PackagesFolderName, nuGetPackage.Id, nuGetPackage.Version);
                dllFilePaths.AddRange(this.GetPackageDlls(packageDir, targetFramework));
            }

            IEnumerable<AssemblyReference> assemblyReferences = dllFilePaths.Distinct().Select(d => this.GetAssemblyReferenceFromDllFilePath(d, projectDir));

            return assemblyReferences;
        }

        private AssemblyReference GetAssemblyReferenceFromDllFilePath(string dllFilePath, string projectDir)
        {
            Assembly assembly = Assembly.LoadFile(dllFilePath);
            AssemblyName assemblyName = assembly.GetName();

            AssemblyReference assemblyReference = new AssemblyReference();
            assemblyReference.Name = assemblyName.Name;
            assemblyReference.Version = assemblyName.Version;
            assemblyReference.FullName = assemblyName.FullName;
            assemblyReference.HintPath = this.GetRelativePathTo(projectDir + "\\", dllFilePath);

            return assemblyReference;
        }

        private Version ExtractPackageVersionFromIncludeAttribute(string includeAttrValue)
        {
            var versionChunk = includeAttrValue.Split(',')
                .FirstOrDefault(x => x.Contains("Version"));

            if (versionChunk == null)
            {
                this.logger.LogInformation($"Unable to get the version in {includeAttrValue}");
                return null;
            }

            var packageVersionString = versionChunk
                .Split("=")
                .ToList()[1];

            var parsedVersion = Version.Parse(packageVersionString);

            return parsedVersion;
        }

        public IEnumerable<string> DefaultPackageSource
        {
            get
            {
                return new List<string>(this.defaultSources);
            }
        }

        private void SyncBindingRedirects(XmlDocument configDoc, XmlNodeList bindingRedirectNodes, string assemblyFullName, string assemblyVersion)
        {
            if (bindingRedirectNodes != null)
            {
                foreach (XmlNode node in bindingRedirectNodes)
                {
                    XmlNode assemblyIdentity = null;
                    XmlNode bindingRedirect = null;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.Name == assemblyIdentityAttributeName)
                            assemblyIdentity = childNode;

                        if (childNode.Name == bindingRedirectAttributeName)
                            bindingRedirect = childNode;
                    }

                    if (assemblyIdentity != null && bindingRedirect != null)
                    {
                        var name = assemblyIdentity.Attributes["name"]?.Value;
                        if (name == assemblyFullName)
                        {
                            var newVersionAttribute = bindingRedirect.Attributes[newVersionAttributeName];
                            if (newVersionAttribute != null && !this.ShouldUpdateBindingRedirect(newVersionAttribute.Value, assemblyVersion))
                            {
                                break;
                            }

                            if (newVersionAttribute == null)
                            {
                                newVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(newVersionAttribute);
                            }

                            newVersionAttribute.Value = assemblyVersion;

                            var oldVersionAttribute = bindingRedirect.Attributes[oldVersionAttributeName];
                            if (oldVersionAttribute == null)
                            {
                                oldVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(oldVersionAttribute);
                            }

                            oldVersionAttribute.Value = string.Format("0.0.0.0-{0}", assemblyVersion);

                            break;
                        }
                    }
                }
            }
        }
        
        private bool ShouldUpdateBindingRedirect(string oldAssemblyVersion, string newAssemblyVersion)
        {
            var oldVersion = Version.Parse(oldAssemblyVersion);
            var newVersion = Version.Parse(newAssemblyVersion);

            return newVersion > oldVersion;
        }

        private bool TrySetTargetFramework(XmlDocument doc, string targetFramework)
        {
            var targetFrameworkVersionElems = doc.GetElementsByTagName(Constants.TargetFrameworkVersionElem);
            if (targetFrameworkVersionElems.Count != 1)
            {
                throw new InvalidOperationException("Unable to set the target framework");
            }

            if (targetFrameworkVersionElems[0].InnerText != targetFramework)
            {
                targetFrameworkVersionElems[0].InnerText = targetFramework;

                return true;
            }

            return false;
        }

        private string GetTargetFramework(XmlDocument doc)
        {
            XmlNodeList targetFrameworkVersionElems = doc.GetElementsByTagName(Constants.TargetFrameworkVersionElem);
            if (targetFrameworkVersionElems.Count != 1)
            {
                throw new InvalidOperationException("Unable to get the target framework");
            }

            return targetFrameworkVersionElems[0].InnerText;
        }

        private string GetTargetFrameworkForSitefinityVersion(string version)
        {
            var versionWithoutSeperator = version.Replace(".", string.Empty).Substring(0, 3);
            var versionAsInt = int.Parse(versionWithoutSeperator);

            if (versionAsInt < 100)
            {
                return "v4.0";
            }
            else if (versionAsInt >= 100 && versionAsInt <= 102)
            {
                return "v4.5";
            }
            else if (versionAsInt >= 110 && versionAsInt <= 112)
            {
                return "v4.7.1";
            }
            else if (versionAsInt >= 120 && versionAsInt < 132)
            {
                return "v4.7.2";
            }
            else if (versionAsInt >= 132)
            {
                return "v4.8";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the dlls that should be referenced for the current package
        /// Check https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks for more info
        /// </summary>
        private IEnumerable<string> GetPackageDlls(string packagePath, string targetVersion)
        {
            // Target framework convention looks like v4.7.2
            var versionPart = targetVersion.Replace(".", string.Empty).Replace("v", string.Empty);
            int.TryParse(versionPart, out int targetVersionNumber);
            if (targetVersionNumber == 0)
                return new List<string>();

            var libDir = Path.Combine(packagePath, LibFolderName);
            string dllStorageDir = null;
            if (Directory.Exists(libDir))
            {
                if (Directory.GetDirectories(libDir).Any())
                {
                    int currentMaxFolderFrameworkVersion = 0;
                    foreach (var subDir in Directory.GetDirectories(libDir, DotNetPrefix + "*"))
                    {
                        var versionFromFolder = subDir.Split("\\").Last().Replace(DotNetPrefix, string.Empty);

                        // The folder may have "-" in the name
                        if (versionFromFolder.Contains("-"))
                        {
                            versionFromFolder = versionFromFolder.Split("-").FirstOrDefault();
                        }

                        // Fix for the cases when we upgrade from versions with different length - 4.7.1 to 4.8
                        if (versionPart.Length < versionFromFolder.Length)
                        {
                            targetVersionNumber *= 10;
                        }

                        int.TryParse(versionFromFolder, out int currentFolderFrameworkVersion);

                        if (currentFolderFrameworkVersion != 0 && currentFolderFrameworkVersion <= targetVersionNumber && currentFolderFrameworkVersion > currentMaxFolderFrameworkVersion)
                        {
                            dllStorageDir = subDir;
                            currentMaxFolderFrameworkVersion = currentFolderFrameworkVersion;
                        }
                    }
                }
                else
                {
                    dllStorageDir = libDir;
                }
            }
            else
            {
                dllStorageDir = packagePath;
            }

            return dllStorageDir != null && Directory.Exists(dllStorageDir) ? Directory.GetFiles(dllStorageDir, DllFilterPattern) : new string[] { };
        }

        private string GetPackagesConfigFilePathForProject(string projectFilePath)
        {
            string projectDirectory = Path.GetDirectoryName(projectFilePath);
            string packagesConfigFilePath = Path.Combine(projectDirectory, Constants.PackagesConfigFileName);

            if (!File.Exists(packagesConfigFilePath))
            {
                throw new FileNotFoundException(string.Format("File \"{0}\" not found in project directory \"{1}\". Cannot proceed with upgrade.", Constants.PackagesConfigFileName, projectDirectory));
            }

            return packagesConfigFilePath;
        }

        private string GetRelativePathTo(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"Invalid version: {version}");
            }

            var targetFramework = this.GetTargetFrameworkForSitefinityVersion(version);

            foreach (var projectFilePath in sitefinityProjectFilePaths)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(projectFilePath);

                if (this.TrySetTargetFramework(doc, targetFramework))
                {
                    doc.Save(projectFilePath);

                    this.logger.LogInformation(string.Format(Constants.TargetFrameworkChanged, Path.GetFileName(projectFilePath), targetFramework));
                }
                else
                {
                    this.logger.LogInformation(string.Format(Constants.TargetFrameworkDoesNotNeedChanged, Path.GetFileName(projectFilePath), targetFramework));
                }
            }
        }

        private readonly INuGetApiClient nuGetApiClient;

        private readonly INuGetCliClient nuGetCliClient;

        private readonly IPackagesConfigFileEditor packagesConfigFileEditor;

        private readonly IProjectConfigFileEditor projectConfigFileEditor;

        private readonly ILogger logger;

        private readonly IEnumerable<string> defaultSources;

        private const string SitefinityPublicNuGetSource = "https://nuget.sitefinity.com/nuget/";

        private const string PublicNuGetSource = "https://nuget.org/api/v2/";

        private const string PackagesFolderName = "packages";

        private const string LibFolderName = "lib";

        private const string DotNetPrefix = "net";

        private const string DllFilterPattern = "*.dll";

        private const string ProcessorArchitectureAttribute = "processorArchitecture";

        private readonly Regex supportedFrameworksRegex;

        private const string assemblyIdentityAttributeName = "assemblyIdentity";
        private const string bindingRedirectAttributeName = "bindingRedirect";
        private const string oldVersionAttributeName = "oldVersion";
        private const string newVersionAttributeName = "newVersion";
    }
}
