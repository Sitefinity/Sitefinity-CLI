using System;

namespace Sitefinity_CLI.Exceptions
{
    internal class InvalidVersionException : ApplicationException
    {
        public InvalidVersionException(string message) : base(message)
        {
        }
    }
}
