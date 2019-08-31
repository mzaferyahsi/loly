//using System;
//using System.Threading;
//using Confluent.Kafka;
//using Loly.Agent.Analysers;
//using Loly.Agent.Configuration;
//using Loly.Analysers;
//using Loly.Kafka;
//using Loly.Kafka.Config;
//using Loly.Kafka.Consumer;
//using Loly.Kafka.Producer;
//using Microsoft.Extensions.Options;
//using Moq;
//using Xunit;
//
//namespace Loly.Agent.Tests.Analysers
//{
//    public class FileAnalyserHostedServiceTests
//    {
//        LolyFeatureManager _featureManager = new LolyFeatureManager(new OptionsWrapper<LolyFeatureConfiguration>(new LolyFeatureConfiguration()
//        {
//            AnalyseFile = true
//        }));
//        
//        [Fact]
//        public void ConsumerErrorTest()
//        {
//            var mockedKafkaConfigProducer = Mock.Of<IConfigProducer>(x =>
//                x.GetConsumerConfig() == new ConsumerConfig
//                {
//                    GroupId = "test-group"
//                } && x.GetProducerConfig() == new ProducerConfig());
//            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x =>
//                x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>
//                {
//                    Message = new Message<Ignore, string>
//                    {
//                        Value = "./"
//                    },
//                    Topic = "test-topic"
//                });
//
//            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
//            Action<IConsumer<Ignore, string>, Error> errorAction = null;
//
//            var mockedConsumerProvider = new Mock<IConsumerProvider>();
//            mockedConsumerProvider.Setup(x => x.Get<Ignore, string>())
//                .Returns(mockedConsumer);
//            mockedConsumerProvider.Setup(x =>
//                    x.Get(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
//                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
//                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>(
//                    (log, error) =>
//                    {
//                        logAction = log;
//                        errorAction = error;
//                    })
//                .Returns(mockedConsumer);
//
//            var fileAnalyserHostedService = new OldFileAnalyserHostedService(mockedKafkaConfigProducer,
//                mockedConsumerProvider.Object,
//                new FileAnalyser(), _featureManager);
//
//            fileAnalyserHostedService.StartAsync(CancellationToken.None);
//            Assert.NotNull(errorAction);
//            try
//            {
//                errorAction.Invoke(mockedConsumer, new Error(ErrorCode.Local_TimedOut));
//            }
//            catch (KafkaException e)
//            {
//                Assert.Equal("Local: Timed out", e.Message);
//            }
//            finally
//            {
//                fileAnalyserHostedService.StopAsync(CancellationToken.None);
//            }
//
//            fileAnalyserHostedService.Dispose();
//
//            fileAnalyserHostedService = new OldFileAnalyserHostedService(mockedKafkaConfigProducer,
//                mockedConsumerProvider.Object,
//                new FileAnalyser(),_featureManager);
//
//            fileAnalyserHostedService.StartAsync(CancellationToken.None);
//            Assert.NotNull(errorAction);
//            try
//            {
//                errorAction.Invoke(mockedConsumer, new Error(ErrorCode.Local_TimedOut, "reason", true));
//            }
//            catch (KafkaException e)
//            {
//                Assert.Equal("Local: Timed out", e.Message);
//            }
//            finally
//            {
//                fileAnalyserHostedService.StopAsync(CancellationToken.None);
//            }
//        }
//
//        [Fact]
//        public void InitializeTest()
//        {
//            var mockedKafkaConfigProducer = Mock.Of<IConfigProducer>(x =>
//                x.GetConsumerConfig() == new ConsumerConfig
//                {
//                    GroupId = "test-group"
//                } && x.GetProducerConfig() == new ProducerConfig());
//            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>();
//            var mockedConsumerProvider =
//                Mock.Of<IConsumerProvider>(x => x.Get<Ignore, string>() == mockedConsumer);
//
//            var fileAnalyserHostedService = new OldFileAnalyserHostedService(mockedKafkaConfigProducer,
//                mockedConsumerProvider,
//                new FileAnalyser(),_featureManager);
//        }
//
//        [Fact]
//        public void LogMessageHandlerTest()
//        {
//            var mockedKafkaConfigProducer = Mock.Of<IConfigProducer>(x =>
//                x.GetConsumerConfig() == new ConsumerConfig
//                {
//                    GroupId = "test-group"
//                } && x.GetProducerConfig() == new ProducerConfig());
//            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x =>
//                x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>
//                {
//                    Message = new Message<Ignore, string>
//                    {
//                        Value = "./"
//                    },
//                    Topic = "test-topic"
//                });
//
//            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
//            Action<IConsumer<Ignore, string>, Error> errorAction = null;
//
//            var mockedConsumerProvider = new Mock<IConsumerProvider>();
//            mockedConsumerProvider.Setup(x => x.Get<Ignore, string>())
//                .Returns(mockedConsumer);
//            mockedConsumerProvider.Setup(x =>
//                    x.Get(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
//                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
//                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>(
//                    (log, error) =>
//                    {
//                        logAction = log;
//                        errorAction = error;
//                    })
//                .Returns(mockedConsumer);
//            var queue = new KafkaProducerQueue();
//            var fileAnalyserHostedService = new OldFileAnalyserHostedService(mockedKafkaConfigProducer,
//                mockedConsumerProvider.Object,
//                new FileAnalyser(),_featureManager);
//
//            fileAnalyserHostedService.StartAsync(CancellationToken.None);
//            Assert.NotNull(logAction);
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Info, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Error, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Warning, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Emergency, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Alert, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Debug, "test", "test"));
//            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Notice, "test", "test"));
//            fileAnalyserHostedService.StopAsync(CancellationToken.None);
//        }
//
//        [Fact]
//        public void StartAndReStartTest()
//        {
//            var mockedKafkaConfigProducer = Mock.Of<IConfigProducer>(x =>
//                x.GetConsumerConfig() == new ConsumerConfig
//                {
//                    GroupId = "test-group"
//                } && x.GetProducerConfig() == new ProducerConfig());
//            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x =>
//                x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>
//                {
//                    Message = new Message<Ignore, string>
//                    {
//                        Value = "./"
//                    },
//                    Topic = "test-topic"
//                });
//
//            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
//            Action<IConsumer<Ignore, string>, Error> errorAction = null;
//
//            var mockedConsumerProvider = new Mock<IConsumerProvider>();
//            mockedConsumerProvider.Setup(x => x.Get<Ignore, string>())
//                .Returns(mockedConsumer);
//            mockedConsumerProvider.Setup(x =>
//                    x.Get(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
//                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
//                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>(
//                    (log, error) =>
//                    {
//                        logAction = log;
//                        errorAction = error;
//                    })
//                .Returns(mockedConsumer);
//            var fileAnalyserHostedService = new OldFileAnalyserHostedService(mockedKafkaConfigProducer,
//                mockedConsumerProvider.Object,
//                new FileAnalyser(),_featureManager);
//
//            fileAnalyserHostedService.StartAsync(CancellationToken.None);
//            fileAnalyserHostedService.StartAsync(CancellationToken.None);
//            fileAnalyserHostedService.StopAsync(CancellationToken.None);
//        }
//    }
//}