using Loly.Streaming.Config;
using Loly.Streaming.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace Loly.Streaming.Tests
{
    public class KafkaConfigProviderTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var configOptions = Options.Create(new KafkaSettings
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new ConfigProvider(configOptions);
            Assert.NotNull(configProvider);
        }

        [Fact]
        public void GetConsumerConfigTest()
        {
            var configOptions = Options.Create(new KafkaSettings
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new ConfigProvider(configOptions);
            var consumerconfig = configProvider.GetConsumerConfig();

            Assert.Equal("localhost:9092", consumerconfig.BootstrapServers);
            Assert.Equal("loly-agent", consumerconfig.GroupId);
        }

        [Fact]
        public void GetProducerConfigTest()
        {
            var configOptions = Options.Create(new KafkaSettings
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new ConfigProvider(configOptions);
            var producerConfig = configProvider.GetProducerConfig();

            Assert.Equal("localhost:9092", producerConfig.BootstrapServers);
        }
    }
}