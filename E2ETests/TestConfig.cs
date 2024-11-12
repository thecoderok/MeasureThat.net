using Microsoft.Playwright;

namespace E2ETests
{
    internal class TestConfig
    {
        public static string Site
        {
            get
            {
                return "http://localhost:5000";
            }
        }

        public static BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions()
            {
                ColorScheme = ColorScheme.Light,
                ViewportSize = new()
                {
                    Width = 1920,
                    Height = 1080
                },
                BaseURL = TestConfig.Site,
                IgnoreHTTPSErrors = true,
            };
        }
    }
}
