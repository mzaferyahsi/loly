//using System.Collections.Generic;
//using System.Linq;
//using Loly.Agent.Discoveries;
//using Microsoft.AspNetCore.Builder.Internal;
//using Microsoft.AspNetCore.Mvc.Internal;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Primitives;
//using Xunit;
//
//namespace Loly.Agent.Tests
//{
//    public class TestConfiguration : IConfiguration
//    {
//        public IConfigurationSection GetSection(string key)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public IEnumerable<IConfigurationSection> GetChildren()
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public IChangeToken GetReloadToken()
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public string this[string key]
//        {
//            get => throw new System.NotImplementedException();
//            set => throw new System.NotImplementedException();
//        }
//    }
//    public class ProgramTests
//    {
//        [Fact]
//        public void ConfigureServicesTest()
//        {
//            var testConfig = new TestConfiguration();
//            var startup = new Startup(testConfig);
//            var serviceCollection = new ServiceCollection();
//            startup.ConfigureServices(serviceCollection);
//            
//            Assert.Equal(testConfig, startup.Configuration);
//            Assert.True(serviceCollection.Count > 0);
//            Assert.Contains(serviceCollection, x => x.Lifetime == ServiceLifetime.Transient && x.ServiceType == typeof(IDiscoveryService));
//        }
//    }
//}