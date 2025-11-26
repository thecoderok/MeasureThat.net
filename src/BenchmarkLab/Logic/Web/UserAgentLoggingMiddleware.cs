using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MeasureThat.Net.Logic.Web
{
    public class UserAgentLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserAgentLoggingMiddleware> _logger;

        public UserAgentLoggingMiddleware(RequestDelegate next, ILogger<UserAgentLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            if (!string.IsNullOrEmpty(userAgent))
            {
                _logger.LogInformation("UserAgent: {UserAgent} | IP: {IP} | Path: {Path} | Method: {Method}",
                    userAgent,
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    context.Request.Method);
            }

            await _next(context);
        }
    }
}
