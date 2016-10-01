using Microsoft.AspNetCore.Authorization;

namespace MeasureThat.Net.Logic.Web
{
    public class ConfigurableAuthorizationRequirement : IAuthorizationRequirement
    {
        public readonly bool AllowGuestsToCreateBenchmarks;

        public ConfigurableAuthorizationRequirement(bool allowGuestsToCreateBenchmarks)
        {
            this.AllowGuestsToCreateBenchmarks = allowGuestsToCreateBenchmarks;
        }
    }
}
