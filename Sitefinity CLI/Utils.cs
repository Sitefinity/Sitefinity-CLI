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
    }
}
