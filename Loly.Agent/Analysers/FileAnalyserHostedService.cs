using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;
using Loly.Agent.Kafka;
using Loly.Agent.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Error = Confluent.Kafka.Error;

namespace Loly.Agent.Analysers
{
    public class FileAnalyserHostedService : IHostedService
    {
        private readonly IKafkaConsumerProvider _consumerProvider;
        private readonly IKafkaProducerHostedService _kafkaProducerHostedService;
        private readonly ILog _log = LogManager.GetLogger(typeof(FileAnalyserHostedService));
        private readonly FileAnalyser _analyser;
        private CancellationTokenSource _cancellationTokenSource;
        private IConsumer<Ignore, string> _consumer;
        private Task _consumeTask;

        public FileAnalyserHostedService(IKafkaProducerHostedService kafkaProducerHostedService,
            IKafkaConsumerProvider consumerProvider, FileAnalyser analyser)
        {
            _consumerProvider = consumerProvider;
            _analyser = analyser;
            _kafkaProducerHostedService = kafkaProducerHostedService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitializeConsumer();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DeInitializeConsumer();
            return Task.CompletedTask;
        }

        protected void ErrorHandler(IConsumer<Ignore, string> consumer, Error error)
        {
            if (error.IsFatal)
            {
                _log.Fatal(error.Reason);
                StopAsync(_cancellationTokenSource.Token);
            }
            else if (error.IsError)
            {
                _log.Error(error.Reason);
                StopAsync(_cancellationTokenSource.Token);
            }
            
            throw new KafkaException(error.Code);
        }

        protected void LogHandler(IConsumer<Ignore, string> consumer, LogMessage logMessage)
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

        private void InitializeConsumer()
        {
            _log.Info("Initializing kafka consumer for file analyser.");
            if (_consumer != null)
            {
                _log.Warn("There's already an initialized kafka consumer for file analyser.");
                DeInitializeConsumer();
            }

            _consumer = _consumerProvider.GetConsumer<Ignore, string>(LogHandler, ErrorHandler);
            _consumer.Subscribe("loly-discovered");
            _cancellationTokenSource = new CancellationTokenSource();
            _consumeTask = Task.Run(async () => 
            {
                if (_consumer == null)
                    throw new NullReferenceException("Consumer is not initialized.");
    
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var cr = _consumer.Consume(CancellationToken.None);
                    _log.Debug($"Received {cr.Value} for analyse.");
                    var fileInfo = await _analyser.Analyse(cr.Value);
                    _log.Debug($"Analyse result as {JsonConvert.SerializeObject(fileInfo)}");
    
                    if (fileInfo != null)
                        ProduceMessage(fileInfo);
                    else
                        _log.Warn($"Unable to analyse {cr.Value}");
                }
            }, _cancellationTokenSource.Token);
            
            _log.Info("Initialized kafka consumer for file analyser.");
        }

        private void DeInitializeConsumer()
        {
            _log.Info("De-initializing kafka consumer for file analyser.");
            if (_consumer != null)
            {
                _consumer.Close();
                _consumer = null;
            }

            if (_consumeTask != null)
            {
                _cancellationTokenSource.Cancel();
                _consumeTask = null;
                _cancellationTokenSource = null;
            }

            _log.Info("De-initialized kafka consumer for file analyser.");
        }

        private void ProduceMessage(FileInformation fileInfo)
        {
            var serializedMessage = JToken.Parse(JsonConvert.SerializeObject(fileInfo)).ToString();
            var message = new KafkaMessage
            {
                Topic = "loly-files",
                Message = serializedMessage
            };
            _kafkaProducerHostedService.AddMessage(message);
            _kafkaProducerHostedService.StartAsync(CancellationToken.None);
        }
    }
}