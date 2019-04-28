using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Agent.Discovery;
using Loly.Agent.Kafka;
using Loly.Agent.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Loly.Agent.Tests.Discovery
{
    public class DiscoveryServiceTests
    {
        [Fact]
        public void DiscoverTest()
        {
            var mock = Mock.Of<IKafkaProducerHostedService>(x => x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            controller.Discover("./");            
        }

        [Fact]
        public void GetDiscoverTaskTest()
        {
            var mock = Mock.Of<IKafkaProducerHostedService>(x => x.StartAsync(It.IsAny<CancellationToken>()) == Task.CompletedTask);
            var controller = new DiscoveryService(mock);
            var task = controller.GetDiscoverTask("./");
            Assert.IsType<Task>(task);
        }
    }
}