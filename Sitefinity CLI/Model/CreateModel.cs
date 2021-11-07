using System;
using System.Collections.Generic;
using System.Text;

namespace Sitefinity_CLI.Model
{
    public class CreateModel
    {
        public CreateModel()
        {
            PackageSources = new List<string>();
        }

        public string TargetFolder { get; set; }
        public string Version { get; set; }
        public string SqlServer { get; set; }
        public string SolutionName { get; set; }
        public string ProjectName { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseRestoreUserName { get; set; }
        public string DatabaseRestorePassword { get; set; }
        public string ProjectTemplatePath { get; set; }
        public bool SkipDbRestore { get; set; }
        public bool SkipConfirmation { get; set; }
        public string LicenseFilePath { get; set; }
        public List<string> PackageSources { get; set; }
        public string SfUser { get; set; }
        public string SfPassword { get; set; }

    }
}
