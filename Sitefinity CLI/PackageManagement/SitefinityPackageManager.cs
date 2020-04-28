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
            this.sources = new List<string>() { SitefinityPublicNuGetSource, PublicNuGetSource };
            this.supportedFrameworksRegex = new Regex("^net[0-9]*$", RegexOptions.Compiled);
        }

        public async Task Install(string packageId, string version, string solutionFilePath)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath);

            this.logger.LogInformation(string.Format("[{0}] Installing package \"{1}\"...", solutionDirectory, packageId));

            this.nuGetCliClient.InstallPackage(packageId, version, solutionDirectory, this.sources);

            this.logger.LogInformation(string.Format("[{0}] Install for package \"{1}\" is complete", solutionDirectory, packageId));
        }

        public async Task Restore(string solutionFilePath)
        {
            this.logger.LogInformation(string.Format("[{0}] Restoring packages staretd...", solutionFilePath));

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
            IEnumerable<string> sources = new List<string>() { SitefinityPublicNuGetSource };

            return await nuGetApiClient.GetPackageWithFullDependencyTree(Constants.SitefinityAllNuGetPackageId, version, sources, this.supportedFrameworksRegex);
        }


        public void SyncReferencesWithPackages(string projectPath, string solutionFolder, IEnumerable<NuGetPackage> packages, string sitefinityVersion)
        {
            this.logger.LogInformation(string.Format("Synchronizing packages and references for project '{0}'", projectPath));

            XmlDocument doc = new XmlDocument();
            doc.Load(projectPath);

            var processedAssemblies = new HashSet<string>();
            var projectLocation = projectPath.Substring(0, projectPath.LastIndexOf("\\") + 1);

            var projectConfigPath = this.projectConfigFileEditor.GetProjectConfigPath(projectLocation);
            XmlNodeList bindingRedirectNodes = null;
            XmlDocument projectConfig = null;
            if (!string.IsNullOrEmpty(projectConfigPath))
            {
                projectConfig = new XmlDocument();
                projectConfig.Load(projectConfigPath);
                bindingRedirectNodes = projectConfig.GetElementsByTagName("dependentAssembly");
            }

            var references = doc.GetElementsByTagName(Constants.ReferenceElem);
            var targetFramework = this.GetTargetFrameworkForVersion(sitefinityVersion);

            this.SetTargetFramework(doc, targetFramework);
            // Foreach package installed for this project, check if all DLLs are included. If not - include missing ones. Fix binding redirects in web.config if necessary.
            foreach (var package in packages)
            {
                var packageDir = string.Format("{0}\\{1}\\{2}.{3}", solutionFolder, PackagesFolderName, package.Id, package.Version);
                foreach (var dllFile in this.GetPackageDlls(packageDir, targetFramework))
                {
                    var assembly = Assembly.LoadFile(dllFile);
                    string assemblyVersion = assembly.GetName().Version.ToString();
                    var assemblyFullName = assembly.FullName;
                    if (!processedAssemblies.Contains(assemblyFullName))
                    {
                        processedAssemblies.Add(assemblyFullName);

                        var assemblyName = assemblyFullName.Split(",").First();
                        bool assemblyReferenceFound = false;
                        for (int i = 0; i < references.Count; i++)
                        {
                            var referenceElement = references[i];
                            var includeAttr = referenceElement.Attributes[Constants.IncludeAttribute];
                            var includeAttrValue = includeAttr.Value;

                            if (includeAttrValue.StartsWith(assemblyName + ",") || includeAttrValue == assemblyName)
                            {
                                var proccesorArchitecture = includeAttrValue.Split(',').FirstOrDefault(x => x.Contains(ProcessorArchitectureAttribute));
                                var includeAttributeNewValue = string.IsNullOrEmpty(proccesorArchitecture) ? assemblyFullName : $"{assemblyFullName},{proccesorArchitecture}";

                                includeAttr.Value = includeAttributeNewValue;
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

                                var hintPathValue = GetRelativePathTo(projectLocation, dllFile);

                                // Hint path missing, so we add it
                                if (hintPathNode == null)
                                {
                                    this.logger.LogInformation(string.Format("Added hint path for reference assembly '{0}'", assemblyFullName));

                                    hintPathNode = doc.CreateElement(Constants.HintPathElem, doc.DocumentElement.NamespaceURI);
                                    referenceElement.AppendChild(hintPathNode);
                                    hintPathNode.InnerText = hintPathValue;
                                }
                                else if (hintPathNode.InnerText != hintPathValue)
                                {
                                    // TODO: we can load the currently referenced assembly and replace the hint path only if the assemblie version is different. There are cases when one dll is located in multiple packages
                                    this.logger.LogInformation(string.Format("Fixing broken hint path for reference assembly '{0}' from '{1}' to '{2}'", assemblyFullName, hintPathNode.InnerText, hintPathValue));

                                    hintPathNode.InnerText = hintPathValue;
                                }

                                assemblyReferenceFound = true;
                                break;
                            }
                        }

                        // DLL reference is missing, so we add it.
                        if (!assemblyReferenceFound)
                        {
                            this.logger.LogInformation(string.Format("Added missing assembly reference '{0}' from package '{1}'", assemblyFullName, package.Id));

                            var referencesGroup = doc.GetElementsByTagName(Constants.ItemGroupElem)[0];
                            var referenceNode = doc.CreateElement(Constants.ReferenceElem, doc.DocumentElement.NamespaceURI);
                            var includeAttr = doc.CreateAttribute(Constants.IncludeAttribute);
                            includeAttr.Value = assemblyFullName;
                            referenceNode.Attributes.Append(includeAttr);
                            var hintPathNode = doc.CreateElement(Constants.HintPathElem, doc.DocumentElement.NamespaceURI);
                            hintPathNode.InnerText = GetRelativePathTo(projectLocation, dllFile);
                            referenceNode.AppendChild(hintPathNode);
                            referencesGroup.AppendChild(referenceNode);
                        }
                    }

                    this.SyncBindingRedirects(projectConfig, bindingRedirectNodes, assembly.GetName().Name, assemblyVersion);
                }
            }

            doc.Save(projectPath);
            projectConfig?.Save(projectConfigPath);

            this.logger.LogInformation(string.Format("Synchronization completed for project '{0}'", projectPath));
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
                            var oldVersionAttribute = bindingRedirect.Attributes[oldVersionAttributeName];
                            if (oldVersionAttribute == null)
                            {
                                oldVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(oldVersionAttribute);
                            }

                            oldVersionAttribute.Value = string.Format("0.0.0.0-{0}", assemblyVersion);

                            var newVersionAttribute = bindingRedirect.Attributes[newVersionAttributeName];
                            if (newVersionAttribute == null)
                            {
                                newVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(newVersionAttribute);
                            }

                            newVersionAttribute.Value = assemblyVersion;

                            break;
                        }
                    }
                }
            }
        }

        private void SetTargetFramework(XmlDocument doc, string targetFramework)
        {
            var targetFrameworkVersionElems = doc.GetElementsByTagName(Constants.TargetFrameworkVersionElem);
            if (targetFrameworkVersionElems.Count == 1)
            {
                targetFrameworkVersionElems[0].InnerText = targetFramework;
                return;
            }

            throw new InvalidOperationException("Unable to set the target framework");
        }

        private string GetTargetFrameworkForVersion(string version)
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
            else if (versionAsInt >= 120)
            {
                return "v4.7.2";
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
                        int.TryParse(subDir.Split("\\").Last().Replace(DotNetPrefix, string.Empty), out int currentFolderFrameworkVersion);
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

        private static string GetRelativePathTo(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        private readonly INuGetApiClient nuGetApiClient;

        private readonly INuGetCliClient nuGetCliClient;

        private readonly IPackagesConfigFileEditor packagesConfigFileEditor;

        private readonly IProjectConfigFileEditor projectConfigFileEditor;

        private readonly ILogger logger;

        private readonly IEnumerable<string> sources;

        private const string SitefinityPublicNuGetSource = "http://nuget.sitefinity.com/nuget/";

        private const string PublicNuGetSource = "https://nuget.org/api/v2/";

        private const string PackagesFolderName = "packages";

        private const string ToolsFolderName = "tools";

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
