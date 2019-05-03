using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Loly.Agent.Discoveries;
using Loly.Agent.Discovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Loly.Agent.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ConfigureServicesTest()
        {
            var webHostBuilder = Program.CreateWebHostBuilder(new string[0]);
            var webHost = webHostBuilder.Build();
            Assert.NotNull(webHost);
        }

//        [Fact]
//        public void StartupConfigureTest()
//        {
//            var mockedServerFeatureCollection = new Mock<IFeatureCollection>();
//            var mockedServerAddressesFeature = new Mock<IServerAddressesFeature>();
//            mockedServerAddressesFeature.SetupGet(x => x.Addresses)
//                .Returns(new List<string>() {"http://localhost:8001"});
//            mockedServerFeatureCollection.Setup(x => x.Get<IServerAddressesFeature>())
//                .Returns(mockedServerAddressesFeature.Object);
//
//            var mockedServiceProvider = new Mock<IServiceProvider>();
//            var mockedApplicationBuilder = new ApplicationBuilder(mockedServiceProvider.Object);
//            mockedApplicationBuilder.Properties.Add("server.Features", mockedServerFeatureCollection.Object);
//            var mockedHostingEnvironment = new Mock<IHostingEnvironment>();
//            var mockedLoggerFactory = new Mock<ILoggerFactory>();
//
//            mockedHostingEnvironment.SetupGet(x => x.EnvironmentName).Returns(EnvironmentName.Development);
//            mockedLoggerFactory.Setup(x => x.AddProvider(It.IsAny<ILoggerProvider>()));
//
//
//            var startup = new Startup(Mock.Of<IConfiguration>());
//            
//            startup.Configure(mockedApplicationBuilder, mockedHostingEnvironment.Object, mockedLoggerFactory.Object);
//            
//            
//        }
    }
}

