using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// Represents a project that is read in from a solution file.
    /// </summary>
    public interface ISolutionProject
    {
        /// <summary>
        /// Gets the project id.
        /// </summary>
        public Guid ProjectId { get; }

        /// <summary>
        /// Gets the relative path of the project from the solution file.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        string SolutionDirectory { get; }

        /// <summary>
        /// Gets the solution file path.
        /// </summary>
        string SolutionFilePath { get; }

        /// <summary>
        /// Gets the absolute path for this project.
        /// </summary>
        string AbsolutePath { get; }

        /// <summary>
        /// Gets the project type enum.
        /// </summary>
        SolutionProjectType ProjectType { get; }
    }
}
