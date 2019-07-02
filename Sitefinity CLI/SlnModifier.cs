using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sitefinity_CLI
{
    internal static class SlnModifier
    {
        public static FileModifierResult AddFile(string slnFilePath, string filePath, Guid projectGuid)
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
                    Message = "Unable to read solution",
                    Success = false
                };
            }

            var projectInfo = string.Format(ProjectInfoMask, projectGuid.ToString().ToUpper());
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

            File.WriteAllText(slnFilePath, solutionContents);

            return new FileModifierResult()
            {
                Message = string.Empty,
                Success = true
            };
        }

        private static string solutionContents;

        private const string ProjectInfoMask = @"EndProject
Project(""{{{0}}}"") = ""SitefinityWebApp.Tests.Integration"", ""SitefinityWebApp.Tests.Integration\SitefinityWebApp.Tests.Integration.csproj"", ""{{{0}}}""
";

        private const string GlobalSectionMask = @"     {{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{0}}}.Release Pro|Any CPU.ActiveCfg = Release|Any CPU
		{{{0}}}.Release Pro|Any CPU.Build.0 = Release|Any CPU
		{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU
    ";
    }
}
