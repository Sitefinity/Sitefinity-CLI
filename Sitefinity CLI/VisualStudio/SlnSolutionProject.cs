using System;
using System.IO;

namespace Sitefinity_CLI.VisualStudio
{
    /// <summary>
    /// This class represents a project (or solution folder) that is read in from a solution file.
    /// </summary>
    public class SlnSolutionProject
    {
        /// <summary>
        /// Gets the project guid.
        /// </summary>
        public Guid ProjectGuid { get; }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string ProjectName { get; }

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
        /// Gets the project type guid string.
        /// </summary>
        public Guid ProjectTypeGuid { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SolutionProject"/>.
        /// </summary>
        /// <param name="projectGuid">The guid of the project.</param>
        /// <param name="csProjFilePath">The project file path.</param>
        /// <param name="solutionFilePath">The solution file path.</param>
        /// <param name="projectType">The project type.</param>
        public SlnSolutionProject(Guid projectGuid, string csProjFilePath, string solutionFilePath, SolutionProjectType projectType)
        {
            this.ProjectGuid = projectGuid;
            this.ProjectName = Path.GetFileName(csProjFilePath);
            this.ProjectType = projectType;
            this.SolutionFilePath = solutionFilePath;
            this.SolutionDirectory = Path.GetDirectoryName(solutionFilePath);
            this.RelativePath = Path.GetRelativePath(this.SolutionDirectory, csProjFilePath);
            this.AbsolutePath = Path.Combine(this.SolutionDirectory, this.RelativePath);

            Guid? projectTypeGuid = GetProjectTypeGuidFromProjectTypeEnum(projectType);
            if (!projectTypeGuid.HasValue)
            {
                throw new Exception(string.Format("Cannot find project type guid for project type {0}", projectType));
            }
            this.ProjectTypeGuid = projectTypeGuid.Value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SolutionProject"/>.
        /// </summary>
        /// <param name="projectGuid">The guid of the project.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="relativePath">The relative path of the project from the solution file.</param>
        /// <param name="projectTypeGuid">The project type guid.</param>
        /// <param name="solutionFilePath">The file path to the solution.</param>
        public SlnSolutionProject(Guid projectGuid, string projectName, string relativePath, Guid projectTypeGuid, string solutionFilePath)
        {
            this.ProjectGuid = projectGuid;
            this.ProjectName = projectName;
            this.ProjectType = GetProjectTypeEnumFromProjectTypeGuid(projectTypeGuid);
            this.ProjectTypeGuid = projectTypeGuid;
            this.SolutionFilePath = solutionFilePath;
            this.SolutionDirectory = Path.GetDirectoryName(solutionFilePath);
            this.RelativePath = relativePath;
            this.AbsolutePath = Path.Combine(this.SolutionDirectory, this.RelativePath);
        }

        private SolutionProjectType GetProjectTypeEnumFromProjectTypeGuid(Guid projectTypeGuid)
        {
            if (projectTypeGuid == vbProjectGuid)
            {
                return SolutionProjectType.ManagedVbProject;
            }
            else if (projectTypeGuid == csProjectGuid)
            {
                return SolutionProjectType.ManagedCsProject;
            }
            else if (projectTypeGuid == vjProjectGuid)
            {
                return SolutionProjectType.ManagedVjProject;
            }
            else if (projectTypeGuid == vcProjectGuid)
            {
                return SolutionProjectType.VCProject;
            }
            else if (projectTypeGuid == webProjectGuid)
            {
                return SolutionProjectType.WebProject;
            }
            else if (projectTypeGuid == solutionFolderGuid)
            {
                return SolutionProjectType.SolutionFolder;
            }

            return SolutionProjectType.Unknown;
        }

        private Guid? GetProjectTypeGuidFromProjectTypeEnum(SolutionProjectType solutionProjectType)
        {
            return solutionProjectType switch
            {
                SolutionProjectType.ManagedCsProject => csProjectGuid,
                SolutionProjectType.ManagedVbProject => vbProjectGuid,
                SolutionProjectType.ManagedVjProject => vjProjectGuid,
                SolutionProjectType.SolutionFolder => solutionFolderGuid,
                SolutionProjectType.VCProject => vcProjectGuid,
                SolutionProjectType.WebProject => webProjectGuid,
                _ => null
            };
        }

        private static Guid vbProjectGuid = new Guid("F184B08F-C81C-45F6-A57F-5ABD9991F28F");
        private static Guid csProjectGuid = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
        private static Guid vjProjectGuid = new Guid("E6FDF86B-F3D1-11D4-8576-0002A516ECE8");
        private static Guid vcProjectGuid = new Guid("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
        private static Guid webProjectGuid = new Guid("E24C65DC-7377-472B-9ABA-BC803B73C61A");
        private static Guid solutionFolderGuid = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
    }
}
