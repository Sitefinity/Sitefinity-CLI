using System;
using System.IO;

namespace Sitefinity_CLI
{
    public static class FileAttributeModifier
    {
        public static FileAttributes RemoveAttributesFromFile(string filePath, FileAttributes attributesToRemove)
        {
            return ModifyAttributes(filePath, attributesToRemove, (currentAttributes, providedAttributes) =>
            {
                return RemoveAttribute(currentAttributes, providedAttributes);
            });
        }

        public static FileAttributes AddAttributesToFile(string filePath, FileAttributes attributesToAdd)
        {
            return ModifyAttributes(filePath, attributesToAdd, (currentAttributes, providedAttributes) =>
            {
                return AddAttribute(currentAttributes, providedAttributes);
            });
        }

        private static FileAttributes ModifyAttributes(string filePath, FileAttributes attrs, Func<FileAttributes, FileAttributes, FileAttributes> modifyAction)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                FileAttributes attributesAfterModification = modifyAction(attributes, attrs);
                File.SetAttributes(filePath, attributesAfterModification);

                return attributesAfterModification ^ attributes;
            }
            catch
            {
                return 0;
            }
        }
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        private static FileAttributes AddAttribute(FileAttributes attributes, FileAttributes attributesToAdd)
        {
            return attributes | attributesToAdd;
        }
    }
}
