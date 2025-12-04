using System.Collections.Generic;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface IDotnetCliClient
    {
        void InstallProjectTemplate(string path);
        void UninstallProjectTemplate(string path);
        void CreateProjectFromTemplate(string templateName, string projectName, string directory);
        void MigrateSlnToSlnx(string projectName, string directory);
        void CreateSolution(string name, string directory);
        void AddProjectToSolution(string solutionName, string projectDirectory, string projectName);
        void AddPackageToProject(string projectPath, string packageName, string version);
        void AddSourcesToNugetConfig(string[] sources, string filePath);
        IEnumerable<string> GetPackageVersionsInNugetSources(string sitefinityPackage, string[] sources);
        IEnumerable<string> GetPackageVersionsInNugetSourcesUsingConfig(string sitefinityPackage, string nugetConfigFilePath);
        bool VersionExists(string version, string sitefinityPackage, string[] sources);
        string GetLatestVersionInNugetSources(string[] sources, string sitefinityPackage);
    }
}
