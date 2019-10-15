using System;
using Elastic.Apm.AspNetCore;
using Loly.App.Analysers;
using Loly.App.Db.Services;
using Loly.App.Db.Settings;
using Loly.App.HostedServices;
using Loly.Configuration;
using Loly.Streaming.Config;
using Loly.Streaming.Consumer;
using Loly.Streaming.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Loly.App
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
            
            loggerConfiguration.Enrich.WithMachineName();
            
            var elasticConfig = Configuration.GetSection("Elasticsearch").Get<ElasticsearchConfiguration>();
            if (elasticConfig != null)
            {
                Console.WriteLine("Elasticsearch configuration found.");
                Console.WriteLine($"Elasticsearch Uri is {elasticConfig.Uri}");
                
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticConfig.Uri))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = "loly-app-logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Debug
                });
            }

//            loggerConfiguration
//                .WriteTo.Console(new ElasticsearchJsonFormatter()); 
            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                var enumConverter = new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()};
                options.SerializerSettings.Converters.Add(enumConverter);
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
            
            services.AddOptions();
            
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.AddTransient<IConfigProducer, ConfigProvider>();
            services.AddSingleton<IConsumerProvider, ConsumerProvider>();
            services.Configure<ElasticsearchConfiguration>(Configuration.GetSection("Elasticsearch"));

            services.Configure<LolyDatabaseSettings>(Configuration.GetSection("Db"));
            services.AddSingleton<ILolyDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<LolyDatabaseSettings>>().Value);

            services.AddSingleton<FilesService>();
            services.AddSingleton<DuplicateFilesService>();

            services.AddSingleton<DuplicateFileAnalyser>();
            
            services.AddHostedService<FileInformationHostedService>();
//            services.AddHostedService<FileMetaDataHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseElasticApm(Configuration);
            
            loggerFactory.AddSerilog();

            var log = loggerFactory.CreateLogger<Startup>();
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            log.LogInformation($"Application started at {string.Join(", ", serverAddressesFeature.Addresses)}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}