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