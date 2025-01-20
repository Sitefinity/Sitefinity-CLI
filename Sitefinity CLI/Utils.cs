using System;
using System.IO;

namespace Sitefinity_CLI
{
    internal static class Utils
    {
        public static void WriteLine(string message, ConsoleColor? foregroundColor = null)
        {
            if (foregroundColor.HasValue)
            {
                Console.ForegroundColor = foregroundColor.Value;
            }

            Console.WriteLine(message);

            if (foregroundColor.HasValue)
            {
                Console.ResetColor();
            }
        }

        public static string GetRelativePath(string destination, string origin)
        {
            origin = Path.GetDirectoryName(origin);

            if (!origin.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                origin += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(origin);
            Uri pathUri = new Uri(destination);

            string result = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
            return result;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                File.Copy(file.FullName, targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void RemoveDir(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
