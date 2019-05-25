using System.Threading;
using System.Threading.Tasks;
using Loly.Agent.Discoveries;
using Loly.Agent.Kafka;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Loly.Agent.Tests.Discoveries
{
    public class DiscoveryServiceTests
    {
        [Fact]
        public void DiscoverTest()
        {
            var configOptions = Options.Create(new KafkaSettings()
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig()
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new KafkaConfigProvider(configOptions);
            var controller = new DiscoveryService(configProvider);
            controller.Discover("./");
        }
        
        [Fact]
        public void DiscoverHomePathTest()
        {
            var configOptions = Options.Create(new KafkaSettings()
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig()
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new KafkaConfigProvider(configOptions);
            var controller = new DiscoveryService(configProvider);
            controller.Discover("~/found.txt");
        }

        [Fact]
        public void DiscoverFileNotFoundTest()
        {
            var configOptions = Options.Create(new KafkaSettings()
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig()
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new KafkaConfigProvider(configOptions);
            var controller = new DiscoveryService(configProvider);
            controller.Discover("./.notfound");
        }
        
        [Fact]
        public void GetDiscoverTaskTest()
        {
            var configOptions = Options.Create(new KafkaSettings()
            {
                BootstrapServers = "localhost:9092",
                Consumer = new KafkaConsumerConfig()
                {
                    GroupId = "loly-agent"
                }
            });

            var configProvider = new KafkaConfigProvider(configOptions);
            var controller = new DiscoveryService(configProvider);
            var task = controller.GetDiscoverTask("./");
            Assert.IsType<Task>(task);
            task.Start();
        }
    }
}