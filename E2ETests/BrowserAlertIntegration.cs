using Microsoft.Playwright;

namespace E2ETests
{
    internal class BrowserAlertIntegration
    {
        public string alertText { get; private set; }
        public bool alertShown
        {
            get; private set;
        }

        public BrowserAlertIntegration()
        {
            alertShown = false;
            alertText = "";
        }

        public void InstallEvent(IPage page)
        {
            page.Dialog += async (_, dialog) =>
            {
                // Check if the dialog is an alert
                if (dialog.Type == DialogType.Alert)
                {
                    this.alertShown = true;
                    this.alertText = dialog.Message;
                    await dialog.AcceptAsync();
                }
            };
        }

        public async Task WaitForAlertAsync()
        {
            const int timeout = 10000; // Timeout in milliseconds
            const int interval = 100; // Interval in milliseconds
            int elapsed = 0;

            while (!alertShown && elapsed < timeout)
            {
                await Task.Delay(interval);
                elapsed += interval;
            }

            if (!alertShown)
            {
                throw new TimeoutException("The alert was not shown within the expected time.");
            }
        }

        public void ResetState()
        {
            alertShown = false;
            alertText = "";
        }
    }
}
