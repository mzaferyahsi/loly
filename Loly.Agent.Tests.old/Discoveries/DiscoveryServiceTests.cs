//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Loly.Agent.Discoveries;
//using Loly.Agent.Kafka;
//using Loly.Agent.Tests.Helpers;
//using Loly.Streaming;
//using Loly.Streaming.Producer;
//using Moq;
//using Xunit;
//using Xunit.Abstractions;
//
//namespace Loly.Agent.Tests.Discoveries
//{
//    public class DiscoveryServiceTests : IDisposable
//    {
//        public DiscoveryServiceTests(ITestOutputHelper testOutputHelper)
//        {
//            _testOutputHelper = testOutputHelper;
//            TestFileHelper.Prepare();
//        }
//
//        public void Dispose()
//        {
//            TestFileHelper.Cleanup();
//        }
//
//        private readonly ITestOutputHelper _testOutputHelper;
//
//        [Fact]
//        public void DiscoverFileNotFoundTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//            controller.Discover("./.notfound");
//        }
//
//        [Fact]
//        public void DiscoverHomePathTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//            controller.Discover("~/loly/file1.txt");
//        }
//
//        [Fact]
//        public void DiscoverTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//            controller.Discover("./");
//        }
//
//        [Fact]
//        public void DiscoverWithExclusionCaseInsensitiveTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task &&
//                x.StopAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//
//            var exclusions = new List<string> {"(~/loly/File1)"};
//            controller.Discover("~/loly/", exclusions);
//        }
//
//        [Fact]
//        public void DiscoverWithExclusionTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task &&
//                x.StopAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//
//            var exclusions = new List<string> {"(~/loly/file1.txt)"};
//            controller.Discover("~/loly/", exclusions);
//        }
//
//        [Fact]
//        public void GetDiscoverTaskTest()
//        {
//            var task = new Task(() => { _testOutputHelper.WriteLine("Producer hosted service started."); });
//            var mock = Mock.Of<IProducerHostedService>(x =>
//                x.Queue == new KafkaProducerQueue() && x.StartAsync(It.IsAny<CancellationToken>()) == task);
//
//
//            var controller = new DiscoveryService(mock);
//
//            var discoverTask = controller.GetDiscoverTask("./");
//            Assert.IsType<Task>(discoverTask);
//            discoverTask.Start();
//        }
//    }
//}