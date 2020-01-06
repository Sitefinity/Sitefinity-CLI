using System.IO;

namespace Sitefinity_CLI
{
    public static class FileAttributeEditor
    {
        public static FileAttributes GetFileAttributes(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return 0;
            }

            return File.GetAttributes(filePath);
        }

        public static void RemoveAttributesFromFile(string filePath, FileAttributes attributesToRemove)
        {
            FileAttributes attributes = GetFileAttributes(filePath);

            attributes = attributes & ~attributesToRemove;

            SetFileAttributes(filePath, attributes);
        }

        public static void SetFileAttributes(string filePath, FileAttributes attributes)
        {
            File.SetAttributes(filePath, attributes);
        }
    }
}
