//using System;
//using System.Threading;
//using Confluent.Kafka;
//using Loly.Agent.Kafka;
//using Loly.Kafka;
//using Loly.Kafka.Config;
//using Loly.Kafka.Models;
//using Loly.Kafka.Producer;
//using Moq;
//using Newtonsoft.Json;
//using Xunit;
//using Xunit.Abstractions;
//
//namespace Loly.Agent.Tests.Kafka
//{
//    public class MockProducerHostedService : ProducerService
//    {
//        protected ITestOutputHelper _testOutputHelper;
//
//        public MockProducerHostedService(IConfigProducer configProducer, IProducerQueue queue,
//            ITestOutputHelper testOutputHelper) : base(configProducer, queue)
//        {
//            _testOutputHelper = testOutputHelper;
//        }
//
//        public bool TestCompleted { get; set; }
//
//
//        protected override IProducer<Null, string> GetProducer()
//        {
//            var message = new KafkaMessage
//            {
//                Topic = "test-topic",
//                Message = "HelloWorld"
//            };
//
//            var deliveryResult =
//                new DeliveryResult<Null, string>
//                {
//                    Message = new Message<Null, string> {Value = JsonConvert.SerializeObject(message.Message)}
//                };
//
//            var mockProducer = new Mock<IProducer<Null, string>>();
//            mockProducer.Setup(x => x.ProduceAsync(message.Topic,
//                    It.IsAny<Message<Null, string>>()))
//                .ReturnsAsync(() =>
//                {
//                    TestCompleted = true;
//                    return deliveryResult;
//                });
//
//            return mockProducer.Object;
//        }
//
//        public void HandleError(Error error)
//        {
//            this.ErrorHandler(GetProducer(), error);
//        }
//
//        public void HandleLog(LogMessage message)
//        {
//            this.LogHandler(GetProducer(), message);
//        }
//    }
//
//    public class MockProducerHostedServiceFailOnProduce : MockProducerHostedService
//    {
//        public MockProducerHostedServiceFailOnProduce(IConfigProducer configProducer,
//            IProducerQueue queue, ITestOutputHelper testOutputHelper) : base(configProducer, queue,
//            testOutputHelper)
//        {
//        }
//
//
//        protected override IProducer<Null, string> GetProducer()
//        {
//            var message = new KafkaMessage
//            {
//                Topic = "test-topic",
//                Message = "HelloWorld"
//            };
//
//            var deliveryResult =
//                new DeliveryResult<Null, string>
//                {
//                    Message = new Message<Null, string> {Value = JsonConvert.SerializeObject(message.Message)}
//                };
//
//            var mockProducer = new Mock<IProducer<Null, string>>();
//            mockProducer.Setup(x =>
//                    x.ProduceAsync(message.Topic, It.IsAny<Message<Null, string>>()))
//                .ReturnsAsync(() =>
//                {
//                    TestCompleted = true;
//                    _testOutputHelper.WriteLine("Throwing exception");
//                    throw new ProduceException<Null, string>(new Error(ErrorCode.Local_Fail), null);
//                });
//
//            return mockProducer.Object;
//        }
//    }
//
//    public class KafkaProducerHostedServiceTests
//    {
//        private readonly ITestOutputHelper _testOutputHelper;
//
//        public KafkaProducerHostedServiceTests(ITestOutputHelper testOutputHelper)
//        {
//            _testOutputHelper = testOutputHelper;
//        }
//
//        [Fact]
//        public void InitializeTest()
//        {
//            var mock = Mock.Of<IConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
//            var queue = new KafkaProducerQueue();
//            var producerHostedService = new ProducerService(mock, queue);
//            Assert.IsAssignableFrom<IProducerHostedService>(producerHostedService);
//        }
//
//        [Fact]
//        public void UnscheduleTest()
//        {
//            try
//            {
//                var mock = Mock.Of<IConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
//                var queue = new KafkaProducerQueue();
//                var producerHostedService = new ProducerService(mock, queue);
//                Assert.IsAssignableFrom<IProducerHostedService>(producerHostedService);
//                producerHostedService.Start(CancellationToken.None);
//                Thread.Sleep(30);
//                producerHostedService.Stop(CancellationToken.None);
//            }
//            catch (PlatformNotSupportedException e)
//            {
//                _testOutputHelper.WriteLine(e.ToString());
////                throw;
//            }
//        }
//
//        [Fact]
//        public void RescheduleTest()
//        {
//            var mock = Mock.Of<IConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
//            var queue = new KafkaProducerQueue();
//            var producerHostedService = new ProducerService(mock, queue);
//            Assert.IsAssignableFrom<IProducerHostedService>(producerHostedService);
//            producerHostedService.Start(CancellationToken.None);
//            Thread.Sleep(30);
//            producerHostedService.Start(CancellationToken.None);
//            producerHostedService.Stop(CancellationToken.None);
//        }
//
//        [Fact]
//        public async void PublishTest()
//        {
//            var message = new KafkaMessage
//            {
//                Topic = "test-topic",
//                Message = "HelloWorld"
//            };
//
//            var mockKafkaConfigProducer = new Mock<IConfigProducer>();
//            var queue = new KafkaProducerQueue();
//            queue.Enqueue(message);
//
//            var mockKafkaProducerHostedService =
//                new MockProducerHostedService(mockKafkaConfigProducer.Object, queue, _testOutputHelper);
//
//
//            await mockKafkaProducerHostedService.Start(CancellationToken.None);
//            Thread.Sleep(100);
//        }
//
//        [Fact]
//        public async void PublishFailTest()
//        {
//            var message = new KafkaMessage
//            {
//                Topic = "test-topic",
//                Message = "HelloWorld"
//            };
//
//            var mockKafkaConfigProducer = new Mock<IConfigProducer>();
//            var queue = new KafkaProducerQueue();
//            queue.Enqueue(message);
//
//            var mockKafkaProducerHostedService =
//                new MockProducerHostedServiceFailOnProduce(mockKafkaConfigProducer.Object, queue,
//                    _testOutputHelper);
//
//
//            await mockKafkaProducerHostedService.Start(CancellationToken.None);
//            while (!mockKafkaProducerHostedService.TestCompleted)
//            {
//                _testOutputHelper.WriteLine("waiting..");
//                Thread.Sleep(100);
//            }
//
//            Thread.Sleep(100);
//            _testOutputHelper.WriteLine($"completed, ${mockKafkaProducerHostedService.TestCompleted}");
//        }
//
//        [Fact]
//        public void HandleErrorTest()
//        {
//            var mockKafkaConfigProducer = new Mock<IConfigProducer>();
//            var queue = new KafkaProducerQueue();
//            var mockKafkaProducerHostedService =
//                new MockProducerHostedService(mockKafkaConfigProducer.Object, queue, _testOutputHelper);
//
//            mockKafkaProducerHostedService.HandleError(new Error(ErrorCode.Local_TimedOut));
//            mockKafkaProducerHostedService.HandleError(new Error(ErrorCode.Local_TimedOut, "reason", true));
//        }
//
//        [Fact]
//        public void HandleLogMessageTest()
//        {
//            var mockKafkaConfigProducer = new Mock<IConfigProducer>();
//            var queue = new KafkaProducerQueue();
//            var mockKafkaProducerHostedService =
//                new MockProducerHostedService(mockKafkaConfigProducer.Object, queue, _testOutputHelper);
//
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Info, "test", "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Alert, "test", "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Debug, "test", "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Error, "test", "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Notice, "test", "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Warning, "test",
//                "test"));
//            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Emergency, "test",
//                "test"));
//        }
//    }
//}