using Microsoft.Playwright;

namespace E2ETests
{
    [TestClass]
    public class CreateEditForkDeleteBenchmarkTest : BenchmarkLabBaseTest
    {
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

        [TestMethod]
        public async Task TestAccountLogin()
        {
            var credentials = ReadCredentials("../../../TestConfig.ini");
            var username = credentials["username"];
            var password = credentials["password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Test credentials are not set in TestConfig.ini.");
            }

            // Navigate to the login page
            await Page.GotoAsync("/Account/Login");

            // Fill in the login form
            await Page.FillAsync("#Email", username);
            await Page.FillAsync("#Password", password);
            await Page.ClickAsync("button[type='submit'].btn.btn-default");
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));

            var userElement = Page.Locator($"a[title='Manage'][href='/Manage']:has-text('Hello {username}!')");
            await Expect(userElement).ToBeVisibleAsync();

            var logOffButton = Page.Locator("button[type='submit'].btn.btn-link.navbar-btn.navbar-link");
            await Expect(logOffButton).ToBeVisibleAsync();
        }
    }
}
