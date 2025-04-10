using MeasureThat.Net.Data;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using MeasureThat.Net.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net
{
    using BenchmarkLab.Services;
    using MeasureThat.Logic.Web.Sitemap;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        private ILogger m_logger;

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddMemoryCache();

            services.AddDetection();

            services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

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

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            // Add application services.
            services.AddTransient<IEmailSender, MailjetEmailService>();
            services.AddTransient<ISmsSender, MailjetEmailService>();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddScoped<ValidateReCaptchaAttribute>();

            services.AddTransient<SqlServerBenchmarkRepository>();
            services.AddTransient<SqlServerResultsRepository>();
            services.AddTransient<SqlServerSaveThatBlobReporitory>();

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

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
            services.AddScoped<SitemapGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            m_logger = loggerFactory.CreateLogger<Startup>();

            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                m_logger.LogInformation("Running in development mode");
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                m_logger.LogInformation("Running in production mode");
                app.UseExceptionHandler("/Home/Error");

            }

            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
              .AddDefaultSecurePolicy()
            );

            app.UseAuthentication();

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
    }
}
