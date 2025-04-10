using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace MeasureThat.Net.Services
{
    public class SendGridAuthMessageSender : IEmailSender, ISmsSender
    {
        public SendGridAuthMessageSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options
        {
            get;
        } 

        // https://github.com/sendgrid/sendgrid-csharp
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Options.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Options.SenderEmail, Options.SenderName);
            var to = new EmailAddress(email);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Unable to send an email: {response.StatusCode} - {responseBody}");
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
