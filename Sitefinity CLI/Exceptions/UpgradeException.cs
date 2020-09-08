using System;

namespace Sitefinity_CLI.Exceptions
{
    public class UpgradeException : ApplicationException
    {
        public UpgradeException(string message) : base(message)
        {
        }
    }
}
