using System;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using MeasureThat.Net.Services;
using Microsoft.Extensions.Options;

namespace BenchmarkLab.Services
{
    public class MailjetEmailService : IEmailSender, ISmsSender
    {
        public MailjetEmailService(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options
        {
            get;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MailjetClient client = new MailjetClient(
               Options.MailjetPublicKey,
               Options.MailjetPrivateKey);

            var preparedEmail = new TransactionalEmailBuilder()
                   .WithFrom(new SendContact(Options.SenderEmail))
                   .WithSubject(subject)
                   .WithHtmlPart(message)
                   .WithTo(new SendContact(email))
                   .Build();

            var response = await client.SendTransactionalEmailAsync(preparedEmail);
            if (!response.Messages.Any() ||
                response.Messages.Any(m => m.Status != "success"))
            {
                var errors = string.Join("; ", response.Messages
                    .Where(m => m.Status != "success")
                    .Select(m => m.Errors != null ?
                        string.Join(", ", m.Errors.Select(e => $"{e.ErrorCode}: {e.ErrorMessage}")) :
                        $"Status: {m.Status}"));

                throw new Exception($"Failed to send email: {errors}");
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            throw new System.NotImplementedException();
        }
    }
}
