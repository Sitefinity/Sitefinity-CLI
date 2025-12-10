using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// A class used to manage the contents of a solution file.
    /// </summary>
    public static class SolutionFileEditor
    {
        /// <summary>
        /// Returns the projects from a solution file.
        /// </summary>
        /// <param name="solutionFilePath">The path to the solution file.</param>
        /// <returns>Collection of <see cref="ISolutionProject"/>.</returns>
        public static IEnumerable<ISolutionProject> GetProjects(string solutionFilePath)
        {
            var extension = Path.GetExtension(solutionFilePath);
            IEnumerable<ISolutionProject> solutionProjects = Enumerable.Empty<ISolutionProject>();

            if (extension.Equals(Constants.SlnFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                solutionProjects = GetProjectsFromSln(solutionFilePath);
            }
            else if (extension.Equals(Constants.SlnxFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                solutionProjects = GetProjectsFromSlnx(solutionFilePath);
            }

            return solutionProjects;
        }

        /// <summary>
        /// Adds reference to a project file in a solution. 
        /// </summary>
        /// <param name="solutionFilePath">The solution file path</param>
        /// <param name="projectAbsoluteFilePath">The project absolute file path to add</param>
        public static void AddProject(Guid projectId, string solutionFilePath, string projectAbsoluteFilePath)
        {
            string extension = Path.GetExtension(solutionFilePath);

            if (extension.Equals(Constants.SlnFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                SlnSolutionProject solutionProject = new SlnSolutionProject(projectId, projectAbsoluteFilePath, solutionFilePath, SolutionProjectType.ManagedCsProject);
                AddProjectToSln(solutionFilePath, solutionProject);
            }
            else if (extension.Equals(Constants.SlnxFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                SlnxSolutionProject solutionProject = new SlnxSolutionProject(projectId, projectAbsoluteFilePath, solutionFilePath, SolutionProjectType.ManagedCsProject);
                AddProjectToSlnx(solutionFilePath, solutionProject);
            }
        }

        /// <summary>
        /// Adds reference to a project file in a sln solution. 
        /// </summary>
        /// <param name="solutionFilePath">The solution file path</param>
        /// <param name="solutionProject">The project to add</param>
        private static void AddProjectToSln(string solutionFilePath, SlnSolutionProject solutionProject)
        {
            string solutionFileContent = GetSolutionFileContentAsString(solutionFilePath);

            solutionFileContent = AddProjectInfoInSolutionFileContent(solutionFileContent, solutionProject);
            solutionFileContent = AddProjectGlobalSectionInSolutionFileContent(solutionFileContent, solutionProject);

            SaveSolutionFileContent(solutionFilePath, solutionFileContent);
        }

        /// <summary>
        /// Returns the projects from a sln solution file.
        /// </summary>
        /// <param name="solutionFilePath">The path to the solution file.</param>
        /// <returns>Collection of <see cref="SlnSolutionProject"/>.</returns>
        private static IEnumerable<SlnSolutionProject> GetProjectsFromSln(string solutionFilePath)
        {
            string solutionFileContent = GetSolutionFileContentAsString(solutionFilePath);

            IList<SlnSolutionProject> solutionProjects = new List<SlnSolutionProject>();
            foreach (Match match in projectLineRegex.Matches(solutionFileContent))
            {
                Guid projectTypeGuid = Guid.Parse(match.Groups[ProjectTypeGuidRegexGroupName].Value.Trim());
                string projectName = match.Groups[ProjectNameRegexGroupName].Value.Trim();
                string relativePath = match.Groups[ProjectRelativePathRegexGroupName].Value.Trim();
                Guid projectGuid = Guid.Parse(match.Groups[ProjectGuidRegexGroupName].Value.Trim());

                SlnSolutionProject solutionProject = new SlnSolutionProject(projectGuid, projectName, relativePath, projectTypeGuid, solutionFilePath);
                solutionProjects.Add(solutionProject);
            }

            return solutionProjects;
        }

        /// <summary>
        /// Returns the projects from a slnx file.
        /// </summary>
        /// <param name="solutionFilePath">The path to the solution file.</param>
        /// <returns>Collection of <see cref="SlnxSolutionProject"/>.</returns>
        private static IEnumerable<SlnxSolutionProject> GetProjectsFromSlnx(string solutionFilePath)
        {
            var solutionProjects = new List<SlnxSolutionProject>();
            XDocument doc = XDocument.Load(solutionFilePath);

            foreach (var projectElement in doc.Descendants(ProjectElementName))
            {
                var pathAttr = projectElement.Attribute(PathAttributeName);
                var idAttr = projectElement.Attribute(IdAttributeName);
                if (pathAttr != null && idAttr != null)
                {
                    string normalizedPath = pathAttr.Value.Replace('/', Path.DirectorySeparatorChar);
                    Guid projectId = Guid.Parse(idAttr.Value);
                    SlnxSolutionProject project = new SlnxSolutionProject(projectId, normalizedPath, solutionFilePath);
                    solutionProjects.Add(project);
                }
            }

            return solutionProjects;
        }

        /// <summary>
        /// Adds reference to a project file in a slnx solution. 
        /// </summary>
        /// <param name="solutionFilePath">The solution file path</param>
        /// <param name="solutionProject">The project to add</param>
        private static void AddProjectToSlnx(string solutionFilePath, SlnxSolutionProject solutionProject)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"Unable to find {solutionFilePath}");
            }

            try
            {
                XDocument doc = XDocument.Load(solutionFilePath);
                string normalizedPath = solutionProject.RelativePath.Replace(Path.DirectorySeparatorChar, '/');

                var projectExisting = doc.Descendants(ProjectElementName).Any(p => p.Attribute(PathAttributeName)?.Value == normalizedPath &&
                    p.Attribute(IdAttributeName)?.Value == solutionProject.ProjectId.ToString());

                if (!projectExisting)
                {
                    // Find the root element (should be <Solution>)
                    var root = doc.Root;
                    if (root == null || root.Name.LocalName != SolutionRootName)
                    {
                        throw new Exception(Constants.SolutionNotReadable);
                    }

                    var newProject = new XElement(
                        ProjectElementName,
                        new XAttribute(PathAttributeName, normalizedPath),
                        new XAttribute(IdAttributeName, solutionProject.ProjectId.ToString()));

                    root.Add(newProject);
                    doc.Save(solutionFilePath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(Constants.AddFilesInsufficientPrivilegesMessage);
            }
            catch
            {
                throw new Exception(Constants.AddFilesToSolutionFailureMessage);
            }
        }

        private static string GetSolutionFileContentAsString(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Unable to find {filePath}");
            }

            return File.ReadAllText(filePath);
        }

        private static void SaveSolutionFileContent(string filePath, string fileContent)
        {
            try
            {
                File.WriteAllText(filePath, fileContent);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(Constants.AddFilesInsufficientPrivilegesMessage);
            }
            catch
            {
                throw new Exception(Constants.AddFilesToSolutionFailureMessage);
            }
        }

        private static string AddProjectInfoInSolutionFileContent(string solutionFileContent, SlnSolutionProject solutionProject)
        {
            var endProjectIndex = solutionFileContent.LastIndexOf("EndProject");
            if (endProjectIndex < 0)
            {
                throw new Exception(Constants.SolutionNotReadable);
            }

            var projectInfo = GenerateProjectInfoFromSolutionProject(solutionProject);

            solutionFileContent = solutionFileContent.Insert(endProjectIndex, projectInfo);

            return solutionFileContent;
        }

        private static string AddProjectGlobalSectionInSolutionFileContent(string solutionFileContent, SlnSolutionProject solutionProject)
        {
            int beginGlobalSectionIndex = solutionFileContent.IndexOf("GlobalSection(ProjectConfigurationPlatforms)");
            int endGlobalSectionIndex = solutionFileContent.IndexOf("EndGlobalSection", beginGlobalSectionIndex);

            if (endGlobalSectionIndex < 0)
            {
                throw new Exception(Constants.SolutionNotReadable);
            }

            var globalSection = GenerateGlobalSectionFromSolutionProject(solutionProject);
            solutionFileContent = solutionFileContent.Insert(endGlobalSectionIndex, globalSection);

            return solutionFileContent;
        }

        private static string GenerateGlobalSectionFromSolutionProject(SlnSolutionProject solutionProject)
        {
            return string.Format(GlobalSectionMask, solutionProject.ProjectId.ToString().ToUpper());
        }

        private static string GenerateProjectInfoFromSolutionProject(SlnSolutionProject solutionProject)
        {
            return string.Format(ProjectInfoMask,
                solutionProject.ProjectTypeGuid.ToString().ToUpper(),
                solutionProject.ProjectId.ToString().ToUpper(),
                solutionProject.ProjectName,
                solutionProject.RelativePath);
        }

        private const string ProjectTypeGuidRegexGroupName = "PROJECTTYPEGUID";

        private const string ProjectNameRegexGroupName = "PROJECTNAME";

        private const string ProjectRelativePathRegexGroupName = "RELATIVEPATH";

        private const string ProjectGuidRegexGroupName = "PROJECTGUID";

        // An example of a project line looks like this:
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ClassLibrary1", "ClassLibrary1\ClassLibrary1.csproj", "{05A5AD00-71B5-4612-AF2F-9EA9121C4111}"
        private static readonly Regex projectLineRegex = new Regex
        (
            $"Project\\(\"{{(?<{ProjectTypeGuidRegexGroupName}>.*)}}\"\\)"
            + "\\s*=\\s*"                                   // Any amount of whitespace plus "=" plus any amount of whitespace
            + $"\"(?<{ProjectNameRegexGroupName}>.*)\""
            + "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
            + $"\"(?<{ProjectRelativePathRegexGroupName}>.*)\""
            + "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
            + $"\"(?<{ProjectGuidRegexGroupName}>.*)\""
        );

        private const string ProjectInfoMask = @"EndProject
Project(""{{{0}}}"") = ""{2}"", ""{3}"", ""{{{1}}}""
";

        private const string GlobalSectionMask = @"    {{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{0}}}.Release Pro|Any CPU.ActiveCfg = Release|Any CPU
		{{{0}}}.Release Pro|Any CPU.Build.0 = Release|Any CPU
		{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU
    ";

        private const string ProjectElementName = "Project";
        private const string SolutionRootName = "Solution";
        private const string PathAttributeName = "Path";
        private const string IdAttributeName = "Id";
    }
}
