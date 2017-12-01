using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeBackend.Models;
using RecipeBackend.Repositories;
using RecipeBackend.Processes;
using System.Threading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RecipeBackend.JwtAuthentication;
using System.Threading.Tasks;

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
                .AddTransient<IAccountRepository, AccountRepository>()
                .AddTransient<Mongod>()
                .AddTransient<MongoImport>()
                .AddTransient<MongoExport>()
                .Configure<Settings>(options =>
                    {
                        options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                        options.Database = Configuration.GetSection("MongoConnection:Database").Value;
                        options.Collection = Configuration.GetSection("MongoConnection:Collection").Value;
                        options.JwtSecret = Configuration.GetSection("Authentication:JwtSecret").Value;
                        options.ApplicationName = Configuration.GetSection("Authentication:ApplicationName").Value;
                        options.MongoPath = Environment.GetEnvironmentVariable("MONGO") ?? "";
                    })
                 .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = Configuration.GetSection("Authentication:ApplicationName").Value,
                            ValidAudience = Configuration.GetSection("Authentication:ApplicationName").Value,
                            IssuerSigningKey = JwtSecurityKey.Create(Configuration.GetSection("Authentication:JwtSecret").Value)
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                _logger.LogInformation("OnAuthenticationFailed: " + context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                _logger.LogInformation("OnTokenValidated: " + context.SecurityToken);
                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Member",
                    policy => policy.RequireClaim("MembershipId"));
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddFile(pathFormat: $"../Logs/{DateTime.Now.ToFileTime()}.log", minimumLevel: LogLevel.Trace); ;

            app.UseAuthentication();
            app.UseMvc();

            _logger = serviceProvider.GetRequiredService(typeof(ILogger<Startup>)) as ILogger<Startup>;

            applicationLifetime.ApplicationStarted.Register(() => OnStart(serviceProvider));
            applicationLifetime.ApplicationStopping.Register(() => OnStopping(serviceProvider));
            applicationLifetime.ApplicationStopped.Register(() => OnStopped(serviceProvider));
        }

        private void OnStart(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Starting Mongo Import");

            _mongoImport = serviceProvider.GetRequiredService(typeof(MongoImport)) as MongoImport;
            Thread.Sleep(1000);
            _mongoImport.WaitForExit();
            _mongoImport.Dispose();

            _logger.LogInformation("Mongo Import Finished");

            _logger.LogInformation("Starting Mongo DB");

            _mongod = serviceProvider.GetRequiredService(typeof(Mongod)) as Mongod;
        }

        private void OnStopping(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Starting Mongo Export");

            _mongoExport = serviceProvider.GetRequiredService(typeof(MongoExport)) as MongoExport;
            Thread.Sleep(1000);
            _mongoExport.WaitForExit();
            _mongoExport.Dispose();

            _logger.LogInformation("Mongo Export Finished");
        }

        private void OnStopped(IServiceProvider serviceProvider)
        {
            Thread.Sleep(1000);
            _mongod.WaitForExit();
            _mongod.Dispose();

            _logger.LogInformation("MongoDB Stopped");
            _logger.LogInformation("Closing Application");
        }
    }
}
