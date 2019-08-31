using Confluent.Kafka;
using log4net;

namespace Loly.Kafka.Handlers
{
    public class GenericLogHandler<TKey, TValue>
    {
        private ILog _log;
        
        public GenericLogHandler(ILog log)
        {
            _log = log;
        }
        
        protected void Handle(IConsumer<TKey, TValue> consumer, LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case SyslogLevel.Info:
                    _log.Info(logMessage.Message);
                    break;
                case SyslogLevel.Alert:
                case SyslogLevel.Warning:
                    _log.Warn(logMessage.Message);
                    break;
                case SyslogLevel.Debug:
                    _log.Debug(logMessage.Message);
                    break;
                case SyslogLevel.Error:
                    _log.Error(logMessage.Message);
                    break;
                case SyslogLevel.Critical:
                case SyslogLevel.Emergency:
                    _log.Fatal(logMessage.Message);
                    break;
                default:
                    _log.Info(logMessage.Message);
                    break;
            }
        }
    }
}