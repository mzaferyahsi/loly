using System;
using Elastic.Apm.AspNetCore;
using Loly.Agent.Configuration;
using Loly.Kafka.Config;
using Loly.Kafka.Consumer;
using Loly.Kafka.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace Loly.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    var enumConverter = new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()};
                    options.SerializerSettings.Converters.Add(enumConverter);
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddOptions();
            
            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.AddTransient<IConfigProducer, ConfigProvider>();
            services.AddSingleton<IConsumerProvider, ConsumerProvider>();
            services.Configure<ElasticsearchConfiguration>(Configuration.GetSection("Elasticsearch"));

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}