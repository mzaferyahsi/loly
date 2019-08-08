using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loly.Agent.Discoveries;
using Loly.Agent.Tests.Helpers;
using Loly.Kafka;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Loly.Agent.Tests.Discoveries
{
    public class DiscoveryServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DiscoveryServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            TestFileHelper.Prepare();
        }
        [Fact]
        public void DiscoverTest()
        {
            Task task = new Task(() =>
            {
                _testOutputHelper.WriteLine("Producer hosted service started.");
            });
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
            

            var controller = new DiscoveryService(mock);
            controller.Discover("./");
        }
        
        [Fact]
        public void DiscoverHomePathTest()
        {
            Task task = new Task(() =>
            {
                _testOutputHelper.WriteLine("Producer hosted service started.");
            });
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
            

            var controller = new DiscoveryService(mock);
            controller.Discover("~/loly/file1.txt");
        }

        [Fact]
        public void DiscoverFileNotFoundTest()
        {
            Task task = new Task(() =>
            {
                _testOutputHelper.WriteLine("Producer hosted service started.");
            });
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
            

            var controller = new DiscoveryService(mock);
            controller.Discover("./.notfound");
        }
        
        [Fact]
        public void GetDiscoverTaskTest()
        {
            Task task = new Task(() =>
            {
                _testOutputHelper.WriteLine("Producer hosted service started.");
            });
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
            

            var controller = new DiscoveryService(mock);
            
            var discoverTask = controller.GetDiscoverTask("./");
            Assert.IsType<Task>(discoverTask);
            discoverTask.Start();
        }

        [Fact]
        public void DiscoverWithExclusionTest()
        {
            Task task = new Task(() =>
            {
                _testOutputHelper.WriteLine("Producer hosted service started.");
            });
            var mock = Mock.Of<IKafkaProducerHostedService>(x =>
                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task && x.StopAsync(It.IsAny<CancellationToken>()) == task);
            

            var controller = new DiscoveryService(mock);
            
            var exclusions = new List<string>() { "(~/loly/file1.txt)" };
            controller.Discover("~/loly/", exclusions);
        }

        public void Dispose()
        {
            TestFileHelper.Cleanup();
        }
    }
}