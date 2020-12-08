using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SitefinityCLI.Tests.UpgradeCommandTests.Mocks
{
    class TestLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
