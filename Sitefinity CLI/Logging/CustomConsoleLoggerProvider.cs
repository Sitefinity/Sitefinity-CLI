using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Console.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Sitefinity_CLI.Logging
{
    [ProviderAlias("Console")]
    public class CustomConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, CustomConsoleLogger> loggers = new ConcurrentDictionary<string, CustomConsoleLogger>();

        private readonly Func<string, LogLevel, bool> filter;
        private readonly ConsoleLoggerProcessor messageQueue = new ConsoleLoggerProcessor();

        private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;
        private static readonly Func<string, LogLevel, bool> falseFilter = (cat, level) => false;
        private IDisposable optionsReloadToken;
        private bool includeScopes;
        private bool disableColors;
        private IExternalScopeProvider scopeProvider;

        public CustomConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerOptions> options)
        {
            // Filter would be applied on LoggerFactory level
            this.filter = trueFilter;
            this.optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);
        }

        private void ReloadLoggerOptions(ConsoleLoggerOptions options)
        {
            this.includeScopes = options.IncludeScopes;
            this.disableColors = options.DisableColors;
            var scopeProvider = GetScopeProvider();
            foreach (var logger in this.loggers.Values)
            {
                logger.ScopeProvider = scopeProvider;
                logger.DisableColors = options.DisableColors;
            }
        }

        public ILogger CreateLogger(string name)
        {
            return this.loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private CustomConsoleLogger CreateLoggerImplementation(string name)
        {
            var disableColors = this.disableColors;

            return new CustomConsoleLogger(name, GetFilter(), this.includeScopes ? this.scopeProvider : null, this.messageQueue)
            {
                DisableColors = disableColors
            };
        }

        private Func<string, LogLevel, bool> GetFilter()
        {
            if (this.filter != null)
            {
                return this.filter;
            }

            return falseFilter;
        }

        private IExternalScopeProvider GetScopeProvider()
        {
            if (this.includeScopes && this.scopeProvider == null)
            {
                this.scopeProvider = new LoggerExternalScopeProvider();
            }

            return this.includeScopes ? this.scopeProvider : null;
        }

        public void Dispose()
        {
            this.optionsReloadToken?.Dispose();
            this.messageQueue.Dispose();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }
    }
}
