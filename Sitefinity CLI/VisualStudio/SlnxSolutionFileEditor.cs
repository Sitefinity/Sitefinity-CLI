using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sitefinity_CLI.VisualStudio
{
    public static class SlnxSolutionFileEditor
    {
        /// <summary>
        /// Returns the projects from a solution file.
        /// </summary>
        /// <param name="solutionFilePath">The path to the solution file.</param>
        /// <returns>Collection of <see cref="SlnxSolutionProject"/>.</returns>
        public static IEnumerable<SlnxSolutionProject> GetProjects(string solutionFilePath)
        {
            var solutionProjects = new List<SlnxSolutionProject>();
            XDocument doc = XDocument.Load(solutionFilePath);

            foreach (var projectElement in doc.Descendants("Project"))
            {
                var pathAttr = projectElement.Attribute("Path");
                if (pathAttr != null && !string.IsNullOrWhiteSpace(pathAttr.Value))
                {
                    string normalizedPath = pathAttr.Value.Replace('/', Path.DirectorySeparatorChar);
                    SlnxSolutionProject project = new SlnxSolutionProject(normalizedPath, solutionFilePath);
                    solutionProjects.Add(project);
                }
            }

            return solutionProjects;
        }

        /// <summary>
        /// Adds reference to a csproj file in a solution. 
        /// </summary>
        /// <param name="solutionFilePath">The solution file path</param>
        /// <param name="solutionProject">The project to add</param>
        public static void AddProject(string solutionFilePath, SlnxSolutionProject solutionProject)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"Unable to find {solutionFilePath}");
            }

            try
            {
                XDocument doc = XDocument.Load(solutionFilePath);
                string normalizedPath = solutionProject.RelativePath.Replace(Path.DirectorySeparatorChar, '/');

                var projectExisting = doc.Descendants("Project").Any(p => p.Attribute("Path")?.Value == normalizedPath);

                if (!projectExisting)
                {
                    // Find the root element (should be <Solution>)
                    var root = doc.Root;
                    if (root == null)
                    {
                        throw new Exception(Constants.SolutionNotReadable);
                    }

                    var newProject = new XElement("Project", new XAttribute("Path", normalizedPath));

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
    }
}
