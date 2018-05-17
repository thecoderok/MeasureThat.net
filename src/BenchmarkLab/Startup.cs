using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeasureThat.Net.Data;
using MeasureThat.Net.Models;
using MeasureThat.Net.Services;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MeasureThat.Net
{
    using System.IO;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.FileProviders;
    // using MySQL.Data.EntityFrameworkCore.Extensions;

    public class Startup
    {
        private ILogger m_logger;

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);

            AddDatabaseContext(services);

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddScoped<ValidateReCaptchaAttribute>();

            services.AddTransient<SqlServerBenchmarkRepository>();
            services.AddTransient<SqlServerResultsRepository>();

            services.AddOptions();
            services.Configure<ResultsConfig>(options => Configuration.GetSection("ResultsConfig").Bind(options));

            services.AddSingleton<StaticSiteConfigProvider>();

            services.AddTransient<UserManager<ApplicationUser>>();
            services.AddTransient<ApplicationDbContext>();

            bool allowGuestUsersToCreateBenchmarks = bool.Parse(Configuration["AllowGuestUsersToCreateBenchmarks"]);

            services.AddTransient<IAuthorizationHandler, ConfigurableAuthorizationHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowGuests",
                    policy => policy.Requirements.Add(
                        new ConfigurableAuthorizationRequirement(
                            allowGuestUsersToCreateBenchmarks)));
            });

            services.Configure<AuthMessageSenderOptions>(Configuration);

            AddExternalAuthentication(services);
        }

        private void AddDatabaseContext(IServiceCollection services)
        {
            string dbTypeString = Configuration["DatabaseType"];
            SupportedDatabase db;
            if (!Enum.TryParse(dbTypeString, out db))
            {
                throw new Exception("Such database type is not supported: " + dbTypeString);
            }

            switch (db)
            {
                case SupportedDatabase.SqlServer:
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                    break;
                case SupportedDatabase.MySql:
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));
                    break;
                case SupportedDatabase.PostgreSQL:
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
                    break;
                default:
                    throw new Exception("There is no initialization section for DB: " + dbTypeString);
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            m_logger = loggerFactory.CreateLogger<Startup>();

            DefaultFilesOptions options = new DefaultFilesOptions();
            //options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("blog_source", "public"))),
                RequestPath = new PathString("/blog")
            });
            
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("blog_source", "public"))),
                RequestPath = "/blog",
                OnPrepareResponse = ctx =>
                {
                    // Requires the following import:
                    // using Microsoft.AspNetCore.Http;
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                },
            }
            );

            //app.UseApplicationInsightsRequestTelemetry();

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

            //app.UseStatusCodePagesWithRedirects("~/errors/code/{0}");

            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
              .AddDefaultSecurePolicy()
            );

            //app.UseApplicationInsightsExceptionTelemetry();

            app.UseAuthentication();

            //AddExternalAuthentication(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "showTest",
                    template: "{controller=Benchmarks}/{action=Show}/{id}/{version}/{name?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void AddExternalAuthentication(IServiceCollection services)
        {
            // https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-google-authentication/
            if (Boolean.Parse(Configuration["UseFacebookAuthentication"]))
            {
                services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                });
            }

            if (Boolean.Parse(Configuration["UseTwitterAuthentication"]))
            {
                services.AddAuthentication()
                .AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                });
            }

            if (Boolean.Parse(Configuration["UseGoogleAuthentication"]))
            {
                services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                });
            }

            if (Boolean.Parse(Configuration["UseMicrosoftAuthenticaiton"]))
            {
                services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                });
            }
        }
    }
}
