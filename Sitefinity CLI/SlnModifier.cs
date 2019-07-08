using System;
using System.IO;

namespace Sitefinity_CLI
{
    public static class SlnModifier
    {
        public static FileModifierResult AddFile(string slnFilePath, string csProjFilePath, Guid projectGuid, string webAppName)
        {
            if (!File.Exists(slnFilePath))
            {
                return new FileModifierResult()
                {
                    Message = $"Unable to find {slnFilePath}",
                    Success = false
                };
            }

            solutionContents = File.ReadAllText(slnFilePath);

            var endProjectIndex = solutionContents.LastIndexOf("EndProject");
            if (endProjectIndex < 0)
            {
                return new FileModifierResult()
                {
                    Message = Constants.SolutionNotReadable,
                    Success = false
                };
            }

            var fileName = Path.GetFileName(csProjFilePath);

            if (string.IsNullOrEmpty(webAppName))
            {
                return new FileModifierResult()
                {
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var fileIndex = solutionContents.IndexOf(webAppName);

            if (fileIndex < 0)
            {
                return new FileModifierResult()
                {
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var projectTypeGuidBeginIndex = solutionContents.LastIndexOf("(\"{", fileIndex) + 3;

            if (projectTypeGuidBeginIndex < 0)
            {
                return new FileModifierResult()
                {
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var projectTypeGuidEndIndex = solutionContents.IndexOf("}\")", projectTypeGuidBeginIndex);

            if (projectTypeGuidEndIndex < 0)
            {
                return new FileModifierResult()
                {
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var length = projectTypeGuidEndIndex - projectTypeGuidBeginIndex;

            var projectTypeGuid = solutionContents.Substring(projectTypeGuidBeginIndex, length);

            var projectInfo = string.Format(ProjectInfoMask, projectTypeGuid, projectGuid.ToString().ToUpper(), Path.GetFileName(csProjFilePath), Path.GetRelativePath(Path.GetDirectoryName(slnFilePath), csProjFilePath));
            solutionContents = solutionContents.Insert(endProjectIndex, projectInfo);

            var beginGlobalSectionIndex = solutionContents.IndexOf("GlobalSection(ProjectConfigurationPlatforms)");

            var endGlobalSectionIndex = solutionContents.IndexOf("EndGlobalSection", beginGlobalSectionIndex);

            if (endGlobalSectionIndex < 0)
            {
                return new FileModifierResult()
                {
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var globalSection = string.Format(GlobalSectionMask, projectGuid.ToString().ToUpper());

            solutionContents = solutionContents.Insert(endGlobalSectionIndex, globalSection);

            try
            {
                File.WriteAllText(slnFilePath, solutionContents);
            }
            catch
            {
                return new FileModifierResult()
                {
                    Message = Constants.AddFilesInsufficientPrivilegesMessage,
                    Success = false
                };
            }
            return new FileModifierResult()
            {
                Message = string.Empty,
                Success = true
            };
        }

        private static string solutionContents;

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
