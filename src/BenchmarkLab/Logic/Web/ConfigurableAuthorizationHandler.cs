using System.Threading.Tasks;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MeasureThat.Net.Logic.Web
{
    /// <summary>
    /// Class to replace unconditional authorization attribute
    /// Will allow to disable authorize attribute from config
    /// </summary>
    public class ConfigurableAuthorizationHandler : AuthorizationHandler<ConfigurableAuthorizationRequirement>
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public ConfigurableAuthorizationHandler(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ConfigurableAuthorizationRequirement requirement)
        {
            if (requirement.AllowGuestsToCreateBenchmarks)
            {
                // No further checks need, guests can proceed
                context.Succeed(requirement);
            }
            else
            {
                // Need to make sure that user authenticated
                if (!signInManager.IsSignedIn(context.User))
                {
                    // Reject, not signed in
                    context.Fail();
                }
            }

            return Task.FromResult(0);
        }
    }
}
