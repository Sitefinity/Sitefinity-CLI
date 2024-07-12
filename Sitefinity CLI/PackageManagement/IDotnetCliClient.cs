namespace Sitefinity_CLI.PackageManagement
{
    internal interface IDotnetCliClient
    {
        void InstallProjectTemplate(string path);
        void UninstallProjectTemplate(string path);
        void CreateProjectFromTemplate(string templateName, string projectName, string directory);
        void AddSourcesToNugetConfig(string[] sources, string filePath);
        bool VersionExists(string version, string sitefinityPackage, string[] sources);
    }
}
