using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// Strategy for reading/writing projects in the classic .sln format.
    /// </summary>
    internal class SlnFileStrategy : ISolutionFileStrategy
    {
        public IEnumerable<ISolutionProject> GetProjects(string solutionFilePath)
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

        public void AddProject(string solutionFilePath, string projectAbsoluteFilePath, SolutionProjectType projectType)
        {
            Guid projectId = Guid.NewGuid();
            SlnSolutionProject solutionProject = new SlnSolutionProject(projectId, projectAbsoluteFilePath, solutionFilePath, projectType);
            AddProjectToSln(solutionFilePath, solutionProject);
        }

        private static void AddProjectToSln(string solutionFilePath, SlnSolutionProject solutionProject)
        {
            string solutionFileContent = GetSolutionFileContentAsString(solutionFilePath);

            solutionFileContent = AddProjectInfoInSolutionFileContent(solutionFileContent, solutionProject);
            solutionFileContent = AddProjectGlobalSectionInSolutionFileContent(solutionFileContent, solutionProject);

            SaveSolutionFileContent(solutionFilePath, solutionFileContent);
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

        private static readonly Regex projectLineRegex = new Regex
        (
            $"Project\\(\"{{(?<{ProjectTypeGuidRegexGroupName}>.*)}}\"\\)"
            + "\\s*=\\s*"
            + $"\"(?<{ProjectNameRegexGroupName}>.*)\""
            + "\\s*,\\s*"
            + $"\"(?<{ProjectRelativePathRegexGroupName}>.*)\""
            + "\\s*,\\s*"
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
    }
}
