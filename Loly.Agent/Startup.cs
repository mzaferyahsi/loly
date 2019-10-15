using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Elastic.Apm.AspNetCore;
using HeyRed.Mime;
using Loly.Agent.Analysers;
using Loly.Agent.Discoveries;
using Loly.Analysers;
using Loly.Configuration;
using Loly.Configuration.Agent;
using Loly.Streaming.Config;
using Loly.Streaming.Consumer;
using Loly.Streaming.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Loly.Agent
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
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
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails();

            EnvironmentLoggerConfigurationExtensions.WithMachineName(loggerConfiguration.Enrich);

            var elasticConfig = Configuration.GetSection("Elasticsearch").Get<ElasticsearchConfiguration>();
            if (elasticConfig != null)
            {
                Console.WriteLine("Elasticsearch configuration found.");
                Console.WriteLine($"Elasticsearch Uri is {elasticConfig.Uri}");

                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticConfig.Uri))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = "loly-logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Debug
                });
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                var enumConverter = new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()};
                options.SerializerSettings.Converters.Add(enumConverter);
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
            
            
            InjectServices(services);
        }

        private void InjectServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<LolyAgentFeatureConfiguration>(Configuration.GetSection("Features"));
            services.AddSingleton<LolyAgentFeatureManager>();

            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.AddTransient<IConfigProducer, ConfigProvider>();
            services.AddSingleton<IConsumerProvider, ConsumerProvider>();
            services.Configure<ElasticsearchConfiguration>(Configuration.GetSection("Elasticsearch"));

            services.AddSingleton<IDiscoveryService, DiscoveryService>();
            services.AddSingleton<FileAnalyser>();
            services.AddSingleton<FileHashAnalyser>();
            services.AddSingleton<ImageMetadataAnalyser>();

            services.AddHostedService<FileAnalyserHostedService>();
            services.AddHostedService<FileHashAnalyserHostedService>();
            services.AddHostedService<ImageMetadataAnalyserHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseElasticApm(Configuration);
            
            loggerFactory.AddSerilog();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            var log = loggerFactory.CreateLogger<Startup>();
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            log.LogInformation($"Application started at {string.Join(", ", serverAddressesFeature.Addresses)}");

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

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "runtimes");
                    path = Path.Combine(path, osPath, "native");
                    path = Path.Combine(path, "magic.mgc");

                    MimeGuesser.MagicFilePath = path;
                }
            }
            catch (ArgumentNullException)
            {
                log.LogDebug("Docker environment not set.");
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
