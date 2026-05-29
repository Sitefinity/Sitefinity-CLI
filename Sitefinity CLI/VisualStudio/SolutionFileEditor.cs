using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// A class used to manage the contents of a solution file.
    /// Delegates format-specific work to <see cref="ISolutionFileStrategy"/> implementations.
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
            ISolutionFileStrategy strategy = GetStrategy(solutionFilePath);
            return strategy.GetProjects(solutionFilePath);
        }

        /// <summary>
        /// Returns the projects from a solution file as their concrete type.
        /// </summary>
        /// <typeparam name="T">The concrete solution project type (SlnSolutionProject or SlnxSolutionProject).</typeparam>
        /// <param name="solutionFilePath">The path to the solution file.</param>
        /// <returns>Collection of the concrete project type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested type doesn't match the solution file format.</exception>
        public static IEnumerable<T> GetProjects<T>(string solutionFilePath) where T : ISolutionProject
        {
            ISolutionFileStrategy strategy = GetStrategy(solutionFilePath);
            return strategy.GetProjects(solutionFilePath).Cast<T>();
        }

        /// <summary>
        /// Adds reference to a project file in a solution. 
        /// </summary>
        /// <param name="solutionFilePath">The solution file path</param>
        /// <param name="projectAbsoluteFilePath">The project absolute file path to add</param>
        /// <param name="projectType">The project type</param>
        public static void AddProject(string solutionFilePath, string projectAbsoluteFilePath, SolutionProjectType projectType)
        {
            ISolutionFileStrategy strategy = GetStrategy(solutionFilePath);
            strategy.AddProject(solutionFilePath, projectAbsoluteFilePath, projectType);
        }

        internal static ISolutionFileStrategy GetStrategy(string solutionFilePath)
        {
            string extension = Path.GetExtension(solutionFilePath);

            if (extension.Equals(Constants.SlnFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return new SlnFileStrategy();
            }
            else if (extension.Equals(Constants.SlnxFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return new SlnxFileStrategy();
            }

            throw new InvalidOperationException($"Unsupported solution file format: {extension}");
        }
    }
}
