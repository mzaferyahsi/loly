using System;
using System.Threading.Tasks;
using Loly.Agent.Api;
using Loly.Agent.Controllers;
using Loly.Agent.Discovery;
using Loly.Kafka;
using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using System.Threading;
using Loly.Agent.ErrorResults;

namespace Loly.Agent.Tests.Api
{
    public class DiscoveriesControllerTests
    {
        [Fact]
        public void PostTest()
        {
            var taskExecuted = false;
            var discovery = new Models.Discovery() { Path = "./" };
            var task = new Task((() => { taskExecuted = true; }));
            var mock = Mock.Of<IDiscoveryService>(l => l.GetDiscoverTask("./") == task);

            var controller = new DiscoveriesController(mock);
            var result = controller.Post(discovery);
            Assert.IsType<CreatedResult>(result);
            Thread.Sleep(50);
            Assert.True(taskExecuted);
        }

        [Fact]
        public void PostFailTest()
        {
            var discovery = new Models.Discovery() { Path = "./" };
            var mock = new Mock<IDiscoveryService>();
            mock.Setup(x => x.GetDiscoverTask("./")).Throws(new Exception("Error!"));

            var controller = new DiscoveriesController(mock.Object);
            var result = controller.Post(discovery);
            Assert.IsType<InternalServerErrorResult>(result);
        }
    }
}