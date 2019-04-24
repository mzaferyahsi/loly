using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Loly.Agent.Discoveries;
using Loly.Agent.Models;
using Xunit;

namespace Loly.Agent.Tests.Discoveries
{
    public class DiscoveriesControllerTests
    {
        [Fact]
        public void DiscoverTest()
        {
            var controller = new DiscoveriesController();
            var discovery = new Discovery();
            discovery.Path = "/";
            controller.Discover(discovery);            
        }
    }
}