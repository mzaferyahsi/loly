using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Loly.Streaming.Handlers
{
    public class GenericLogHandler<TKey, TValue>
    {
        private ILogger _logger;
        
        public GenericLogHandler(ILogger log)
        {
            _logger = log;
        }
        
        protected void Handle(IConsumer<TKey, TValue> consumer, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case SyslogLevel.Info:
                    _logger.LogInformation(logMessage.Message);
                    break;
                case SyslogLevel.Alert:
                case SyslogLevel.Warning:
                    _logger.LogWarning(logMessage.Message);
                    break;
                case SyslogLevel.Debug:
                    _logger.LogDebug(logMessage.Message);
                    break;
                case SyslogLevel.Error:
                    _logger.LogError(logMessage.Message);
                    break;
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                    _logger.LogCritical(logMessage.Message);
                    break;
                default:
                    _logger.LogInformation(logMessage.Message);
                    break;
            }
        }
    }
}