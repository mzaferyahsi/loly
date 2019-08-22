using System;
using System.Threading;
using System.Threading.Tasks;
using Loly.Agent.Api;
using Loly.Agent.Configuration;
using Loly.Agent.Discovery;
using Loly.Agent.ErrorResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Loly.Agent.Tests.Api
{
    public class DiscoveriesControllerTests
    {
        
        LolyFeatureManager _featureManager = new LolyFeatureManager(null);

        [Fact]
        public void PostFailTest()
        {
            var discovery = new Models.Api.Discovery {Path = "./"};
            var mock = new Mock<IDiscoveryService>();
            mock.Setup(x => x.GetDiscoverTask("./")).Throws(new Exception("Error!"));
            mock.Setup(x => x.GetDiscoverTask("./", null)).Throws(new Exception("Error!"));

            var controller = new DiscoveriesController(mock.Object, _featureManager);
            var result = controller.Post(discovery);
            Assert.IsType<InternalServerErrorResult>(result);
        }

        [Fact]
        public void PostTest()
        {
            var taskExecuted = false;
            var discovery = new Models.Api.Discovery {Path = "./"};
            var task = new Task(() => { taskExecuted = true; });
            var mock = Mock.Of<IDiscoveryService>(l =>
                l.GetDiscoverTask("./") == task && l.GetDiscoverTask("./", null) == task);

            var controller = new DiscoveriesController(mock, _featureManager);
            var result = controller.Post(discovery);
            Assert.IsType<CreatedResult>(result);
            Thread.Sleep(100);
            Assert.True(taskExecuted);
        }
    }
}