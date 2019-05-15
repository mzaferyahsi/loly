using System.Threading;
using System.Threading.Tasks;
using Loly.Agent.Discoveries;
using Loly.Agent.Kafka;
using Moq;
using Xunit;

namespace Loly.Agent.Tests.Discoveries
{
    public class DiscoveryServiceTests
    {
        [Fact]
        public void DiscoverTest()
        {
            var queue = new KafkaProducerQueue();
            var controller = new DiscoveryService(queue);
            controller.Discover("./");
        }
        
        [Fact]
        public void DiscoverHomePathTest()
        {
            var queue = new KafkaProducerQueue();
            var controller = new DiscoveryService(queue);
            controller.Discover("~/found.txt");
        }

        [Fact]
        public void DiscoverFileNotFoundTest()
        {
            var queue = new KafkaProducerQueue();
            var controller = new DiscoveryService(queue);
            controller.Discover("./.notfound");
        }
        
        [Fact]
        public void GetDiscoverTaskTest()
        {
            var queue = new KafkaProducerQueue();
            var controller = new DiscoveryService(queue);
            var task = controller.GetDiscoverTask("./");
            Assert.IsType<Task>(task);
            task.Start();
        }
    }
}