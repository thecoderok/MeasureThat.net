using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class CreateEditForkDeleteBenchmarkTest : BenchmarkLabBaseTest
    {
        public const string TEST_CASE_NAME_SELECTOR = "input[type='text'][data-role='testCaseName']";
        public const string DEFEFFED_SELECTOR = "input[type='checkbox'][data-role='Deferred']";
        public const string CODE_MIRROR_EDITOR_SELECTOR = "div.CodeMirror-code pre.CodeMirror-line";
        public const string SCRIPT_PREP = @"
function wait(ms) {
    return new Promise(res => setTimeout(() => { res(ms); }, ms));
}
function factorializeRecursive(num) {
  if (num < 0) 
        return -1;
  else if (num == 0) 
      return 1;
  else {
      return (num * factorializeRecursive(num - 1));
  }
}";


        [TestMethod]
        public async Task TestAccountLogin()
        {
            var credentials = TestConfigReader.GetCredentials(TestConfigReader.TEST_CONFIG_FILE);

            // Navigate to the login page
            await Page.GotoAsync("/Account/Login");

            await LoginAsync(credentials);

            var userElement = Page.Locator($"a[title='Manage'][href='/Manage']:has-text('Hello {credentials.Username}!')");
            await Expect(userElement).ToBeVisibleAsync();

            var logOffButton = Page.Locator("button[type='submit'].btn.btn-link.navbar-btn.navbar-link");
            await Expect(logOffButton).ToBeVisibleAsync();
            await logOffButton.ClickAsync();
            await Expect(userElement).Not.ToBeVisibleAsync();

            var registerLink = Page.Locator("a[href='/Account/Register']:has-text('Register')");
            await Expect(registerLink).ToBeVisibleAsync();

            var loginLink = Page.Locator("a[href='/Account/Login']:has-text('Log in')");
            await Expect(loginLink).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task TestCreateEditForkDeleteBenchmark()
        {
            // Login via main page/modal dialog
            await Page.GotoAsync("/");

            var credentials = TestConfigReader.GetCredentials(TestConfigReader.TEST_CONFIG_FILE);
            var loginLink = Page.Locator("a[href='/Account/Login']:has-text('Log in')");

            await loginLink.ClickAsync();

            await LoginAsync(credentials);

            var userElement = Page.Locator($"a[title='Manage'][href='/Manage']:has-text('Hello {credentials.Username}!')");
            await Expect(userElement).ToBeVisibleAsync();

            // Click Create new test
            var createBenchmarkLink = Page.Locator("div.jumbotron a.btn.btn-primary[href='/Benchmarks/Add']");
            await createBenchmarkLink.ClickAsync();

            await Expect(Page.Locator("#benchmark_submit")).ToBeEnabledAsync();

            const string existing_name = "Deferred & Regular & Async hybrid test";
            await Page.FillAsync("#BenchmarkName", existing_name);
            await Page.FillAsync("#Description", "This is a benchmark that was created by the automated e2e test. To be deleted later after the test. #e2e");


            var browserAlertIntegration = new BrowserAlertIntegration();
            browserAlertIntegration.InstallEvent(Page);
            var validateBenchmarkButton = Page.Locator("a.btn.btn-default[data-role='test-benchmark']");

            await ValidateTestCaseCount(2);
            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            var expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "Benchmark code must not be empty.",
                "Test name must not be empty",
                "Benchmark with such name already exists."
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();

            var deleteButtons = Page.Locator("button[data-action='delete-test']");
            await deleteButtons.Nth(1).ClickAsync();
            await ValidateTestCaseCount(1);
            await deleteButtons.Nth(0).ClickAsync();
            await ValidateTestCaseCount(0);

            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "At least two test cases are required",
                "Benchmark with such name already exists"
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();
            
            var guid = Guid.NewGuid();
            var benchmarkName = $"e2e tests {guid}";
            await Page.FillAsync("#BenchmarkName", benchmarkName);

            var codeMirrorDiv = await Page.QuerySelectorAsync("div[data-test-id='HtmlPreparationCodeFormGroup']");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var codeMirrorLine = await codeMirrorDiv.QuerySelectorAsync(CODE_MIRROR_EDITOR_SELECTOR);
            await codeMirrorLine.ClickAsync();
            await Page.Keyboard.TypeAsync("<div>html test prep code</div>");

            codeMirrorDiv = await Page.QuerySelectorAsync("div[data-test-id='ScriptPreparationCodeFormGroup']");
            codeMirrorLine = await codeMirrorDiv.QuerySelectorAsync(CODE_MIRROR_EDITOR_SELECTOR);
            await codeMirrorLine.ClickAsync();
            await Page.Keyboard.PressAsync("Control+A"); // Select all text
            await Page.Keyboard.PressAsync("Delete"); // Delete the selected text
            await Page.Keyboard.TypeAsync(SCRIPT_PREP);

            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "At least two test cases are required",
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();

            await ValidateAddRemoveTestCase(browserAlertIntegration);

            var testCaseList = Page.Locator("ul#test-case-list");

            await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).Nth(0).FillAsync("Deferred wait 50");
            await testCaseList.Locator(DEFEFFED_SELECTOR).Nth(0).CheckAsync();

            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "Benchmark code must not be empty.",
                "Test name must not be empty",
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();

            var testCasesListSection = await Page.QuerySelectorAsync("ul#test-case-list");
            var codeEditors = await testCasesListSection.QuerySelectorAllAsync(CODE_MIRROR_EDITOR_SELECTOR);
            await codeEditors[0].ClickAsync();
            await Page.Keyboard.TypeAsync("setTimeout(function () { deferred.resolve() }, 50);");

            await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).Nth(1).FillAsync("Deferred wait 500");
            await testCaseList.Locator(DEFEFFED_SELECTOR).Nth(1).CheckAsync();
            await codeEditors[1].ClickAsync();
            await Page.Keyboard.TypeAsync("setTimeout(function () { deferred.resolve() }, 500);");

            await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).Nth(2).FillAsync("Deferred wait 100");
            await testCaseList.Locator(DEFEFFED_SELECTOR).Nth(2).CheckAsync();
            await codeEditors[2].ClickAsync();
            await Page.Keyboard.TypeAsync("await wait(100);\ndeferred.resolve();");

            await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).Nth(3).FillAsync("Regular/Sync Test, 5000!");
            await codeEditors[3].ClickAsync();
            await Page.Keyboard.TypeAsync("var r = factorializeRecursive(5000);");

            await validateBenchmarkButton.ClickAsync();

            await Page.WaitForSelectorAsync("#validation_log li");

            var successMessage = await Page.Locator("#validation_log li:has-text('Success! Validation completed.')").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 120000 });
            Assert.IsTrue(successMessage.Contains("Success! Validation completed."));

            await Page.Locator("#benchmark_submit").ClickAsync();

            await Expect(Page).ToHaveTitleAsync(new Regex($"Benchmark: e2e tests {guid} - MeasureThat.net"));
            await ValidateBenchmarkCanRun(Page.Url, false, false);

            await Page.Locator("a:has-text('Edit')").ClickAsync();

            await Expect(Page.Locator("#benchmark_submit")).ToBeEnabledAsync();

            testCasesListSection = await Page.QuerySelectorAsync("ul#test-case-list");
            codeEditors = await testCasesListSection.QuerySelectorAllAsync(CODE_MIRROR_EDITOR_SELECTOR);
            await codeEditors[4].ClickAsync();
            await Page.Keyboard.TypeAsync("/*added comment*/");

            var addButton = Page.Locator("input.btn.btn-default[data-action='new-test'][value='Add test case']");
            await addButton.ClickAsync();

            await ValidateTestCaseCount(5);
            testCasesListSection = await Page.QuerySelectorAsync("ul#test-case-list");
            codeEditors = await testCasesListSection.QuerySelectorAllAsync(CODE_MIRROR_EDITOR_SELECTOR);

            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "Benchmark code must not be empty",
                "Test name must not be empty",
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();

            await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).Nth(4).FillAsync("Factorize again");
            await testCaseList.Locator(DEFEFFED_SELECTOR).Nth(4).CheckAsync();
            await codeEditors[5].ClickAsync();
            await Page.Keyboard.TypeAsync("var r = factorializeRecursive(2500);");

            await Page.Locator("#benchmark_submit").ClickAsync();
            await Page.Locator("span:has-text('var r = factorializeRecursive(5000);/*added comment*/')").WaitForAsync();

            var elements = await Page.Locator("span:has-text('[Async/Deferred]')").CountAsync();
            Assert.AreEqual(4, elements, "The number of elements with the specified text is not equal to 4.");

            // Edit benchmark, unchecked the deferred checkbox for the 5th test case
            await Page.Locator("a:has-text('Edit')").ClickAsync();
            await Expect(Page.Locator("#benchmark_submit")).ToBeEnabledAsync();

            testCaseList = Page.Locator("ul#test-case-list");
            await testCaseList.Locator(DEFEFFED_SELECTOR).Nth(4).UncheckAsync();

            await validateBenchmarkButton.ClickAsync();
            await Page.WaitForSelectorAsync("#validation_log li");

            successMessage = await Page.Locator("#validation_log li:has-text('Success! Validation completed.')").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 120000 });
            Assert.IsTrue(successMessage.Contains("Success! Validation completed."));

            await Page.Locator("#benchmark_submit").ClickAsync();

            await Expect(Page).ToHaveTitleAsync(new Regex($"Benchmark: e2e tests {guid} - MeasureThat.net"));
            await ValidateBenchmarkCanRun(Page.Url, false, false);

            // Test fork the benchmark.
            await Page.ClickAsync("button#fork-btn.btn.btn-default");
            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "Benchmark with such name already exists.",
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();
            await Page.Locator("#BenchmarkName").ClickAsync();
            await Page.Keyboard.TypeAsync(" - Forked");

            await validateBenchmarkButton.ClickAsync();
            await Page.WaitForSelectorAsync("#validation_log li");

            successMessage = await Page.Locator("#validation_log li:has-text('Success! Validation completed.')").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 120000 });
            Assert.IsTrue(successMessage.Contains("Success! Validation completed."));

            await Page.Locator("#benchmark_submit").ClickAsync();

            await Expect(Page).ToHaveTitleAsync(new Regex($"Benchmark: e2e tests {guid} - Forked - MeasureThat.net"));
            await ValidateBenchmarkCanRun(Page.Url, false, false);

            await Page.GotoAsync("/Benchmarks/My");
            await Expect(Page).ToHaveTitleAsync(new Regex($"Your benchmarks - MeasureThat.net"));

            var deleteBenchmarkButtons = Page.Locator("a.btn.btn-default[data-role='delete-benchmark']");
            var buttonCount = await deleteBenchmarkButtons.CountAsync();
            Assert.IsTrue(buttonCount >= 2, "There are less than 2 delete buttons on the page.");

            // Click each delete button until none are left
            while (await deleteBenchmarkButtons.CountAsync() > 0)
            {
                await deleteBenchmarkButtons.First.ClickAsync();
                await Page.ClickAsync("#perform-delete");
            }

            // Verify that no delete buttons are left
            buttonCount = await deleteBenchmarkButtons.CountAsync();
            Assert.AreEqual(0, buttonCount, "There are still delete buttons left on the page.");
        }

        private async Task ValidateAddRemoveTestCase(BrowserAlertIntegration browserAlertIntegration)
        {
            var validateBenchmarkButton = Page.Locator("a.btn.btn-default[data-role='test-benchmark']");

            // Click add test case and validate that test case block was added
            var addButton = Page.Locator("input.btn.btn-default[data-action='new-test'][value='Add test case']");
            await addButton.ClickAsync();

            await ValidateTestCaseCount(1);

            await addButton.ClickAsync();

            await ValidateTestCaseCount(2);

            await addButton.ClickAsync();

            await ValidateTestCaseCount(3);

            await validateBenchmarkButton.ClickAsync();
            await browserAlertIntegration.WaitForAlertAsync();

            var expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "Benchmark code must not be empty",
                "Test name must not be empty",
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(browserAlertIntegration.alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            browserAlertIntegration.ResetState();

            var deleteButtons = Page.Locator("button[data-action='delete-test']");
            await deleteButtons.Nth(1).ClickAsync();
            await ValidateTestCaseCount(2);
            await deleteButtons.Nth(1).ClickAsync();
            await ValidateTestCaseCount(1);
            await deleteButtons.Nth(0).ClickAsync();
            await ValidateTestCaseCount(0);

            await addButton.ClickAsync();
            await addButton.ClickAsync();
            await addButton.ClickAsync();
            await addButton.ClickAsync();
            await ValidateTestCaseCount(4);
        }

        private async Task ValidateTestCaseCount(int expectedCount)
        {
            await Expect(Page).ToHaveTitleAsync(new Regex($".* a benchmark - MeasureThat.net"));

            var testCaseList = Page.Locator("ul#test-case-list");            

            var count = await testCaseList.Locator(TEST_CASE_NAME_SELECTOR).CountAsync();
            Assert.AreEqual(expectedCount, count, $"There should be exactly {expectedCount} input element with data-role='testCaseName'.");

            count = await testCaseList.Locator(DEFEFFED_SELECTOR).CountAsync();
            Assert.AreEqual(expectedCount, count, $"There should be exactly {expectedCount} input element with data-role='Deferred'.");

            count = await testCaseList.Locator(".CodeMirror.cm-s-default").CountAsync();
            Assert.AreEqual(expectedCount, count, $"There should be exactly {expectedCount} input element with data-role='Deferred'.");
        }


        private async Task LoginAsync(TestCredentials credentials)
        {
            await Page.FillAsync("#Email", credentials.Username);
            await Page.FillAsync("#Password", credentials.Password);
            await Page.ClickAsync("button[type='submit'].btn.btn-default");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
