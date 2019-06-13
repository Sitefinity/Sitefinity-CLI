using System;
using System.IO;

namespace Sitefinity_CLI
{
    public static class FileAttributeModifier
    {
        public static FileAttributes GetFileAttributes(string filePath)
        {
            return File.GetAttributes(filePath);
        }
        public static void RemoveAttributesFromFile(string filePath, FileAttributes attributesToRemove)
        {
            FileAttributes attributes = GetFileAttributes(filePath);
            attributes = RemoveAttribute(attributes, attributesToRemove);
            SetFileAttributes(filePath, attributes);
        }

        public static void SetFileAttributes(string filePath, FileAttributes attributes)
        {
            try
            {
                File.SetAttributes(filePath, attributes);
            }
            catch { }
        }
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}
