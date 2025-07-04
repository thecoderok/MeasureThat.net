﻿using Microsoft.Playwright;

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
            var response = await Page.GotoAsync("/sitemap.xml", new PageGotoOptions { Timeout = 1000000 });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.AreEqual(200, response.Status, "The sitemap.xml page did not return a 200 status code.");

            // Verify that the response content type is XML
            var contentType = response.Headers["content-type"];
            Assert.IsTrue(contentType.Contains("application/xml") || contentType.Contains("text/xml"), "The sitemap.xml page did not return an XML content type.");

            // Optionally, you can further verify the content of the XML
            var xmlContent = await response.TextAsync();
            Assert.IsTrue(xmlContent.Contains("<urlset"), "The sitemap.xml content does not contain the expected <urlset> element.");

            // Parse the XML content using XDocument
            var xmlDoc = System.Xml.Linq.XDocument.Parse(xmlContent);

            // Get the namespace from the document
            var ns = xmlDoc.Root.GetDefaultNamespace();

            // Get all URL elements from the sitemap
            var urlElements = xmlDoc.Descendants(ns + "url");
            var urls = urlElements.Select(element => element.Element(ns + "loc")?.Value).ToList();

            // Define well-known URLs to check
            var wellKnownUrls = new[]
            {
                "https://measurethat.net/",
                "https://measurethat.net/Tools",
                "https://measurethat.net/Benchmarks/Add",
            };
            // Verify that each well-known URL exists in the sitemap
            foreach (var url in wellKnownUrls)
            {
                Assert.IsTrue(urls.Any(u => u == url || u.Contains(url)),
                    $"The sitemap.xml does not contain the expected URL: {url}");
            }
        }

        [TestMethod]
        public async Task TestPreviousResultsPage()
        {
            await Page.GotoAsync("/Benchmarks/Show/32502");
            await Page.ClickAsync("text=Previous results");
            var resultsTable = Page.Locator("table[data-test-id='results-list']");
            await Expect(resultsTable).ToBeVisibleAsync();

            // Verify that the table has more than 0 rows
            var rowCount = await resultsTable.Locator("tbody tr").CountAsync();
            Assert.IsTrue(rowCount > 0, "The results list table has no rows.");

            // Click on a link from any random row
            var randomRow = resultsTable.Locator("tbody tr").Nth(new Random().Next(rowCount));
            var link = randomRow.Locator("a");
            await link.ClickAsync();
            await Expect(Page.Locator("small", new PageLocatorOptions { HasTextString = "Run results for:" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestFaqPage()
        {
            await Page.GotoAsync("/Faq");
            await Expect(Page).ToHaveTitleAsync(new Regex("FAQ - MeasureThat.net"));

            // Validate that the page has a header with text "Frequently Asked Questions"
            var headerElement = Page.Locator("h1", new PageLocatorOptions { HasTextString = "Frequently Asked Questions" });
            await Expect(headerElement).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestVersionHistoryPage()
        {
            await Page.GotoAsync("/VersionHistory");
            await Expect(Page).ToHaveTitleAsync(new Regex("Version History - MeasureThat.net"));

            // Validate that the page has a header with text "Version History"
            var headerElement = Page.Locator("h1", new PageLocatorOptions { HasTextString = "Version History" });
            await Expect(headerElement).ToBeVisibleAsync();

            // Validate that the page contains version history entries
            var versionEntries = Page.Locator(".version-entry");
            var entryCount = await versionEntries.CountAsync();
            Assert.IsTrue(entryCount > 0, "The version history page has no entries.");
        }

        [TestMethod]
        public async Task TestPyodidePage()
        {
            await Page.GotoAsync("/Pyodide");
            await Expect(Page).ToHaveTitleAsync(new Regex("Pyodide benchmarks support - MeasureThat.net"));

            // Validate that the page has a header with text "Pyodide"
            var headerElement = Page.Locator("h1", new PageLocatorOptions { HasTextString = "Pyodide" });
            await Expect(headerElement).ToBeVisibleAsync();

            // Validate that the page contains a description or content related to Pyodide
            var contentElement = Page.Locator(".pyodide-content");
            await Expect(contentElement).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestSubmitButtonDisbledOnAdd()
        {
            await Page.GotoAsync("/");

            // Click on "Create a benchmark" link
            var createBenchmarkLink = Page.Locator("ul.nav.navbar-nav > li > a[href='/Benchmarks/Add']");
            await createBenchmarkLink.ClickAsync();
            await Expect(Page.Locator("#benchmark_submit")).ToBeDisabledAsync();

            await Page.Locator("#BenchmarkName").ClickAsync();

            var guid = Guid.NewGuid();
            await Page.Keyboard.TypeAsync($"Test e2e benchmark {guid}");
            await Page.Keyboard.PressAsync("Tab");

            var testCasesListSection = await Page.QuerySelectorAsync("ul#test-case-list");
            var testCaseList = Page.Locator("ul#test-case-list");

            await testCaseList.Locator(CreateEditForkDeleteBenchmarkTest.TEST_CASE_NAME_SELECTOR).Nth(0).FillAsync("Test case #1");
            var codeEditors = await testCasesListSection.QuerySelectorAllAsync(CreateEditForkDeleteBenchmarkTest.CODE_MIRROR_EDITOR_SELECTOR);
            await codeEditors[0].ClickAsync();
            await Page.Keyboard.TypeAsync("let a = 12345;");


            await testCaseList.Locator(CreateEditForkDeleteBenchmarkTest.TEST_CASE_NAME_SELECTOR).Nth(1).FillAsync("Test case #2");
            await codeEditors[1].ClickAsync();
            await Page.Keyboard.TypeAsync("/*THIS IS A TEST*/");

            var validateBenchmarkButton = Page.Locator("a.btn.btn-default[data-role='test-benchmark']");
            await validateBenchmarkButton.ClickAsync();

            var successMessage = await Page.Locator("#validation_log li:has-text('Success! Validation completed.')").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 120000 });
            Assert.IsTrue(successMessage.Contains("Success! Validation completed."));

            await Expect(Page.Locator("#benchmark_submit")).ToBeEnabledAsync();
        }

        [TestMethod]
        public async Task TestSubmitButtonDisbledOnFork()
        {
            await Page.GotoAsync("/Benchmarks/Show/32635");

            await Page.Locator("i.fa.fa-code-fork").ClickAsync();
            await Expect(Page.Locator("#benchmark_submit")).ToBeDisabledAsync();

            await Page.Locator("#BenchmarkName").ClickAsync();

            var guid = Guid.NewGuid();
            await Page.Keyboard.TypeAsync($" - Forked {guid}");
            await Page.Keyboard.PressAsync("Tab");

            var validateBenchmarkButton = Page.Locator("a.btn.btn-default[data-role='test-benchmark']");
            await validateBenchmarkButton.ClickAsync();

            var successMessage = await Page.Locator("#validation_log li:has-text('Success! Validation completed.')").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 120000 });
            Assert.IsTrue(successMessage.Contains("Success! Validation completed."));

            await Expect(Page.Locator("#benchmark_submit")).ToBeEnabledAsync();
        }


        private async Task NavigateToMainViaNavbar()
        {
            var navbarBrandLink = Page.Locator("a.navbar-brand");
            await navbarBrandLink.ClickAsync();

            // Validate that the homepage is loaded
            await Expect(Page).ToHaveURLAsync("/");
        }

        [TestMethod]
        public async Task TestUnicodeTestCaseNamesInChart()
        {
            // Navigate to benchmark with Unicode test case names (ID: 34509)
            await Page.GotoAsync("/Benchmarks/Show/34509");

            // Wait for chart to be rendered
            await Page.WaitForSelectorAsync("#chart_div svg", new PageWaitForSelectorOptions { Timeout = 30000 });

            // Verify the chart has been created and contains SVG elements
            var chartSvg = Page.Locator("#chart_div svg");
            await Expect(chartSvg).ToBeVisibleAsync();

            // Get all text elements in the chart - SVG text elements, not HTML
            var textElements = Page.Locator("#chart_div svg text");
            var textCount = await textElements.CountAsync();
            Assert.IsTrue(textCount > 0, "No text elements found in chart");

            // Extract all text content from the chart
            var allTexts = new List<string>();
            for (int i = 0; i < textCount; i++)
            {
                // Use TextContentAsync instead of InnerTextAsync for SVG elements
                var text = await textElements.Nth(i).TextContentAsync();
                allTexts.Add(text);
            }

            // Join all texts for easier searching
            var allTextContent = string.Join(" ", allTexts);

            // Check that HTML entities are not present in the rendered text
            Assert.IsFalse(allTextContent.Contains("&#x"), "HTML encoded characters found in chart text");

            // Check that the Unicode characters are present
            bool hasUnicodeCharacters = allTexts.Any(text =>
                text.Contains("线性插值"));

            Assert.IsTrue(hasUnicodeCharacters, "No Unicode characters found in chart text");
        }

        [TestMethod]
        public async Task TestQuotesInTestCaseNamesInChart()
        {
            // Navigate to benchmark with test case names containing double quotes (ID: 34678)
            await Page.GotoAsync("/Benchmarks/Show/34678");

            // Wait for chart to be rendered
            await Page.WaitForSelectorAsync("#chart_div svg", new PageWaitForSelectorOptions { Timeout = 30000 });

            // Verify the chart has been created and contains SVG elements
            var chartSvg = Page.Locator("#chart_div svg");
            await Expect(chartSvg).ToBeVisibleAsync();

            // Get all text elements in the chart - SVG text elements, not HTML
            var textElements = Page.Locator("#chart_div svg text");
            var textCount = await textElements.CountAsync();
            Assert.IsTrue(textCount > 0, "No text elements found in chart");

            // Extract all text content from the chart
            var allTexts = new List<string>();
            for (int i = 0; i < textCount; i++)
            {
                // Use TextContentAsync instead of InnerTextAsync for SVG elements
                var text = await textElements.Nth(i).TextContentAsync();
                allTexts.Add(text);
            }

            // Join all texts for easier searching
            var allTextContent = string.Join(" ", allTexts);

            // Check that the double quotes are present in the chart text
            bool hasQuotes = allTexts.Any(text =>
                text.Contains("fastify deepmerge, with ''all: true''"));

            Assert.IsTrue(hasQuotes, "No double quotes found in chart text");

            // Check that no HTML entities for quotes are present in the rendered text
            Assert.IsFalse(allTextContent.Contains("&quot;"), "HTML encoded quotes found in chart text");
        }



#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
