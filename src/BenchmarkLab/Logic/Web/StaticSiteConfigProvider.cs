using System;
using JetBrains.Annotations;
using MeasureThat.Net.Models;
using Microsoft.Extensions.Configuration;

namespace MeasureThat.Net.Logic.Web
{
    /// <summary>
    /// Read and caches configuration that rarely (or never) changes
    /// And keeps it till application is alive
    /// </summary>
    public class StaticSiteConfigProvider
    {
        private readonly IConfiguration m_configuration;
        //private readonly ILogger m_logger;
        private GoogleAnalyticsConfig googleAnalyticsConfig = null;
        private NewsletterSubscriptionConfig newsletterConfig = null;

        public StaticSiteConfigProvider(
            [NotNull] IConfiguration mConfiguration
            /*[NotNull] ILogger mLogger*/)
        {
            m_configuration = mConfiguration;
            //m_logger = mLogger;
        }

        public GoogleAnalyticsConfig GetGoogleAnalyticsConfig()
        {
            try
            {
                if (googleAnalyticsConfig == null)
                {
                    bool enabled = Boolean.Parse(this.m_configuration["GoogleAnalytics:Enabled"]);
                    string identifier = this.m_configuration["GoogleAnalytics:Identifier"];
                    this.googleAnalyticsConfig = new GoogleAnalyticsConfig(identifier, enabled);
                }
            }
            catch (Exception)
            {
                // We don't want to break website just because we can't read GA config
                //m_logger.LogError("Error when reading Google Analytics config: {0}.", e.Message);
                return null;
            }

            return this.googleAnalyticsConfig;
        }

        public NewsletterSubscriptionConfig GetNewsletterSignupConfig()
        {
            try
            {
                if (newsletterConfig == null)
                {
                    bool enabled = Boolean.Parse(this.m_configuration["NewsletterConfig:Enabled"]);
                    this.newsletterConfig = new NewsletterSubscriptionConfig(enabled);
                }
            }
            catch (Exception)
            {
                return new NewsletterSubscriptionConfig(false);
            }

            return this.newsletterConfig;
        }
    }
}
