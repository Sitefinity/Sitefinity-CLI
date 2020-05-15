namespace Sitefinity_CLI.VisualStudio
{
    /// <remarks>
    /// An enumeration defining the different types of projects in a solution file.
    /// </remarks>
    public enum SolutionProjectType
    {
        // Everything else besides the below well-known project types
        Unknown,

        // VB projects
        ManagedVbProject,

        // C# projects
        ManagedCsProject,

        // VJ# projects
        ManagedVjProject,

        // VC projects, managed and unmanaged
        VCProject,

        // Not really a project, but persisted as such in the .SLN file.
        SolutionFolder,

        // Venus projects
        WebProject
    }
}
