using System.Collections.Generic;

namespace Sitefinity_CLI.VisualStudio
{
    public interface ICsProjectFileEditor
    {
        void RemovePropertyGroupElement(string csProjFilePath, string elementName);

        void RemoveReference(string csProjFilePath, string assemblyFilePath);

        IEnumerable<CsProjectFileReference> GetReferences(string csProjFilePath);

        void AddFiles(string csProjFilePath, IEnumerable<string> filePaths);

        void RemoveFiles(string csProjFilePath, IEnumerable<string> filePaths);
    }
}
