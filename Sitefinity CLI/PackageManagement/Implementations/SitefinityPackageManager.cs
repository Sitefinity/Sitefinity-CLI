﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.VisualStudio;

namespace Sitefinity_CLI.PackageManagement.Implementations
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
            defaultSources = new List<NugetPackageSource>()
            {
               new NugetPackageSource(SitefinityPublicNuGetSource),
               new NugetPackageSource(PublicNuGetSourceV3)
            };
            supportedFrameworksRegex = new Regex("^net[0-9]*$", RegexOptions.Compiled);
            systemAssembliesNotToUpdate = new HashSet<string>() { "System.Runtime", "System.IO" };
        }

        public void Install(string packageId, string version, string solutionFilePath, string nugetConfigFilePath)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath);

            logger.LogInformation($"[{solutionDirectory}] Installing package '{packageId}'...");


            nuGetCliClient.InstallPackage(packageId, version, solutionDirectory, nugetConfigFilePath);

            logger.LogInformation($"[{solutionDirectory}] Install for package '{packageId}' completed");
        }

        public void Restore(string solutionFilePath)
        {
            logger.LogInformation($"[{solutionFilePath}] Restoring packages started...");

            nuGetCliClient.Restore(solutionFilePath);

            logger.LogInformation($"[{solutionFilePath}] Restoring packages completed");
        }

        public bool PackageExists(string packageId, string projectFilePath)
        {
            string packagesConfigFilePath = GetPackagesConfigFilePathForProject(projectFilePath);

            NuGetPackage nuGetPackage = packagesConfigFileEditor.FindPackage(packagesConfigFilePath, packageId);
            if (nuGetPackage != null)
            {
                return true;
            }

            return false;
        }

        public async Task<NuGetPackage> GetSitefinityPackageTree(string version)
        {
            return await GetSitefinityPackageTree(version, defaultSources);
        }

        public async Task<NuGetPackage> GetSitefinityPackageTree(string version, IEnumerable<NugetPackageSource> nugetPackageSources)
        {
            var sourcesUsed = string.Join(',', nugetPackageSources?.Select(x => x.SourceUrl));
            logger.LogInformation($"Package sources used: {sourcesUsed}");

            return await nuGetApiClient.GetPackageWithFullDependencyTree(Constants.SitefinityAllNuGetPackageId, version, nugetPackageSources, supportedFrameworksRegex);
        }

        public async Task<NuGetPackage> GetPackageTree(string id, string version, IEnumerable<NugetPackageSource> nugetPackageSources, Func<NuGetPackage, bool> shouldBreakSearch = null)
        {
            return await nuGetApiClient.GetPackageWithFullDependencyTree(id, version, nugetPackageSources, supportedFrameworksRegex, shouldBreakSearch);
        }

        public void SyncReferencesWithPackages(string projectFilePath, string solutionDir)
        {
            logger.LogInformation($"Synchronizing packages and references for project '{projectFilePath}'");

            string packagesConfigFilePath = GetPackagesConfigFilePathForProject(projectFilePath);
            IEnumerable<NuGetPackage> packages = packagesConfigFileEditor.GetPackages(packagesConfigFilePath);

            XmlDocument projectFileXmlDocument = new XmlDocument();
            projectFileXmlDocument.Load(projectFilePath);

            var processedAssemblies = new HashSet<string>();
            var projectDir = Path.GetDirectoryName(projectFilePath);

            var projectConfigPath = projectConfigFileEditor.GetProjectConfigPath(projectDir);
            XmlNodeList bindingRedirectNodes = null;
            XmlDocument projectConfig = null;
            if (!string.IsNullOrEmpty(projectConfigPath))
            {
                projectConfig = new XmlDocument();
                projectConfig.Load(projectConfigPath);
                bindingRedirectNodes = projectConfig.GetElementsByTagName("dependentAssembly");
            }

            XmlNodeList referenceElements = projectFileXmlDocument.GetElementsByTagName(Constants.ReferenceElem);
            string targetFramework = GetTargetFramework(projectFileXmlDocument);

            IEnumerable<AssemblyReference> nugetPackageAssemblyReferences = GetAssemblyReferencesFromNuGetPackages(packages, targetFramework, projectDir, solutionDir);
            IEnumerable<IGrouping<string, AssemblyReference>> nuGetPackageAssemblyReferenceGroups = nugetPackageAssemblyReferences
                .Where(ar => ar.Version != null && !systemAssembliesNotToUpdate.Contains(ar.Name))
                .GroupBy(ar => ar.Name);

            // Foreach package installed for this project, check if all DLLs are included. If not - include missing ones. Fix binding redirects in web.config if necessary.
            foreach (IGrouping<string, AssemblyReference> nuGetPackageAssemblyReferenceGroup in nuGetPackageAssemblyReferenceGroups)
            {
                AddOrUpdateReferencesForAssembly(projectFileXmlDocument, referenceElements, bindingRedirectNodes, projectConfig, nuGetPackageAssemblyReferenceGroup.Key, nuGetPackageAssemblyReferenceGroup);
            }

            IEnumerable<string> nugetPackageRelativeFileReferences = GetRelativeFilePathsFromNuGetPackages(packages, projectDir, solutionDir);
            RemoveReferencesToMissingNuGetPackageDlls(projectDir, solutionDir, projectFileXmlDocument, nugetPackageRelativeFileReferences);

            projectFileXmlDocument.Save(projectFilePath);
            projectConfig?.Save(projectConfigPath);

            logger.LogInformation($"Synchronization completed for project '{projectFilePath}'");
        }

        public async Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> packageSources, int versionsCount = 10)
        {
            return await nuGetApiClient.GetPackageVersions(id, packageSources, versionsCount);
        }

        private void RemoveReferencesToMissingNuGetPackageDlls(string projectDir, string solutionDir, XmlDocument projectFileXmlDocument, IEnumerable<string> nugetPackageRelativeFileReferences)
        {
            string packagesDir = Path.Combine(solutionDir, PackagesFolderName);
            string relativePackagesDirPath = GetRelativePathTo(projectDir + "\\", packagesDir);

            XmlNodeList elementsWithIncludeAttribute = projectFileXmlDocument.SelectNodes("//*[@Include]");
            for (int i = 0; i < elementsWithIncludeAttribute.Count; i++)
            {
                XmlNode elementWithIncludeAttribute = elementsWithIncludeAttribute[i];
                XmlAttribute includeAttr = elementWithIncludeAttribute.Attributes[Constants.IncludeAttribute];
                string includeAttributeValue = includeAttr.Value;

                if (includeAttributeValue.StartsWith(relativePackagesDirPath, StringComparison.OrdinalIgnoreCase) &&
                    !nugetPackageRelativeFileReferences.Any(fr => fr.Equals(includeAttributeValue, StringComparison.OrdinalIgnoreCase)))
                {
                    logger.LogInformation($"Removing '{elementWithIncludeAttribute.Name}' element with include attribute '{includeAttributeValue}', because file cannot be found in NuGet packages installed for this project.");
                    elementWithIncludeAttribute.ParentNode.RemoveChild(elementWithIncludeAttribute);
                }
            }
        }
        public async Task<IEnumerable<NugetPackageSource>> GetNugetPackageSources(string nugetConfigFilePath)
        {
            if (string.IsNullOrEmpty(nugetConfigFilePath))
            {
                throw new ArgumentException(nameof(nugetConfigFilePath));
            }

            var packageSourceList = new List<NugetPackageSource>();

            var fileContent = await File.ReadAllTextAsync(nugetConfigFilePath);
            XDocument nuGetPackageXmlDoc = XDocument.Parse(fileContent);
            var xmlPackageSources = nuGetPackageXmlDoc.Root?.Element("packageSources")?.Elements().Where(e => e.Name == "add");
            var packageSourceCredentials = nuGetPackageXmlDoc.Root?.Element("packageSourceCredentials");

            foreach (var xmlPackageSource in xmlPackageSources)
            {
                string packageSourceName = xmlPackageSource.Attribute("key")?.Value;
                string packageSourceUrl = xmlPackageSource.Attribute("value")?.Value;

                if (!string.IsNullOrEmpty(packageSourceName) && !string.IsNullOrEmpty(packageSourceUrl))
                {
                    var nugetSource = new NugetPackageSource();
                    nugetSource.SourceName = packageSourceName;
                    nugetSource.SourceUrl = packageSourceUrl;
                    TryAddPackageCredentialsToSource(packageSourceCredentials, packageSourceName, nugetSource);
                    packageSourceList.Add(nugetSource);
                }
            }

            return packageSourceList;
        }

        private void TryAddPackageCredentialsToSource(XElement packageSourceCredentials, string packageSourceName, NugetPackageSource nugetSource)
        {
            if (packageSourceCredentials != null)
            {
                if (packageSourceName.Any(c => char.IsWhiteSpace(c)))
                {
                    logger.LogError("The package source: {packageSource} contains white space char. If you have <packageSourceCredentials> element for it it won't be extracted", nugetSource.SourceName);
                    return;
                }

                var packageCredentials = packageSourceCredentials.Element(packageSourceName);
                if (packageCredentials != null)
                {
                    var userName = packageCredentials.Descendants().FirstOrDefault(e => (string)e.Attribute("key") == "Username");
                    var passWord = packageCredentials.Descendants().FirstOrDefault(e => (string)e.Attribute("key") == "ClearTextPassword");

                    if (userName != null && passWord != null)
                    {
                        nugetSource.Username = userName.Attribute("value")?.Value;
                        nugetSource.Password = passWord.Attribute("value")?.Value;

                        if (string.IsNullOrEmpty(nugetSource.Username) || string.IsNullOrEmpty(nugetSource.Password))
                        {
                            logger.LogError("Error while retrieveing credentials for source: {packageSource}.", nugetSource.SourceUrl);
                            throw new UpgradeException("Upgrade failed due to errors reading the provided nugetConfig");
                        }
                    }
                }

            }
        }

        private void AddOrUpdateReferencesForAssembly(XmlDocument projectFileXmlDocument, XmlNodeList referenceElements, XmlNodeList bindingRedirectNodes, XmlDocument projectConfig, string assemblyName, IEnumerable<AssemblyReference> nugetPackageAssemblyReferences)
        {
            AssemblyReference nugetPackageAssemblyReferenceWithNewestVersion = nugetPackageAssemblyReferences.OrderByDescending(ar => ar.Version).First();

            bool isAssemblyReferenceFound = false;
            for (int i = 0; i < referenceElements.Count; i++)
            {
                XmlNode referenceElement = referenceElements[i];
                XmlAttribute includeAttribute = referenceElement.Attributes[Constants.IncludeAttribute];

                if (!string.IsNullOrWhiteSpace(includeAttribute.Value) &&
                    (includeAttribute.Value.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) || includeAttribute.Value.StartsWith(assemblyName + ",", StringComparison.OrdinalIgnoreCase)))
                {
                    Version currentAssemblyVersion = ExtractAssemblyVersionFromIncludeAttribute(includeAttribute.Value);

                    if (currentAssemblyVersion != null && currentAssemblyVersion > nugetPackageAssemblyReferenceWithNewestVersion.Version)
                    {
                        logger.LogInformation($"The assembly reference '{assemblyName}' is on version '{currentAssemblyVersion}'. It won't be downgraded to version '{nugetPackageAssemblyReferenceWithNewestVersion.Version}'.");
                        isAssemblyReferenceFound = true;
                        break;
                    }

                    string proccesorArchitecture = includeAttribute.Value.Split(',').FirstOrDefault(x => x.Contains(ProcessorArchitectureAttribute));
                    string includeAttributeNewValue = string.IsNullOrEmpty(proccesorArchitecture) ? nugetPackageAssemblyReferenceWithNewestVersion.FullName : $"{nugetPackageAssemblyReferenceWithNewestVersion.FullName},{proccesorArchitecture}";

                    if (!includeAttribute.Value.Equals(includeAttributeNewValue, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogInformation($"Updated include attribue '{includeAttribute.Value}' to '{includeAttributeNewValue}'.");
                        includeAttribute.Value = includeAttributeNewValue;
                    }

                    XmlNode hintPathNode = GetChildNode(referenceElement, Constants.HintPathElem);
                    if (hintPathNode == null)
                    {
                        logger.LogInformation($"Added hint path '{nugetPackageAssemblyReferenceWithNewestVersion.HintPath}' for reference assembly '{nugetPackageAssemblyReferenceWithNewestVersion.FullName}'.");

                        hintPathNode = projectFileXmlDocument.CreateElement(Constants.HintPathElem, projectFileXmlDocument.DocumentElement.NamespaceURI);
                        referenceElement.AppendChild(hintPathNode);
                        hintPathNode.InnerText = nugetPackageAssemblyReferenceWithNewestVersion.HintPath;
                    }
                    else if (!nugetPackageAssemblyReferences.Any(ar => ar.Version == nugetPackageAssemblyReferenceWithNewestVersion.Version && ar.HintPath.Equals(hintPathNode.InnerText, StringComparison.OrdinalIgnoreCase)))
                    {
                        logger.LogInformation($"Updated hint path '{hintPathNode.InnerText}' to '{nugetPackageAssemblyReferenceWithNewestVersion.HintPath}' for reference assembly '{nugetPackageAssemblyReferenceWithNewestVersion.FullName}'.");

                        hintPathNode.InnerText = nugetPackageAssemblyReferenceWithNewestVersion.HintPath;
                    }

                    isAssemblyReferenceFound = true;
                    break;
                }
            }

            if (!isAssemblyReferenceFound)
            {
                logger.LogInformation($"Added missing assembly reference '{nugetPackageAssemblyReferenceWithNewestVersion.FullName}' with hint path '{nugetPackageAssemblyReferenceWithNewestVersion.HintPath}'.");

                XmlNode referencesGroup = projectFileXmlDocument.GetElementsByTagName(Constants.ItemGroupElem)[0];
                XmlElement referenceNode = projectFileXmlDocument.CreateElement(Constants.ReferenceElem, projectFileXmlDocument.DocumentElement.NamespaceURI);

                XmlAttribute includeAttr = projectFileXmlDocument.CreateAttribute(Constants.IncludeAttribute);
                includeAttr.Value = nugetPackageAssemblyReferenceWithNewestVersion.FullName;
                referenceNode.Attributes.Append(includeAttr);

                XmlElement hintPathNode = projectFileXmlDocument.CreateElement(Constants.HintPathElem, projectFileXmlDocument.DocumentElement.NamespaceURI);
                hintPathNode.InnerText = nugetPackageAssemblyReferenceWithNewestVersion.HintPath;

                referenceNode.AppendChild(hintPathNode);
                referencesGroup.AppendChild(referenceNode);
            }

            SyncBindingRedirects(projectConfig, bindingRedirectNodes, assemblyName, nugetPackageAssemblyReferenceWithNewestVersion.Version.ToString());
        }

        private XmlNode GetChildNode(XmlNode node, string childNodeName)
        {
            XmlNodeList childNodes = node.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                var childNode = childNodes[i];
                if (childNode.Name == childNodeName)
                {
                    return childNode;
                }
            }

            return null;
        }

        private IEnumerable<string> GetRelativeFilePathsFromNuGetPackages(IEnumerable<NuGetPackage> nuGetPackages, string projectDir, string solutionDir)
        {
            List<string> filePaths = new List<string>();
            foreach (NuGetPackage nuGetPackage in nuGetPackages)
            {
                string packageDir = GetNuGetPackageDir(solutionDir, nuGetPackage);
                filePaths.AddRange(Directory.GetFiles(packageDir, "*.*", SearchOption.AllDirectories));
            }

            return filePaths.Select(fp => GetRelativePathTo(projectDir + "\\", fp));
        }

        private IEnumerable<AssemblyReference> GetAssemblyReferencesFromNuGetPackages(IEnumerable<NuGetPackage> nuGetPackages, string targetFramework, string projectDir, string solutionDir)
        {
            List<string> dllFilePaths = new List<string>();
            foreach (NuGetPackage nuGetPackage in nuGetPackages)
            {
                string packageDir = GetNuGetPackageDir(solutionDir, nuGetPackage);
                dllFilePaths.AddRange(GetPackageDlls(packageDir, targetFramework));
            }

            IEnumerable<AssemblyReference> assemblyReferences = dllFilePaths.Distinct().Select(d => GetAssemblyReferenceFromDllFilePath(d, projectDir));

            return assemblyReferences;
        }

        private string GetNuGetPackageDir(string solutionDir, NuGetPackage nuGetPackage)
        {
            string nuGetPackageFolderName = $"{nuGetPackage.Id}.{nuGetPackage.Version}";

            return Path.Combine(solutionDir, PackagesFolderName, nuGetPackageFolderName);
        }

        private AssemblyReference GetAssemblyReferenceFromDllFilePath(string dllFilePath, string projectDir)
        {
            Assembly assembly = Assembly.LoadFile(dllFilePath);
            AssemblyName assemblyName = assembly.GetName();

            AssemblyReference assemblyReference = new AssemblyReference();
            assemblyReference.Name = assemblyName.Name;
            assemblyReference.Version = assemblyName.Version;
            assemblyReference.FullName = assemblyName.FullName;
            assemblyReference.HintPath = GetRelativePathTo(projectDir + "\\", dllFilePath);

            return assemblyReference;
        }

        private Version ExtractAssemblyVersionFromIncludeAttribute(string includeAttributeValue)
        {
            string versionChunk = includeAttributeValue
                .Split(',')
                .FirstOrDefault(x => x.Contains("Version="));

            if (string.IsNullOrWhiteSpace(versionChunk))
            {
                logger.LogInformation($"Unable to extract the version from '{includeAttributeValue}'.");

                return null;
            }

            string assemblyVersionString = versionChunk
                .Split("=")
                .ToList()[1];

            Version parsedVersion = null;
            if (!Version.TryParse(assemblyVersionString, out parsedVersion))
            {
                logger.LogInformation($"Unable to parse version string '{assemblyVersionString}'.");
            }

            return parsedVersion;
        }

        public IEnumerable<NugetPackageSource> DefaultPackageSource
        {
            get
            {
                return new List<NugetPackageSource>(defaultSources);
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
                        if (childNode.Name == AssemblyIdentityAttributeName)
                            assemblyIdentity = childNode;

                        if (childNode.Name == BindingRedirectAttributeName)
                            bindingRedirect = childNode;
                    }

                    if (assemblyIdentity != null && bindingRedirect != null)
                    {
                        var name = assemblyIdentity.Attributes["name"]?.Value;
                        if (name == assemblyFullName)
                        {
                            var newVersionAttribute = bindingRedirect.Attributes[NewVersionAttributeName];
                            if (newVersionAttribute != null && !ShouldUpdateBindingRedirect(newVersionAttribute.Value, assemblyVersion))
                            {
                                break;
                            }

                            if (newVersionAttribute == null)
                            {
                                newVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(newVersionAttribute);
                            }

                            newVersionAttribute.Value = assemblyVersion;

                            var oldVersionAttribute = bindingRedirect.Attributes[OldVersionAttributeName];
                            if (oldVersionAttribute == null)
                            {
                                oldVersionAttribute = configDoc.CreateAttribute(Constants.IncludeAttribute);

                                bindingRedirect.Attributes.Append(oldVersionAttribute);
                            }

                            oldVersionAttribute.Value = $"0.0.0.0-{assemblyVersion}";

                            break;
                        }
                    }
                }
            }
        }

        private static bool ShouldUpdateBindingRedirect(string oldAssemblyVersion, string newAssemblyVersion)
        {
            var oldVersion = Version.Parse(oldAssemblyVersion);
            var newVersion = Version.Parse(newAssemblyVersion);

            return newVersion > oldVersion;
        }

        private static bool TrySetTargetFramework(XmlDocument doc, string targetFramework)
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

        private static string GetTargetFramework(XmlDocument doc)
        {
            XmlNodeList targetFrameworkVersionElems = doc.GetElementsByTagName(Constants.TargetFrameworkVersionElem);
            if (targetFrameworkVersionElems.Count != 1)
            {
                throw new InvalidOperationException("Unable to get the target framework");
            }

            return targetFrameworkVersionElems[0].InnerText;
        }

        private static string GetTargetFrameworkForSitefinityVersion(string version)
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
        internal IEnumerable<string> GetPackageDlls(string packagePath, string targetVersion)
        {
            // v4.7.2 -> 472
            string versionPart = targetVersion.Replace(".", string.Empty).Replace("v", string.Empty);
            if (!int.TryParse(versionPart, out int targetFrameworkVersion) || targetFrameworkVersion == 0)
            {
                return Array.Empty<string>();
            }

            string libDir = Path.Combine(packagePath, LibFolderName);
            string dllStorageDir = null;
            if (Directory.Exists(libDir))
            {
                if (Directory.GetDirectories(libDir).Any())
                {
                    // we check for the highest possible .net framework version of the dll
                    dllStorageDir = GetDllStoragePathForNetFramework(targetFrameworkVersion, libDir);

                    if (string.IsNullOrEmpty(dllStorageDir))
                    {
                        // if there is no .net framework dll we check for .netstandard
                        dllStorageDir = GetDllStoragePathForNetStandart(libDir);
                    }
                }
            }
            else
            {
                dllStorageDir = packagePath;
            }

            return dllStorageDir != null && Directory.Exists(dllStorageDir) ? Directory.GetFiles(dllStorageDir, DllFilterPattern) : Array.Empty<string>();
        }

        private static string GetDllStoragePathForNetStandart(string libDir)
        {
            string dllStorageDir = null;
            int highestNetStandardDirFrameworkVersion = 0;

            string[] netStandardDirNames = Directory.GetDirectories(libDir, DotNetStandardPrefix + "*");
            foreach (string netStandardDirName in netStandardDirNames)
            {
                // netstandard2.0 => 2.0 => 20
                string netStandardDirFrameworkVersionString = netStandardDirName
                    .Split("\\")
                    .Last()
                    .Replace(DotNetStandardPrefix, string.Empty)
                    .Replace(".", string.Empty);

                if (int.TryParse(netStandardDirFrameworkVersionString, out int netStandardDirFrameworkVersion) &&
                    netStandardDirFrameworkVersion != 0 &&
                    netStandardDirFrameworkVersion <= 20 &&
                    netStandardDirFrameworkVersion > highestNetStandardDirFrameworkVersion)
                {
                    dllStorageDir = netStandardDirName;
                    highestNetStandardDirFrameworkVersion = netStandardDirFrameworkVersion;
                }
            }

            return dllStorageDir;
        }

        private static string GetDllStoragePathForNetFramework(int targetFrameworkVersion, string libDir)
        {
            if (targetFrameworkVersion.ToString().Length == 2)
            {
                // Fix for the cases when we upgrade from versions with different length - 4.7.1 to 4.8
                targetFrameworkVersion *= 10;
            }

            string dllStorageDir = null;
            int highestFolderFrameworkVersion = 0;
            IEnumerable<string> netFrameworkDirNames = Directory.GetDirectories(libDir)
                .Where(dirName => dirName.Contains(DotNetPrefix) && !dirName.Contains(DotNetStandardPrefix));

            foreach (string netFrameworkDirName in netFrameworkDirNames)
            {
                var netFrameworkDirVersionString = netFrameworkDirName.Split("\\").Last().Replace(DotNetPrefix, string.Empty);

                // The folder may have "-" in the name
                if (netFrameworkDirVersionString.Contains('-'))
                {
                    netFrameworkDirVersionString = netFrameworkDirVersionString.Split("-").FirstOrDefault();
                }

                if (int.TryParse(netFrameworkDirVersionString, out int netFrameworkDirVersion))
                {
                    if (netFrameworkDirVersion.ToString().Length == 2)
                    {
                        // Fix for the cases when we upgrade from versions with different length - 4.7.1 to 4.8
                        netFrameworkDirVersion *= 10;
                    }

                    if (netFrameworkDirVersion != 0 &&
                        netFrameworkDirVersion <= targetFrameworkVersion &&
                        netFrameworkDirVersion > highestFolderFrameworkVersion)
                    {
                        dllStorageDir = netFrameworkDirName;
                        highestFolderFrameworkVersion = netFrameworkDirVersion;
                    }
                }
            }

            return dllStorageDir;
        }

        private static string GetPackagesConfigFilePathForProject(string projectFilePath)
        {
            string projectDirectory = Path.GetDirectoryName(projectFilePath);
            string packagesConfigFilePath = Path.Combine(projectDirectory, Constants.PackagesConfigFileName);

            if (!File.Exists(packagesConfigFilePath))
            {
                throw new FileNotFoundException($"File '{Constants.PackagesConfigFileName}' not found in project directory '{projectDirectory}'. Cannot proceed with the upgrade.");
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

        public void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"Invalid version: {version}");
            }

            var targetFramework = GetTargetFrameworkForSitefinityVersion(version);

            foreach (var projectFilePath in sitefinityProjectFilePaths)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(projectFilePath);

                if (TrySetTargetFramework(doc, targetFramework))
                {
                    doc.Save(projectFilePath);

                    logger.LogInformation(string.Format(Constants.TargetFrameworkChanged, Path.GetFileName(projectFilePath), targetFramework));
                }
                else
                {
                    logger.LogInformation(string.Format(Constants.TargetFrameworkDoesNotNeedChanged, Path.GetFileName(projectFilePath), targetFramework));
                }
            }
        }
        // remove
        private readonly INuGetApiClient nuGetApiClient;

        private readonly INuGetCliClient nuGetCliClient;

        private readonly IPackagesConfigFileEditor packagesConfigFileEditor;

        private readonly IProjectConfigFileEditor projectConfigFileEditor;

        private readonly ILogger logger;

        private readonly IEnumerable<NugetPackageSource> defaultSources;

        private readonly Regex supportedFrameworksRegex;

        private readonly HashSet<string> systemAssembliesNotToUpdate;

        private const string SitefinityPublicNuGetSource = "https://nuget.sitefinity.com/nuget/";

        private const string PublicNuGetSourceV3 = "https://api.nuget.org/v3/index.json";
        //private const string PublicNuGetSourceV3 = "https://www.nuget.org/api/v2";

        private const string PackagesFolderName = "packages";

        private const string LibFolderName = "lib";

        private const string DotNetPrefix = "net";

        private const string DllFilterPattern = "*.dll";

        private const string ProcessorArchitectureAttribute = "processorArchitecture";

        private const string AssemblyIdentityAttributeName = "assemblyIdentity";

        private const string BindingRedirectAttributeName = "bindingRedirect";

        private const string OldVersionAttributeName = "oldVersion";

        private const string NewVersionAttributeName = "newVersion";

        private const string DotNetStandardPrefix = "netstandard";
    }
}