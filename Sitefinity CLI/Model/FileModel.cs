using System;
using System.Collections.Generic;
using System.Text;

namespace Sitefinity_CLI.Model
{
    /// <summary>
    /// Represents a file model
    /// </summary>
    internal class FileModel
    {
        /// <summary>
        /// The target absolute file path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The template absolute file path
        /// </summary>
        public string TemplatePath { get; set; }
    }
}
