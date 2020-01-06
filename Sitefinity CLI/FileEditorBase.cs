using System;
using System.IO;

namespace Sitefinity_CLI
{
    public abstract class FileEditorBase
    {
        protected void EnsureFileOperation(string xmlFilePath, bool isWriteAction, Action fileAction)
        {
            FileAttributes initialFileAttributes = FileAttributeEditor.GetFileAttributes(xmlFilePath);

            try
            {
                FileAttributes attributesToRemove = FileAttributes.Hidden;
                if (isWriteAction)
                {
                    attributesToRemove = attributesToRemove | FileAttributes.ReadOnly;
                }

                FileAttributeEditor.RemoveAttributesFromFile(xmlFilePath, attributesToRemove);

                fileAction();
            }
            finally
            {
                try
                {
                    FileAttributeEditor.SetFileAttributes(xmlFilePath, initialFileAttributes);
                }
                catch { }
            }
        }
    }
}
