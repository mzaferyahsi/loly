using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Loly.Kafka.Handlers
{
    public class GenericLogHandler<TKey, TValue>
    {
        private ILogger _log;
        
        public GenericLogHandler(ILogger log)
        {
            _log = log;
        }
        
        protected void Handle(IConsumer<TKey, TValue> consumer, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case SyslogLevel.Info:
                    _log.LogInformation(logMessage.Message);
                    break;
                case SyslogLevel.Alert:
                case SyslogLevel.Warning:
                    _log.LogWarning(logMessage.Message);
                    break;
                case SyslogLevel.Debug:
                    _log.LogDebug(logMessage.Message);
                    break;
                case SyslogLevel.Error:
                    _log.LogError(logMessage.Message);
                    break;
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                    _log.LogCritical(logMessage.Message);
                    break;
                default:
                    _log.LogInformation(logMessage.Message);
                    break;
            }
        }
    }
}