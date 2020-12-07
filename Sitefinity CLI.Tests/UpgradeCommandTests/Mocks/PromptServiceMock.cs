using System;
using System.Collections.Generic;
using System.Text;
using Sitefinity_CLI;

namespace SitefinityCLI.Tests.UpgradeCommandTests.Mocks
{
    internal class PromptServiceMock : IPromptService
    {
        public bool PromptYesNo(string message, bool defaultAnswer = false)
        {
            return true;
        }
    }
}
