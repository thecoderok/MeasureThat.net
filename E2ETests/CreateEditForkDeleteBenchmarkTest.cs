using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETests
{
    [TestClass]
    public class CreateEditForkDeleteBenchmarkTest : BenchmarkLabBaseTest
    {
        // TODO: how to avoid relative path here? 
        const string TEST_CONFIG_FILE = "../../../TestConfig.ini";

        private struct Credentials
        {
            public string Username
            {
                get; set;
            }
            public string Password
            {
                get; set;
            }
        }

        private Dictionary<string, string> ReadCredentials(string filePath)
        {
            var credentials = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("[") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    credentials[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return credentials;
        }

        private Credentials GetCredentials(string filePath)
        {
            var credentialsDict = ReadCredentials(filePath);
            var username = credentialsDict["username"];
            var password = credentialsDict["password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Test credentials are not set in TestConfig.ini.");
            }

            return new Credentials { Username = username, Password = password };
        }

        [TestMethod]
        public async Task TestAccountLogin()
        {
            var credentials = GetCredentials(TEST_CONFIG_FILE);

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

            var credentials = GetCredentials(TEST_CONFIG_FILE);
            var loginLink = Page.Locator("a[href='/Account/Login']:has-text('Log in')");

            await loginLink.ClickAsync();

            await LoginAsync(credentials);

            var userElement = Page.Locator($"a[title='Manage'][href='/Manage']:has-text('Hello {credentials.Username}!')");
            await Expect(userElement).ToBeVisibleAsync();

            // Click Create new test
            var createBenchmarkLink = Page.Locator("div.jumbotron a.btn.btn-primary[href='/Benchmarks/Add']");
            await createBenchmarkLink.ClickAsync();


            const string existing_name = "Deferred & Regular & Async hybrid test";
            await Page.FillAsync("#BenchmarkName", existing_name);
            await Page.FillAsync("#Description", "This is a benchmark that was created by the automated e2e test. To be deleted later after the test. #e2e");

            var alertText = "";
            bool alertShown = false;
            Page.Dialog += async (_, dialog) =>
            {
                // Check if the dialog is an alert
                if (dialog.Type == DialogType.Alert)
                {
                    alertShown = true;
                    alertText = dialog.Message;
                    await dialog.AcceptAsync();
                }
            };


            var validateBenchmarkButton = Page.Locator("a.btn.btn-default[data-role='test-benchmark']");
            await validateBenchmarkButton.ClickAsync();
            Assert.IsTrue(alertShown, "The alert was not shown as expected.");
            
            var expectedSubstrings = new[]
            {
                "Benchmark failed during validation",
                "Benchmark is not valid",
                "At least two test cases are required",
                "Benchmark with such name already exists"
            };

            foreach (var substring in expectedSubstrings)
            {
                StringAssert.Contains(alertText, substring, $"The alert text does not contain the expected substring: {substring}.");
            }
            alertShown = false;
            alertText = "";

            var benchmarkName = $"e2e tests {Guid.NewGuid()}";

        }

        private async Task LoginAsync(Credentials credentials)
        {
            await Page.FillAsync("#Email", credentials.Username);
            await Page.FillAsync("#Password", credentials.Password);
            await Page.ClickAsync("button[type='submit'].btn.btn-default");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));
        }
    }
}
