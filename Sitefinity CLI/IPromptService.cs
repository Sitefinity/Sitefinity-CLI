using System;
using System.Collections.Generic;
using System.Text;

namespace Sitefinity_CLI
{
    internal interface IPromptService
    {
        bool PromptYesNo(string message, bool defaultAnswer = false);
    }
}
