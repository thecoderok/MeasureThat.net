namespace MeasureThat.Net.Models
{
    public class GoogleAnalyticsConfig
    {
        public readonly string Identifier;
        public readonly bool Enabled;

        public GoogleAnalyticsConfig(string identifier, bool enabled)
        {
            Identifier = identifier;
            Enabled = enabled;
        }
    }
}
