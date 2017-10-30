using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeBackend.Models;
using RecipeBackend.Repositories;
using RecipeBackend.Processes;
using Microsoft.Extensions.Options;

namespace RecipeBackend
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        private ILogger<Startup> _logger;

        private Mongod _mongod;
        private MongoExport _mongoExport;
        private MongoImport _mongoImport;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services
                .AddTransient<IRecipeRepository, RecipeRepository>()
                .AddTransient<Mongod>()
                .AddTransient<MongoImport>()
                .AddTransient<MongoExport>()
                .Configure<Settings>(options =>
                    {
                        options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                        options.Database = Configuration.GetSection("MongoConnection:Database").Value;
                        options.Collection = Configuration.GetSection("MongoConnection:Collection").Value;
                        options.MongoPath = Environment.GetEnvironmentVariable("MONGO") ?? "";
                    })
                .AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddFile(pathFormat: $"../Logs/{DateTime.Now.ToFileTime()}.log", minimumLevel: LogLevel.Trace); ;

            app.UseMvc();
            
            applicationLifetime.ApplicationStarted.Register(() => OnStart(serviceProvider));
            applicationLifetime.ApplicationStopping.Register(() => OnStopping(serviceProvider));
            applicationLifetime.ApplicationStopped.Register(() => OnStopped(serviceProvider));
        }
        
        private void OnStart(IServiceProvider serviceProvider)
        {
            _mongod = serviceProvider.GetRequiredService(typeof(Mongod)) as Mongod;
            _mongoImport = serviceProvider.GetRequiredService(typeof(MongoImport)) as MongoImport;

            _mongoImport.WaitForExit();
            _mongoImport.Dispose();
        }

        private void OnStopping(IServiceProvider serviceProvider)
        {   
            _mongoExport = serviceProvider.GetRequiredService(typeof(MongoExport)) as MongoExport;
            _mongoExport.WaitForExit();
            _mongoExport.Dispose();
        }

        private void OnStopped(IServiceProvider serviceProvider)
        {
            _mongod.WaitForExit();
            _mongod.Dispose();
        }
    }
}
