//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Loly.Agent.Api;
//using Loly.Agent.Configuration;
//using Loly.Agent.Discovery;
//using Loly.Agent.ErrorResults;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using Xunit;
//
//namespace Loly.Agent.Tests.Api
//{
//    public class DiscoveriesControllerTests
//    {
//        
//        LolyFeatureManager _featureManager = new LolyFeatureManager(null);
//
//        [Fact]
//        public void PostFailTest()
//        {
//            var discovery = new Models.Api.Discovery {Path = "./"};
//            var mock = new Mock<IDiscoveryService>();
//            mock.Setup(x => x.GetDiscoverTask("./")).Throws(new Exception("Error!"));
//            mock.Setup(x => x.GetDiscoverTask("./", null)).Throws(new Exception("Error!"));
//            var mockLogger = new Mock<ILogger<DiscoveriesController>>();
//            mockLogger.Setup(x => x.LogInformation(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogError(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogCritical(It.IsAny<string>())).Raises(Console.WriteLine);
//
//            var controller = new DiscoveriesController(mock.Object, _featureManager, mockLogger.Object);
//            var result = controller.Post(discovery);
//            Assert.IsType<InternalServerErrorResult>(result);
//        }
//
//        [Fact(Timeout = 300)]
//        public void PostTest()
//        {
//            var taskExecuted = false;
//            var discovery = new Models.Api.Discovery {Path = "./"};
//            var task = new Task(() => { taskExecuted = true; });
//            var mock = Mock.Of<IDiscoveryService>(l =>
//                l.GetDiscoverTask("./") == task && l.GetDiscoverTask("./", null) == task);
//            var mockLogger = new Mock<ILogger<DiscoveriesController>>();
//            mockLogger.Setup(x => x.LogInformation(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogError(It.IsAny<string>())).Raises(Console.WriteLine);
//            mockLogger.Setup(x => x.LogCritical(It.IsAny<string>())).Raises(Console.WriteLine);
//
//            var controller = new DiscoveriesController(mock, _featureManager, mockLogger.Object);
//            var result = controller.Post(discovery);
//            Assert.IsType<CreatedResult>(result);
//            while (!taskExecuted)
//            {
//                Thread.Sleep(10);
//            }
//            Assert.True(taskExecuted);
//        }
//    }
//}