using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeBackend.Models;
using RecipeBackend.Repositories;

namespace RecipeBackend
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

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
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddFile(pathFormat: $"../Logs/{DateTime.Now.ToFileTime()}.log", minimumLevel: LogLevel.Trace); ;

            app.UseMvc();
        }
    }
}
