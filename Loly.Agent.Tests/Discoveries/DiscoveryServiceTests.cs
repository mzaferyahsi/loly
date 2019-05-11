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
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            controller.Discover("./");
        }
        
        [Fact]
        public void DiscoverHomePathTest()
        {
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            controller.Discover("~/");
        }

        [Fact]
        public void DiscoverFileNotFoundTest()
        {
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            controller.Discover("./.notfound");
        }
        
        [Fact]
        public void GetDiscoverTaskTest()
        {
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            var task = controller.GetDiscoverTask("./");
            Assert.IsType<Task>(task);
            task.Start();
        }
    }
}