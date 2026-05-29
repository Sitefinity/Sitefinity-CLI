using System.Collections.Generic;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// Strategy for reading/writing projects in a specific solution file format.
    /// </summary>
    internal interface ISolutionFileStrategy
    {
        /// <summary>
        /// Returns the projects from a solution file.
        /// </summary>
        IEnumerable<ISolutionProject> GetProjects(string solutionFilePath);

        /// <summary>
        /// Adds a project reference to the solution file.
        /// </summary>
        void AddProject(string solutionFilePath, string projectAbsoluteFilePath, SolutionProjectType projectType);
    }
}
