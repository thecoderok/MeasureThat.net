using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class SmokeTest : BenchmarkLabBaseTest
    {
        [TestMethod]
        public async Task TestHomepage()
        {
            await Page.GotoAsync("/");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));

            // Validate that the page has a link with text "Create a benchmark"
            var createBenchmarkLink = Page.Locator("a", new PageLocatorOptions { HasTextString = "Create a benchmark" });
            await Expect(createBenchmarkLink).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestPublicTools()
        {
            await Page.GotoAsync("/Tools");
            await Expect(Page).ToHaveTitleAsync(new Regex("Free online tools - MeasureThat.net"));

            var headerElement = Page.Locator("div.page-header > h1", new PageLocatorOptions { HasTextString = "Free online tools" });
            await Expect(headerElement).ToBeVisibleAsync();

            var toolsLinks = new[]
            {
                "/Tools/JSONBeautify",
                "/Tools/JSONMinify",
                "/Tools/JavaScriptBeautify",
                "/Tools/HTMLBeautify",
                "/Tools/CSSBeautify",
                "/Tools/FormatSQL",
                "/Tools/WhoisLookup",
                "/Tools/GetIPAddressesByHostName",
                "/Tools/GetHostsByIPAddress",
                "/Tools/URLEncode",
                "/Tools/URLDecode",
                "/Tools/Base64Encode",
                "/Tools/Base64Decode"
            };

            foreach (var link in toolsLinks)
            {
                var locator = Page.Locator($"a[href='{link}']");
                await Expect(locator).ToBeVisibleAsync();
            }
        }

        [TestMethod]
        public async Task MainNavigationLinks()
        {
            await Page.GotoAsync("/");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));

            // TODO: Test all navbar links
        }
    }
}
