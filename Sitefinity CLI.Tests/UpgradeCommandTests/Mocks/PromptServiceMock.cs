using Sitefinity_CLI;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks
{
    internal class PromptServiceMock : IPromptService
    {
        public bool PromptYesNo(string message, bool defaultAnswer = false)
        {
            return Answer;
        }

        public bool Answer { get; set; }
    }
}
