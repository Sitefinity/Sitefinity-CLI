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
            FileAttributes attributes = File.GetAttributes(filePath);
            attributes = RemoveAttribute(attributes, attributesToRemove);
            File.SetAttributes(filePath, attributes);
        }

        public static void SetFileAttributes(string filePath, FileAttributes attributes)
        {
            File.SetAttributes(filePath, attributes);
        }
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}
