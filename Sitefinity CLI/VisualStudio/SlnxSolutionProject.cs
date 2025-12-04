using System;
using System.IO;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// This class represents a project (or solution folder) that is read in from an .slnx solution file.
    /// </summary>
    public class SlnxSolutionProject
    {
        /// <summary>
        /// Gets the relative path of the project from the solution file.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        public string SolutionDirectory { get; }

        /// <summary>
        /// Gets the solution file path.
        /// </summary>
        public string SolutionFilePath { get; }

        /// <summary>
        /// Gets the absolute path for this project.
        /// </summary>
        public string AbsolutePath { get; }

        /// <summary>
        /// Gets the project type enum.
        /// </summary>
        public SolutionProjectType ProjectType { get; }

        /// <summary>
        /// Gets the project file extension (e.g., .csproj, .vbproj).
        /// </summary>
        public string ProjectTypeExtension { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SlnxSolutionProject"/>.
        /// </summary>
        /// <param name="csProjFilePath">The project file path.</param>
        /// <param name="solutionFilePath">The solution file path.</param>
        /// <param name="projectType">The project type.</param>
        public SlnxSolutionProject(string csProjFilePath, string solutionFilePath, SolutionProjectType projectType)
        {
            string ext = Path.GetExtension(csProjFilePath).ToLowerInvariant();

            this.ProjectTypeExtension = ext;
            this.ProjectType = projectType;
            this.SolutionFilePath = solutionFilePath;
            this.SolutionDirectory = Path.GetDirectoryName(solutionFilePath);
            this.RelativePath = Path.GetRelativePath(this.SolutionDirectory, csProjFilePath);
            this.AbsolutePath = Path.Combine(this.SolutionDirectory, this.RelativePath);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SlnxSolutionProject"/>.
        /// </summary>
        /// <param name="relativePath">The relative path of the project from the solution file.</param>
        /// <param name="solutionFilePath">The file path to the solution.</param>
        public SlnxSolutionProject(string relativePath, string solutionFilePath)
        {
            string ext = Path.GetExtension(relativePath).ToLowerInvariant();

            this.ProjectTypeExtension = ext;
            this.ProjectType = GetProjectTypeFromExtension(ext);
            this.SolutionFilePath = solutionFilePath;
            this.SolutionDirectory = Path.GetDirectoryName(solutionFilePath);
            this.RelativePath = relativePath;
            this.AbsolutePath = Path.Combine(this.SolutionDirectory, this.RelativePath);
        }

        private SolutionProjectType GetProjectTypeFromExtension(string ext)
        {
            return ext switch
            {
                ".csproj" => SolutionProjectType.ManagedCsProject,
                ".vbproj" => SolutionProjectType.ManagedVbProject,
                ".vjproj" => SolutionProjectType.ManagedVjProject,
                ".vcxproj" => SolutionProjectType.VCProject,
                ".vcproj" => SolutionProjectType.VCProject,
                ".webproj" => SolutionProjectType.WebProject,
                _ => SolutionProjectType.Unknown
            };
        }
    }
}
