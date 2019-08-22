using System;
using System.IO;
using System.Runtime.InteropServices;
using Hangfire;
using HeyRed.Mime;
using log4net;
using Loly.Agent.Analysers;
using Loly.Agent.Configuration;
using Loly.Agent.Discoveries;
using Loly.Agent.Discovery;
using Loly.Analysers;
using Loly.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Loly.Agent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddJsonFile("Configs/loly.json", true, true)
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
                    var enumConverter = new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()};
                    options.SerializerSettings.Converters.Add(enumConverter);
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
            ;
            services.AddHangfire(configuration => configuration
                .UseLog4NetLogProvider());


            services.AddOptions();

            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.Configure<LolyFeatureConfiguration>(Configuration.GetSection("Features"));
            services.AddTransient<IKafkaConfigProducer, KafkaConfigProvider>();
            services.AddTransient<IKafkaProducerHostedService, KafkaProducerHostedService>();
            services.AddSingleton<IKafkaConsumerProvider, KafkaConsumerProvider>();
            services.AddSingleton<LolyFeatureManager>();
            services.AddSingleton<IDiscoveryService, DiscoveryService>();
            services.AddSingleton<FileAnalyser>();
            services.AddHostedService<FileAnalyserHostedService>();
            services.AddSingleton<FileHashAnalyser>();
            services.AddHostedService<FileHashAnalyserHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            loggerFactory.AddLog4Net("Configs/log4net.config");
            app.UseHttpsRedirection();
            app.UseMvc();

            var log = LogManager.GetLogger(typeof(Program));
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            log.InfoFormat("Application started at {0}", string.Join(", ", serverAddressesFeature.Addresses));

            try
            {
                var isDockerEnv = Environment.GetEnvironmentVariable("IS_DOCKER");
                
                if (!String.IsNullOrEmpty(isDockerEnv) && bool.Parse(isDockerEnv))
                {
                    var osPath = string.Empty;
                    if (OperatingSystem.IsLinux())
                        osPath = "linux-x64";
                    else if (OperatingSystem.IsWindows())
                        osPath = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "win-x86" : "win-x64";
                    else if (OperatingSystem.IsMacOs())
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
        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool IsMacOs()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}