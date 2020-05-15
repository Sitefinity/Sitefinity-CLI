using System.IO;

namespace Sitefinity_CLI.VisualStudio
{
    internal class ProjectConfigFileEditor : IProjectConfigFileEditor
    {
        public string GetProjectConfigPath(string projectLocation)
        {
            if (string.IsNullOrEmpty(projectLocation))
                return null;

            var projectDirectory = Path.GetDirectoryName(projectLocation);

            var webConfigPath = Path.Combine(projectDirectory, WebConfigName);
            if (File.Exists(webConfigPath))
                return webConfigPath;

            var appConfigPath = Path.Combine(projectDirectory, AppConfigName);
            if (File.Exists(appConfigPath))
                return appConfigPath;

            return null;
        }

        private const string WebConfigName = "web.config";
        private const string AppConfigName = "app.config";
    }
}
