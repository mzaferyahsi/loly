using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Loly.Streaming.Consumer
{
    public class ConsumerService<TKey, TValue> : IDisposable
    {
        private ILogger _logger;
        private IConsumerProvider _consumerProvider;
        private Task _consumerTask;
        private IConsumer<TKey, TValue> _consumer;
        private List<string> _topicList;
        private CancellationTokenSource _cancellationTokenSource;
        private ConsumerConfig _consumerConfig;

        public delegate void ConsumerErrorEventHandler(object sender, ErrorEventHandlerArgs<TKey, TValue> args);
        public delegate void ConsumerLogEventHandler(object sender, LogEventHandlerArgs<TKey, TValue> args);
        public delegate void ConsumerConsumeResultEventHandler(object sender,
            ConsumeResultHandlerArgs<TKey, TValue> args);
        
        public event ConsumerErrorEventHandler ConsumerError;
        public event ConsumerLogEventHandler ConsumerLog;
        public event ConsumerConsumeResultEventHandler ConsumeResult;

        public ConsumerService(IConsumerProvider consumerProvider, List<string> topicList, ILogger logger) 
            : this(consumerProvider, null, topicList,logger)
        {
            
        }

        public ConsumerService(IConsumerProvider consumerProvider, ConsumerConfig consumerConfig,
            List<string> topicList,
            ILogger logger)
        {
            _consumerConfig = consumerConfig;
            _consumerProvider = consumerProvider;
            _topicList = topicList;
            _logger = logger;
        }

        /// <summary>
        /// Handles errors received from Kafka Consumer. If the ConsumerErrorEventHandler is empty, the error will be
        /// passed to the handler, if not, then the error will not be handled.
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="error"></param>
        public virtual void ErrorHandler(IConsumer<TKey, TValue> consumer, Error error)
        {
            ConsumerError?.Invoke(this,
                new ErrorEventHandlerArgs<TKey, TValue>() {Error = error, Consumer = consumer});
        }
        
        /// <summary>
        /// Handles log messages received from Kafka Consumer. If the ConsumerLogEventHandler is set, the message will
        /// be passed to the handler without managing, else it will use logger to log message depending on the level.
        /// </summary>
        /// <param name="consumer">Kafka consumer</param>
        /// <param name="logMessage">Kafka log message</param>
        public void LogHandler(IConsumer<TKey, TValue> consumer, LogMessage logMessage)
        {
            if (ConsumerLog != null)
            {
                ConsumerLog(this, new LogEventHandlerArgs<TKey, TValue>() {Consumer = consumer, Message = logMessage});
            }
            else
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
        /// <summary>
        /// Stars consuming messages.
        /// </summary>
        /// <exception cref="ConsumerException<TKey, TValue>">Thrown when OnConsumeResult not initialized.</exception>
        public void Consume()
        {
            if (ConsumeResult == null)
                throw new ConsumerException<TKey, TValue>(_consumer, "Consume result event handler not initialized");
            
            if(_consumerTask != null)
                return;
            
//            if (_consumer != null)
//                return;
//            
            _cancellationTokenSource = new CancellationTokenSource();

            _consumerTask = new Task(() =>
            {
                if (_consumer == null)
                {
                    _consumer = _consumerConfig == null ? 
                        _consumerProvider.Get<TKey, TValue>(LogHandler, ErrorHandler) : _consumerProvider.Get<TKey, TValue>(_consumerConfig, LogHandler, ErrorHandler);

                    _consumer.Subscribe(_topicList);
                }
                
                while (true)
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(3));
                    if (consumeResult == null) continue;

                    ConsumeResult(this, new ConsumeResultHandlerArgs<TKey, TValue>() {Consumer = _consumer, ConsumeResult = consumeResult});
                }
            }, _cancellationTokenSource.Token);
            _consumerTask.Start();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            while (_consumerTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(10);
            }
            _consumerTask?.Dispose();
            _consumer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        public void Stop()
        {
            if (_consumer != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}