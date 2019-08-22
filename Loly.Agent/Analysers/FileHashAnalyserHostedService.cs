using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Agent.Configuration;
using Loly.Models;
using Loly.Agent.Utility;
using Loly.Analysers;
using Loly.Kafka;
using Loly.Models.Messages;

namespace Loly.Agent.Analysers
{
    public class FileHashAnalyserHostedService : BaseAnalyserHostedService<FileHashAnalyser, FileInformation>
    {
        public FileHashAnalyserHostedService(IKafkaConfigProducer configProducer,
            IKafkaConsumerProvider consumerProvider, FileHashAnalyser analyser, LolyFeatureManager featureManager) : base(configProducer, consumerProvider, analyser)
        {
            if (featureManager.IsFileHashAnalyserEnabled())
            {
                Enabled = true;
            }
        }

        public override List<string> ConsumerTopic => new List<string>() {"loly-files"};

        private void ProduceFileHashMessage(FileInformation fileInformation, string hash)
        {
            var fileMetaDataMessage = new FileMetaDataMessage
            {
                Path = fileInformation.Path,
                FileId = fileInformation.Id
            };
            fileMetaDataMessage.MetaData.Add(FileMetadataTypes.Hash.ToString(), hash);

            ProduceMessage("loly-file-metadata", fileMetaDataMessage);
        }

        public override async Task<bool> Consume(ConsumeResult<Ignore, FileInformation> message)
        {
            var hash = await _analyser.Analyse(HashMethods.Md5, message.Value.Path);

            if (!string.IsNullOrEmpty(hash))
                ProduceFileHashMessage(message.Value, hash);
            else
                _log.Warn($"Unable to analyse {message.Value}");

            return true;
        }
    }
}