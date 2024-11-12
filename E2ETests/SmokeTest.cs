using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class ExampleTest : PageTest
    {
        [TestMethod]
        public async Task TestBasicNavigation()
        {
            await Page.GotoAsync("/");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));

            // Validate that the page has a link with text "Create a benchmark"
            var createBenchmarkLink = Page.Locator("a", new PageLocatorOptions { HasTextString = "Create a benchmark" });
            await Expect(createBenchmarkLink).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestLatestBenchmarks()
        {
            await Page.GotoAsync("/Benchmarks");
            await Expect(Page).ToHaveTitleAsync(new Regex("Latest benchmarks - MeasureThat.net"));

            // Validate that the page contains <ul class="pager"> element
            var pagerElement = Page.Locator("ul.pager");
            await Expect(pagerElement).ToBeVisibleAsync();

            // Validate that the page contains <table> with data-test-id="latest-benchmarks"
            var tableElement = Page.Locator("table[data-test-id='latest-benchmarks']");
            await Expect(tableElement).ToBeVisibleAsync();

            var addBenchmarkLink = Page.Locator("a[href='/Benchmarks/Add']");
            await Expect(addBenchmarkLink).ToHaveCountAsync(2);

            // Validate that go back pagination link is disabled since we're on the first page
            var addBenchmarkLinkInDisabledLi = Page.Locator("li.disabled > a", new PageLocatorOptions { HasTextString = "«" });
            await Expect(addBenchmarkLinkInDisabledLi).ToBeVisibleAsync();

            // Click on the link with text "Next"
            var nextLink = Page.Locator("a", new PageLocatorOptions { HasTextString = "Next" });
            await nextLink.ClickAsync();

            // Validate that the URL is now "/Benchmarks?page=1"
            await Expect(Page).ToHaveURLAsync(new Regex("/Benchmarks\\?page=1"));
        }

        public override BrowserNewContextOptions ContextOptions()
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
