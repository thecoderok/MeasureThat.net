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
        public async Task TestSEOFriendlyNavigation()
        {
            // Test navigation just by benchmark ID
            await Page.GotoAsync("/Benchmarks/Show/32502");
            await Expect(Page).ToHaveTitleAsync(new Regex("Benchmark: Async Test - MeasureThat.net"));
            await Expect(Page.Locator("b", new PageLocatorOptions { HasTextString = "Suite status:" })).ToBeVisibleAsync();

            await this.NavigateToMainViaNavbar();

            // Test navigation by benchmark ID and name
            await Page.GotoAsync("/Benchmarks/Show/32502/Some_random_text");
            await Expect(Page).ToHaveTitleAsync(new Regex("Benchmark: Async Test - MeasureThat.net"));
            await Expect(Page.Locator("b", new PageLocatorOptions { HasTextString = "Suite status:" })).ToBeVisibleAsync();

            await this.NavigateToMainViaNavbar();

            // Test navigation by benchmark ID and version + name
            await Page.GotoAsync("/Benchmarks/Show/32502/55/Some_random_text");
            await Expect(Page).ToHaveTitleAsync(new Regex("Benchmark: Async Test - MeasureThat.net"));

            // Check that the element <b>Suite status:</b> exists
            await Expect(Page.Locator("b", new PageLocatorOptions { HasTextString = "Suite status:" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task MainNavigationLinks()
        {
            await Page.GotoAsync("/");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));

            // Click on "Create a benchmark" link
            var createBenchmarkLink = Page.Locator("ul.nav.navbar-nav > li > a[href='/Benchmarks/Add']");
            await Expect(createBenchmarkLink).ToBeVisibleAsync();
            await createBenchmarkLink.ClickAsync();
            await Expect(Page).ToHaveURLAsync("/Benchmarks/Add");

            // Navigate back to the homepage
            await Page.GotoAsync("/");

            // Click on "Free Online Tools" link
            var freeOnlineToolsLink = Page.Locator("ul.nav.navbar-nav > li > a[href='/Tools']");
            await Expect(freeOnlineToolsLink).ToBeVisibleAsync();
            await freeOnlineToolsLink.ClickAsync();
            await Expect(Page).ToHaveURLAsync("/Tools");

            // Navigate back to the homepage
            await Page.GotoAsync("/");

            // Click on "About" link
            var aboutLink = Page.Locator("ul.nav.navbar-nav > li > a[data-toggle='modal'][data-target='#aboutModal']");
            await Expect(aboutLink).ToBeVisibleAsync();
            await aboutLink.ClickAsync();
            await Expect(Page.Locator("#aboutModal")).ToBeVisibleAsync();

            // Close the modal
            var closeModalButton = Page.Locator("#aboutModal .close");
            await closeModalButton.ClickAsync();
            await Expect(Page.Locator("#aboutModal")).ToBeHiddenAsync();

            // Click on "Feedback" link
            var feedbackLink = Page.Locator("ul.nav.navbar-nav > li > a[href='/Home/Discussions']");
            await Expect(feedbackLink).ToBeVisibleAsync();
            await feedbackLink.ClickAsync();
            await Expect(Page).ToHaveURLAsync("/Home/Discussions");
            var headerElement = Page.Locator("div.page-header > h1", new PageLocatorOptions { HasTextString = "Discussions & Feedback" });
            await Expect(headerElement).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestJSONBeautify()
        {
            // Navigate to the JSON Beautify tool
            await Page.GotoAsync("/Tools/JSONBeautify");

            // Paste unformatted JSON into the raw input textarea
            string unformattedJson = "{\"name\":\"John\",\"age\":30,\"city\":\"New York\"}";
            await Page.FillAsync("#raw_input", unformattedJson);

            // Click the format button
            await Page.ClickAsync("#btn_format");

            // Validate that the JSON was formatted in the formatted output textarea
            string formattedJson = await Page.InputValueAsync("#formatted_output_text");
            string expectedFormattedJson = "{\n    \"name\": \"John\",\n    \"age\": 30,\n    \"city\": \"New York\"\n}";
            Assert.AreEqual(expectedFormattedJson, formattedJson);

            // Click the clear all button
            await Page.ClickAsync("#btn_clear_all");

            // Check that both text areas are free of text
            string rawInputValue = await Page.InputValueAsync("#raw_input");
            string formattedOutputValue = await Page.InputValueAsync("#formatted_output_text");
            Assert.AreEqual(string.Empty, rawInputValue);
            Assert.AreEqual(string.Empty, formattedOutputValue);
        }

        [TestMethod]
        public async Task TestSitemapXML()
        {
            var response = await Page.GotoAsync("/sitemap.xml");

            Assert.AreEqual(200, response.Status, "The sitemap.xml page did not return a 200 status code.");

            // Verify that the response content type is XML
            var contentType = response.Headers["content-type"];
            Assert.IsTrue(contentType.Contains("application/xml") || contentType.Contains("text/xml"), "The sitemap.xml page did not return an XML content type.");

            // Optionally, you can further verify the content of the XML
            var xmlContent = await response.TextAsync();
            Assert.IsTrue(xmlContent.Contains("<urlset"), "The sitemap.xml content does not contain the expected <urlset> element.");
        }


        private async Task NavigateToMainViaNavbar()
        {
            var navbarBrandLink = Page.Locator("a.navbar-brand");
            await navbarBrandLink.ClickAsync();

            // Validate that the homepage is loaded
            await Expect(Page).ToHaveURLAsync("/");
        }
    }
}
