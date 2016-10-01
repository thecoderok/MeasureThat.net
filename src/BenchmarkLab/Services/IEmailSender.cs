using System.Threading.Tasks;

namespace MeasureThat.Net.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
