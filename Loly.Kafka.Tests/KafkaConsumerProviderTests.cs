//using Confluent.Kafka;
//using Loly.Kafka;
//using Loly.Kafka.Config;
//using Loly.Kafka.Consumer;
//using Moq;
//using Xunit;
//
//namespace Loly.Agent.Tests.Kafka
//{
//    public class KafkaConsumerProviderTests
//    {
//        [Fact]
//        public void GetProducerTest()
//        {
//            var mockConfigProdider = Mock.Of<IConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig
//            {
//                GroupId = "test-group"
//            });
//            var consumerProvider = new ConsumerProvider(mockConfigProdider);
//            var consumer = consumerProvider.Get<Ignore, string>();
//            Assert.IsAssignableFrom<IConsumer<Ignore, string>>(consumer);
//        }
//
//        [Fact]
//        public void GetProducerWithHandlersTest()
//        {
//            var mockConfigProdider = Mock.Of<IConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig
//            {
//                GroupId = "test-group"
//            });
//            var consumerProvider = new ConsumerProvider(mockConfigProdider);
//            var consumer = consumerProvider.Get<Ignore, string>(
//                (consumer1, message) => Assert.IsAssignableFrom<IConsumer<Ignore, string>>(consumer1),
//                (consumer1, error) => Assert.IsAssignableFrom<IConsumer<Ignore, string>>(consumer1));
//            Assert.IsAssignableFrom<IConsumer<Ignore, string>>(consumer);
//        }
//    }
//}