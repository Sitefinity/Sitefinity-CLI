using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI
{
    internal class PromptService : IPromptService
    {
        public bool PromptYesNo(string message, bool defaultAnswer = false)
        {
            return Prompt.GetYesNo(message, defaultAnswer);
        }
    }
}
