using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Hangfire;
using log4net;
using Loly.Agent.Analysers;
using Loly.Agent.Discoveries;
using Loly.Agent.Discovery;
using Loly.Agent.Kafka;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

            services.AddTransient<IDiscoveryService, DiscoveryService>();

            services.AddOptions();
            services.Configure<KafkaSettings>(Configuration.GetSection("Kafka"));
            services.AddTransient<IKafkaConfigProducer, KafkaConfigProvider>();
            services.AddSingleton<IKafkaProducerHostedService, KafkaProducerHostedService>();
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

            loggerFactory.AddLog4Net();
            app.UseHttpsRedirection();
            app.UseMvc();

            var log = LogManager.GetLogger(typeof(Program));
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            log.InfoFormat("Application started at {0}", string.Join(", ", serverAddressesFeature.Addresses));
        }
    }
}