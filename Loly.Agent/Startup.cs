using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Confluent.Kafka;
using Hangfire;
using HeyRed.Mime;
using log4net;
using Loly.Agent.Analysers;
using Loly.Agent.Discoveries;
using Loly.Agent.Discovery;
using Loly.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Loly.Agent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("Configs/loly.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    var enumConverter = new Newtonsoft.Json.Converters.StringEnumConverter()
                        {NamingStrategy = new CamelCaseNamingStrategy()};
                    options.SerializerSettings.Converters.Add(enumConverter);
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });
            ;
            services.AddHangfire(configuration => configuration
                .UseLog4NetLogProvider());

            services.AddSingleton<IDiscoveryService, DiscoveryService>();

            services.AddOptions();
            
            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.AddTransient<IKafkaConfigProducer, KafkaConfigProvider>();
//            services.AddSingleton<IKafkaProducerQueue, KafkaProducerQueue>();
//            services.AddHostedService<KafkaProducerHostedService>();
            services.AddSingleton<IKafkaConsumerProvider, KafkaConsumerProvider>();
            services.AddHostedService<FileAnalyserHostedService>();
            services.AddSingleton<FileAnalyser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddLog4Net("Configs/log4net.config");
            app.UseHttpsRedirection();
            app.UseMvc();

            var log = LogManager.GetLogger(typeof(Program));
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            log.InfoFormat("Application started at {0}", string.Join(", ", serverAddressesFeature.Addresses));

            try
            {
                var isDockerEnv = Environment.GetEnvironmentVariable("IS_DOCKER");
                if (Boolean.Parse(isDockerEnv))
                {
                    var osPath = string.Empty;
                    if (OperatingSystem.IsLinux())
                        osPath = "linux-x64";
                    else if (OperatingSystem.IsWindows())
                        osPath = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "win-x86" : "win-x64";
                    else if (OperatingSystem.IsMacOS())
                        osPath = "osx-x64";

                    var path = Path.Join(Directory.GetCurrentDirectory(), "runtimes");
                    path = Path.Join(path, osPath, "native");
                    path = Path.Join(path, "magic.mgc");

                    MimeGuesser.MagicFilePath = path;
                }
            }
            catch (ArgumentNullException e)
            {
                log.Debug(e);
            }         
        }
    }
    
    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}