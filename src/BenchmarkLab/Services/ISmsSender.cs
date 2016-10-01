using System.Threading.Tasks;

namespace MeasureThat.Net.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
