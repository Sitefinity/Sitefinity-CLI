//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Extensions.Logging;

//namespace SitefinityCLI.Tests.UpgradeCommandTests.Mocks
//{
//    public class MockedLogger : ILogger
//    {
//        public IDisposable BeginScope<TState>(TState state)
//        {
//        }


//        public bool IsEnabled(LogLevel logLevel)
//        {
//            return true;
//        }

//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            Console.WriteLine(state);
//        }
//    }
//}
