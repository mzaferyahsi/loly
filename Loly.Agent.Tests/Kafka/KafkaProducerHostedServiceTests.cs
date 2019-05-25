using System;
using System.Threading;
using Confluent.Kafka;
using Loly.Agent.Kafka;
using Moq;
using Xunit;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Loly.Agent.Tests.Kafka
{
    public class MockKafkaProducerHostedService : KafkaProducerHostedService
    {
        public MockKafkaProducerHostedService(IKafkaConfigProducer configProducer, IKafkaProducerQueue queue) : base(configProducer, queue)
        {
        }

        protected override IProducer<Null, string> GetProducer()
        {
            var message = new KafkaMessage()
            {
                Topic = "test-topic",
                Message = "HelloWorld"
            };

            var deliveryResult =
                new DeliveryResult<Null, string>
                {
                    Message = new Message<Null, string>() {Value = JsonConvert.SerializeObject(message.Message)}
                };

            var mockProducer = new Mock<IProducer<Null, string>>();
            mockProducer.Setup(x => x.ProduceAsync(message.Topic,
                It.IsAny<Message<Null, string>>())).ReturnsAsync(deliveryResult);

            return mockProducer.Object;
        }

        public void HandleError(Error error)
        {
            this.ErrorHandler(this.GetProducer(), error);
        }

        public void HandleLog(LogMessage message)
        {
            this.LogHandler(this.GetProducer(), message);
        }
    }

    public class KafkaProducerHostedServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public KafkaProducerHostedServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void InitializeTest()
        {
            var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
            var queue = new KafkaProducerQueue();
            var producerHostedService = new KafkaProducerHostedService(mock,queue);
            Assert.IsAssignableFrom<IKafkaProducerHostedService>(producerHostedService);
        }

        [Fact]
        public void UnscheduleTest()
        {
            try
            {
                var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
                var queue = new KafkaProducerQueue();
                var producerHostedService = new KafkaProducerHostedService(mock,queue);
                Assert.IsAssignableFrom<IKafkaProducerHostedService>(producerHostedService);
                producerHostedService.StartAsync(CancellationToken.None);
                Thread.Sleep(30);
                producerHostedService.StopAsync(CancellationToken.None);

            }
            catch (PlatformNotSupportedException e)
            {
                _testOutputHelper.WriteLine(e.ToString());
//                throw;
            }
        }

        [Fact]
        public void RescheduleTest()
        {
            var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
            var queue = new KafkaProducerQueue();
            var producerHostedService = new KafkaProducerHostedService(mock,queue);
            Assert.IsAssignableFrom<IKafkaProducerHostedService>(producerHostedService);
            producerHostedService.StartAsync(CancellationToken.None);
            Thread.Sleep(30);
            producerHostedService.StartAsync(CancellationToken.None);
            producerHostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async void PublishTest()
        {
            var message = new KafkaMessage()
            {
                Topic = "test-topic",
                Message = "HelloWorld"
            };

            var mockKafkaConfigProducer = new Mock<IKafkaConfigProducer>();
            var queue = new KafkaProducerQueue();
            queue.Enqueue(message);

            var mockKafkaProducerHostedService =
                new Mock<MockKafkaProducerHostedService>(mockKafkaConfigProducer.Object, queue);


            await mockKafkaProducerHostedService.Object.StartAsync(CancellationToken.None);
            Thread.Sleep(100);
        }
        
        [Fact]
        public void HandleErrorTest()
        {
            var mockKafkaConfigProducer = new Mock<IKafkaConfigProducer>();
            var queue = new KafkaProducerQueue();
            var mockKafkaProducerHostedService =
                new MockKafkaProducerHostedService(mockKafkaConfigProducer.Object, queue);
            
            mockKafkaProducerHostedService.HandleError(new Error(ErrorCode.Local_TimedOut));
            mockKafkaProducerHostedService.HandleError(new Error(ErrorCode.Local_TimedOut, "reason", true));
        }
        
        [Fact]
        public void HandleLogMessageTest()
        {
            var mockKafkaConfigProducer = new Mock<IKafkaConfigProducer>();
            var queue = new KafkaProducerQueue();
            var mockKafkaProducerHostedService =
                new MockKafkaProducerHostedService(mockKafkaConfigProducer.Object, queue);
            
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Info, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Alert, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Debug, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Error, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Notice, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Warning, "test", "test"));
            mockKafkaProducerHostedService.HandleLog(new LogMessage("TestMessage", SyslogLevel.Emergency, "test", "test"));
        }
        
    }
}