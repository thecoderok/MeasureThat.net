using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class BenchmarkIndex : BenchmarkLabBaseTest
    {
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
        }

        [TestMethod]
        public async Task TestPaginationOnBenchmarkTable()
        {
            await Page.GotoAsync("/Benchmarks");

            // Validate that go back pagination link is disabled since we're on the first page
            var firstPageLink = Page.Locator("li.disabled > a", new PageLocatorOptions { HasTextString = "«" });
            await Expect(firstPageLink).ToBeVisibleAsync();

            // Click on the link with text "Next"
            var nextLink = Page.Locator("[data-test-purpose='pagination']", new PageLocatorOptions { HasTextString = "Next" });
            await nextLink.ClickAsync();

            // Validate that the URL is now "/Benchmarks?page=1"
            await Expect(Page).ToHaveURLAsync(new Regex("/Benchmarks\\?page=1"));

            await Expect(Page.Locator("table[data-test-id='latest-benchmarks']")).ToBeVisibleAsync();

            // Go to the last page
            var lastPageLink = Page.Locator("li > a", new PageLocatorOptions { HasTextString = "»" });
            await Expect(lastPageLink).ToBeVisibleAsync();
            await lastPageLink.ClickAsync();

            await Expect(Page.Locator("table[data-test-id='latest-benchmarks']")).ToBeVisibleAsync();

            // GO to the first page
            await Page.Locator("li > a", new PageLocatorOptions { HasTextString = "«" }).ClickAsync();
            await Expect(Page.Locator("table[data-test-id='latest-benchmarks']")).ToBeVisibleAsync();

            var tableElement = Page.Locator("table[data-test-id='latest-benchmarks']");
            await tableElement.Locator("a").First.ClickAsync();
        }

        [TestMethod]
        public async Task TestNavigationFromPage()
        {
            await Page.GotoAsync("/Benchmarks");

            var tableElement = Page.Locator("table[data-test-id='latest-benchmarks']");
            await tableElement.Locator("a").First.ClickAsync();
            await Expect(Page).ToHaveTitleAsync(new Regex("Benchmark: .* - MeasureThat.net"));
        }

        [TestMethod]
        public async Task TestCreateBenchmark()
        {
            await Page.GotoAsync("/Benchmarks");

            // Locate the button and click it
            var addButton = Page.Locator("a.btn.btn-primary.btn-lg[href='/Benchmarks/Add']");
            await addButton.ClickAsync();

            // Validate that the navigation to the Add Benchmark page was successful
            await Expect(Page).ToHaveURLAsync("/Benchmarks/Add");
            await Expect(Page).ToHaveTitleAsync("Create a benchmark - MeasureThat.net");
        }
    }
}
