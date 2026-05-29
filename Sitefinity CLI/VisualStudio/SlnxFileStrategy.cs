using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// Strategy for reading/writing projects in the .slnx format.
    /// </summary>
    internal class SlnxFileStrategy : ISolutionFileStrategy
    {
        public IEnumerable<ISolutionProject> GetProjects(string solutionFilePath)
        {
            var solutionProjects = new List<SlnxSolutionProject>();
            XDocument doc = XDocument.Load(solutionFilePath);

            foreach (var projectElement in doc.Descendants(ProjectElementName))
            {
                var pathAttr = projectElement.Attribute(PathAttributeName);

                if (pathAttr != null)
                {
                    string normalizedPath = pathAttr.Value.Replace('/', Path.DirectorySeparatorChar);
                    SlnxSolutionProject project = new SlnxSolutionProject(normalizedPath, solutionFilePath);
                    solutionProjects.Add(project);
                }
            }

            return solutionProjects;
        }

        public void AddProject(string solutionFilePath, string projectAbsoluteFilePath, SolutionProjectType projectType)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"Unable to find {solutionFilePath}");
            }

            try
            {
                SlnxSolutionProject solutionProject = new SlnxSolutionProject(projectAbsoluteFilePath, solutionFilePath, projectType);
                XDocument doc = XDocument.Load(solutionFilePath);
                string normalizedPath = solutionProject.RelativePath.Replace(Path.DirectorySeparatorChar, '/');

                var projectExisting = doc.Descendants(ProjectElementName).Any(p => p.Attribute(PathAttributeName)?.Value == normalizedPath);

                if (!projectExisting)
                {
                    var root = doc.Root;
                    if (root == null || root.Name.LocalName != SolutionRootName)
                    {
                        throw new Exception(Constants.SolutionNotReadable);
                    }

                    var newProject = new XElement(ProjectElementName, new XAttribute(PathAttributeName, normalizedPath));

                    root.Add(newProject);
                    doc.Save(solutionFilePath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(Constants.AddFilesInsufficientPrivilegesMessage);
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException)
            {
                throw new Exception(Constants.AddFilesToSolutionFailureMessage);
            }
        }

        private const string ProjectElementName = "Project";
        private const string SolutionRootName = "Solution";
        private const string PathAttributeName = "Path";
    }
}
