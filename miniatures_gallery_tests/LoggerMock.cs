using NLog.Config;
using NLog.Targets;
using NLog;
using Microsoft.Extensions.Logging;
using MiniaturesGallery.Services;
using NLog.Extensions.Logging;

namespace MiniaturesGallery.Tests
{
    public class LoggerMock<T>
    {
        private readonly MemoryTarget _memoryTarget = new MemoryTarget { Name = "mem" };

        public IList<string> GetLogs => _memoryTarget.Logs;

        public ILogger<T> GetServiceLogger()
        {
            //init with empty configuration. Add one target and one rule
            var configuration = new LoggingConfiguration();
            configuration.AddTarget(_memoryTarget);
            configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, _memoryTarget));
            LogManager.Configuration = configuration;

            var logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<T>();
            return logger;
        }
    }
}
