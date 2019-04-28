using System.Threading;
using Confluent.Kafka;
using Loly.Agent.Kafka;
using Moq;
using Xunit;
using Newtonsoft.Json;

namespace Loly.Agent.Tests.Kafka
{

    public class MockKafkaProducerHostedService : KafkaProducerHostedService
    {
        public MockKafkaProducerHostedService(IKafkaConfigProducer configProducer) : base(configProducer)
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
    }
    
    public class KafkaProducerHostedServiceTests
    {
        [Fact]
        public void InitializeTest()
        {
            var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
            var producerHostedService = new KafkaProducerHostedService(mock);
            Assert.IsAssignableFrom<IKafkaProducerHostedService>(producerHostedService);
        }
        
        [Fact]
        public void UnscheduleTest()
        {
            var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
            var producerHostedService = new KafkaProducerHostedService(mock);
            Assert.IsAssignableFrom<IKafkaProducerHostedService>(producerHostedService);
            producerHostedService.StartAsync(CancellationToken.None);
            Thread.Sleep(30);
            producerHostedService.StopAsync(CancellationToken.None);
        }
        
        [Fact]
        public void RescheduleTest()
        {
            var mock = Mock.Of<IKafkaConfigProducer>(x => x.GetProducerConfig() == new ProducerConfig());
            var producerHostedService = new KafkaProducerHostedService(mock);
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
            var mockKafkaProducerHostedService = new Mock<MockKafkaProducerHostedService>(mockKafkaConfigProducer.Object);
            
            mockKafkaProducerHostedService.Object.AddMessage(message);

            await mockKafkaProducerHostedService.Object.StartAsync(CancellationToken.None);
            Thread.Sleep(100);
            
        }
        
    }
}