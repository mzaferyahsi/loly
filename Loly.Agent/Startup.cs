using System;
using System.IO;
using System.Runtime.InteropServices;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.NetCoreAll;
using Hangfire;
using HeyRed.Mime;
using Loly.Agent.Analysers;
using Loly.Agent.Configuration;
using Loly.Agent.Discoveries;
using Loly.Agent.Discovery;
using Loly.Analysers;
using Loly.Kafka.Config;
using Loly.Kafka.Consumer;
using Loly.Kafka.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

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

            loggerConfiguration
                .WriteTo.Console(new ElasticsearchJsonFormatter()); 
            Log.Logger = loggerConfiguration.CreateLogger();
            
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
                .UseSerilogLogProvider());

            services.AddOptions();

            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.Configure<ElasticsearchConfiguration>(Configuration.GetSection("Elasticsearch"));
            services.Configure<LolyFeatureConfiguration>(Configuration.GetSection("Features"));
            services.AddTransient<IConfigProducer, ConfigProvider>();
//            services.AddTransient<IProducerHostedService, ProducerService>();
            services.AddSingleton<IConsumerProvider, ConsumerProvider>();
            services.AddSingleton<LolyFeatureManager>();
            services.AddSingleton<IDiscoveryService, DiscoveryService>();
            services.AddSingleton<FileAnalyser>();
            services.AddSingleton<FileHashAnalyser>();

            services.AddHostedService<FileAnalyserHostedService>();
            services.AddHostedService<FileHashAnalyserHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseElasticApm(Configuration);

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            loggerFactory.AddSerilog();
//            loggerFactory.AddLog4Net("Configs/log4net.config");
            app.UseHttpsRedirection();
            app.UseMvc();

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

                    var path = Path.Join(Directory.GetCurrentDirectory(), "runtimes");
                    path = Path.Join(path, osPath, "native");
                    path = Path.Join(path, "magic.mgc");

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