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
