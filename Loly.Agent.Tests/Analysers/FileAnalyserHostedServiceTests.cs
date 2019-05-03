using System;
using System.Threading;
using Confluent.Kafka;
using Loly.Agent.Analysers;
using Loly.Agent.Kafka;
using Loly.Agent.Services;
using Loly.Agent.Tests.Kafka;
using Moq;
using Xunit;

namespace Loly.Agent.Tests.Analysers
{
    public class FileAnalyserHostedServiceTests
    {
        [Fact]
        public void InitializeTest()
        {
            var mockedKafkaConfigProducer = Mock.Of<IKafkaConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig()
            {
                GroupId = "test-group"
            } && x.GetProducerConfig() == new ProducerConfig());            
            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>();
            var mockedConsumerProvider = Mock.Of<IKafkaConsumerProvider>(x => x.GetConsumer<Ignore, string>() == mockedConsumer);
            var fileAnalyserHostedService = new FileAnalyserHostedService(new MockKafkaProducerHostedService(mockedKafkaConfigProducer), mockedConsumerProvider,
                new FileAnalyser());
        }
        
        [Fact]
        public void StartAndReStartTest()
        {
            var mockedKafkaConfigProducer = Mock.Of<IKafkaConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig()
            {
                GroupId = "test-group"
            } && x.GetProducerConfig() == new ProducerConfig());            
            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x => x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>()
            {
                Message = new Message<Ignore, string>()
                {
                    Value = "./",
                },
                Topic = "test-topic"
            });

            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
            Action<IConsumer<Ignore, string>, Error> errorAction = null;

            var mockedConsumerProvider = new Mock<IKafkaConsumerProvider>();
            mockedConsumerProvider.Setup(x => x.GetConsumer<Ignore, string>())
                .Returns(mockedConsumer);
            mockedConsumerProvider.Setup(x =>
                    x.GetConsumer<Ignore, string>(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>((log, error) =>
                {
                    logAction = log;
                    errorAction = error;
                })
                .Returns(mockedConsumer);
            var fileAnalyserHostedService = new FileAnalyserHostedService(new MockKafkaProducerHostedService(mockedKafkaConfigProducer), mockedConsumerProvider.Object,
                new FileAnalyser());

            fileAnalyserHostedService.StartAsync(CancellationToken.None);
            fileAnalyserHostedService.StartAsync(CancellationToken.None);
            fileAnalyserHostedService.StopAsync(CancellationToken.None);
        }
        
        [Fact]
        public void LogMessageHandlerTest()
        {
            var mockedKafkaConfigProducer = Mock.Of<IKafkaConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig()
            {
                GroupId = "test-group"
            } && x.GetProducerConfig() == new ProducerConfig());            
            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x => x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>()
            {
                Message = new Message<Ignore, string>()
                {
                    Value = "./",
                },
                Topic = "test-topic"
            });

            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
            Action<IConsumer<Ignore, string>, Error> errorAction = null;

            var mockedConsumerProvider = new Mock<IKafkaConsumerProvider>();
            mockedConsumerProvider.Setup(x => x.GetConsumer<Ignore, string>())
                .Returns(mockedConsumer);
            mockedConsumerProvider.Setup(x =>
                    x.GetConsumer<Ignore, string>(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>((log, error) =>
                {
                    logAction = log;
                    errorAction = error;
                })
                .Returns(mockedConsumer);
            var fileAnalyserHostedService = new FileAnalyserHostedService(new MockKafkaProducerHostedService(mockedKafkaConfigProducer), mockedConsumerProvider.Object,
                new FileAnalyser());

            fileAnalyserHostedService.StartAsync(CancellationToken.None);
            Assert.NotNull(logAction);
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Info, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Error, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Warning, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Emergency, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Alert, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Debug, "test", "test"));
            logAction.Invoke(mockedConsumer, new LogMessage("TestMessage", SyslogLevel.Notice, "test", "test"));
            fileAnalyserHostedService.StopAsync(CancellationToken.None);
        }
        
        [Fact]
        public void ConsumerErrorTest()
        {
            var mockedKafkaConfigProducer = Mock.Of<IKafkaConfigProducer>(x => x.GetConsumerConfig() == new ConsumerConfig()
            {
                GroupId = "test-group"
            } && x.GetProducerConfig() == new ProducerConfig());            
            var mockedConsumer = Mock.Of<IConsumer<Ignore, string>>(x => x.Consume(It.IsAny<CancellationToken>()) == new ConsumeResult<Ignore, string>()
            {
                Message = new Message<Ignore, string>()
                {
                    Value = "./",
                },
                Topic = "test-topic"
            });

            Action<IConsumer<Ignore, string>, LogMessage> logAction = null;
            Action<IConsumer<Ignore, string>, Error> errorAction = null;

            var mockedConsumerProvider = new Mock<IKafkaConsumerProvider>();
            mockedConsumerProvider.Setup(x => x.GetConsumer<Ignore, string>())
                .Returns(mockedConsumer);
            mockedConsumerProvider.Setup(x =>
                    x.GetConsumer<Ignore, string>(It.IsAny<Action<IConsumer<Ignore, string>, LogMessage>>(),
                        It.IsAny<Action<IConsumer<Ignore, string>, Error>>()))
                .Callback<Action<IConsumer<Ignore, string>, LogMessage>, Action<IConsumer<Ignore, string>, Error>>((log, error) =>
                {
                    logAction = log;
                    errorAction = error;
                })
                .Returns(mockedConsumer);
            var fileAnalyserHostedService = new FileAnalyserHostedService(new MockKafkaProducerHostedService(mockedKafkaConfigProducer), mockedConsumerProvider.Object,
                new FileAnalyser());

            fileAnalyserHostedService.StartAsync(CancellationToken.None);
            Assert.NotNull(errorAction);
            try
            {
                errorAction.Invoke(mockedConsumer, new Error(ErrorCode.Local_TimedOut));
            }
            catch (KafkaException e)
            {
                Assert.Equal("Local: Timed out", e.Message);
            }
            finally
            {
                fileAnalyserHostedService.StopAsync(CancellationToken.None);
            }
            
            fileAnalyserHostedService.StartAsync(CancellationToken.None);
            Assert.NotNull(errorAction);
            try
            {
                errorAction.Invoke(mockedConsumer, new Error(ErrorCode.Local_TimedOut,"reason", true));
            }
            catch (KafkaException e)
            {
                Assert.Equal("Local: Timed out", e.Message);
            }
            finally
            {
                fileAnalyserHostedService.StopAsync(CancellationToken.None);
            }
            
        }
    }
}