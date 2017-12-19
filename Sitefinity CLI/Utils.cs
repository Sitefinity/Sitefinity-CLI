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
    }
}
