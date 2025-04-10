namespace E2ETests
{
    [TestClass]
    public class AccountTest : BenchmarkLabBaseTest
    {
        [TestMethod]
        public async Task TestForgotPassword()
        {
            var credentials = TestConfigReader.GetCredentials(TestConfigReader.TEST_CONFIG_FILE);

            // Navigate to login page
            await Page.GotoAsync("/Account/Login");
            await Expect(Page).ToHaveTitleAsync(new Regex("Log in - MeasureThat.net"));

            // Click on the forgot password link
            var forgotPasswordLink = Page.Locator("a[href='/Account/ForgotPassword']");
            await forgotPasswordLink.ClickAsync();

            // Verify we're on the correct page
            await Expect(Page).ToHaveTitleAsync(new Regex("Forgot your password\\? - MeasureThat.net"));

            await Page.FillAsync("#Email", credentials.Username);

            // Submit the form
            await Page.ClickAsync("button[type='submit'].btn.btn-default");

            // Verify we're redirected to the confirmation page
            await Expect(Page).ToHaveTitleAsync(new Regex("Forgot Password Confirmation - MeasureThat.net"));

            // Verify the confirmation message is displayed
            var confirmationMessage = Page.Locator("p:has-text('Please check your email to reset your password.')");
            await Expect(confirmationMessage).ToBeVisibleAsync();


        }

        [TestMethod]
        public async Task TestUserProfileAndMyBenchmarks()
        {
            var credentials = TestConfigReader.GetCredentials(TestConfigReader.TEST_CONFIG_FILE);

            // Navigate to login page
            await Page.GotoAsync("/Account/Login");
            await Expect(Page).ToHaveTitleAsync(new Regex("Log in - MeasureThat.net"));

            // Sign in with credentials
            await Page.FillAsync("#Email", credentials.Username);
            await Page.FillAsync("#Password", credentials.Password);
            await Page.ClickAsync("button[type='submit'].btn.btn-default");

            // Verify successful login
            await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - MeasureThat.net"));
            var userElement = Page.Locator($"a[title='Manage'][href='/Manage']:has-text('Hello {credentials.Username}!')");
            await Expect(userElement).ToBeVisibleAsync();

            // Click on the username to navigate to profile page
            await userElement.ClickAsync();
            await Expect(Page).ToHaveTitleAsync(new Regex("Manage your account - MeasureThat.net"));

            // Verify profile page has user info
            var profileHeader = Page.Locator("h2:has-text('Manage your account')");
            await Expect(profileHeader).ToBeVisibleAsync();

            // Check for My Benchmarks link
            var myBenchmarksLink = Page.Locator("a[href='/Benchmarks/My']:has-text('Your benchmarks')");
            await Expect(myBenchmarksLink).ToBeVisibleAsync();

            // Navigate to My Benchmarks page
            await myBenchmarksLink.ClickAsync();
            await Expect(Page).ToHaveTitleAsync(new Regex("Your benchmarks - MeasureThat.net"));

            // Verify My Benchmarks page elements
            var benchmarksHeader = Page.Locator("div.page-header h1:has-text('Your benchmarks')");
            await Expect(benchmarksHeader).ToBeVisibleAsync();

            // Log out
            var logOffButton = Page.Locator("button[type='submit'].btn.btn-link.navbar-btn.navbar-link");
            await Expect(logOffButton).ToBeVisibleAsync();
            await logOffButton.ClickAsync();

            // Verify logged out state
            var loginLink = Page.Locator("a[href='/Account/Login']:has-text('Log in')");
            await Expect(loginLink).ToBeVisibleAsync();
        }

    }
}
