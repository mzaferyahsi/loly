using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Agent.Configuration;
using Loly.Models;
using Loly.Kafka;

namespace Loly.Agent.Analysers
{
    public class FileAnalyserHostedService : BaseAnalyserHostedService<FileAnalyser, string>
    {
        public FileAnalyserHostedService(IKafkaConfigProducer configProducer, IKafkaConsumerProvider consumerProvider,
            FileAnalyser analyser, LolyFeatureManager featureManager) : base(configProducer, consumerProvider, analyser)
        {
            if (featureManager.IsFileAnalyserEnabled())
            {
                Enabled = true;
            }
        }

        public override string ConsumerTopic => "loly-discovered";

        private void ProduceFileInfoMessage(FileInformation fileInfo)
        {
            ProduceMessage("loly-files", fileInfo);
        }

        public override async Task<bool> Consume(ConsumeResult<Ignore, string> message)
        {
            var fileInfo = _analyser.Analyse(message.Value);
            _consumer.Resume(new List<TopicPartition> {message.TopicPartition});

            if (fileInfo != null)
                ProduceFileInfoMessage(fileInfo);
            else
                _log.Warn($"Unable to analyse {message.Value}");

            return true;
        }
    }
}