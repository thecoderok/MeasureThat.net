using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BenchmarkLab.Data;
using BenchmarkLab.Models;
using BenchmarkLab.Services;
using BenchmarkLab.Logic.Web;
using BenchmarkLab.Data.Dao;

namespace BenchmarkLab
{
    public class Startup
    {
        private ILogger m_logger;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddScoped<ValidateReCaptchaAttribute>();

            services.AddSingleton<IBenchmarksRepository, MockBenchmarksRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            m_logger = loggerFactory.CreateLogger<Startup>();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                m_logger.LogInformation("Running in development mode");
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                m_logger.LogInformation("Running in production mode");
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            AddExternalAuthentication(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void AddExternalAuthentication(IApplicationBuilder app)
        {
            // https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-google-authentication/
            m_logger.LogInformation("Adding external authentication");
            if (Boolean.Parse(Configuration["UseFacebookAuthentication"]))
            {
                m_logger.LogInformation("Using FB Authentication");
                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    AppId = Configuration["Authentication:Facebook:AppId"],
                    AppSecret = Configuration["Authentication:Facebook:AppSecret"]
                });
            }

            if (Boolean.Parse(Configuration["UseTwitterAuthentication"]))
            {
                m_logger.LogInformation("Using Twitter Authentication");
                app.UseTwitterAuthentication(new TwitterOptions()
                {
                    ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"],
                    ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"],
                });
            }

            if (Boolean.Parse(Configuration["UseGoogleAuthentication"]))
            {
                m_logger.LogInformation("Using Google Authentication");
                app.UseGoogleAuthentication(new GoogleOptions()
                {
                    ClientId = Configuration["Authentication:Google:ClientId"],
                    ClientSecret = Configuration["Authentication:Google:ClientSecret"]
                });
            }

            if (Boolean.Parse(Configuration["UseMicrosoftAuthenticaiton"]))
            {
                m_logger.LogInformation("Using Microsoft Authentication");
                app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
                {
                    ClientId = Configuration["Authentication:Microsoft:ClientId"],
                    ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"],
                });
            }
        }
    }
}
