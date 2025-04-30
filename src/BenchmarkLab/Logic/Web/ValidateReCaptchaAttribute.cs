using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MeasureThat.Net.Logic.Web
{
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        public const string ReCaptchaModelErrorKey = "ReCaptcha";
        private const string RecaptchaResponseTokenKey = "g-recaptcha-response";
        private const string ApiVerificationEndpoint = "https://www.google.com/recaptcha/api/siteverify";
        private readonly IConfiguration m_configuration;
        private readonly Lazy<string> m_reCaptchaSecret;
        private readonly bool reCaptchaValidationEnabled = true;

        public ValidateReCaptchaAttribute(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.m_configuration = configuration;
            this.m_reCaptchaSecret = new Lazy<string>(() => m_configuration["ReCaptcha:Secret"]);
            this.reCaptchaValidationEnabled = bool.Parse(m_configuration["ReCaptchaEnabled"]);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (this.reCaptchaValidationEnabled)
            {
                await DoReCaptchaValidation(context).ConfigureAwait(false);
            }

            await base.OnActionExecutionAsync(context, next);
        }

        private async Task DoReCaptchaValidation(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.HasFormContentType)
            {
                // Get request? 
                AddModelError(context, "No reCaptcha token found");
                return;
            }

            string token = context.HttpContext.Request.Form[RecaptchaResponseTokenKey];

            if (string.IsNullOrWhiteSpace(token))
            {
                AddModelError(context, "No reCaptcha token found");
            }
            else
            {
                await ValidateRecaptcha(context, token).ConfigureAwait(false);
            }
        }

        private static void AddModelError(ActionExecutingContext context, string error)
        {
            context.ModelState.AddModelError(ReCaptchaModelErrorKey, error);
        }

        private async Task ValidateRecaptcha(ActionExecutingContext context, string token)
        {
            using (var webClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("secret", this.m_reCaptchaSecret.Value),
                        new KeyValuePair<string, string>("response", token)
                    });
                HttpResponseMessage response = await webClient.PostAsync(ApiVerificationEndpoint, content);
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(json);
                if (reCaptchaResponse == null)
                {
                    // TODO: logging
                    AddModelError(context, "Unable to read response from reCaptcha server");
                }
                else if (!reCaptchaResponse.success)
                {
                    AddModelError(context, "Invalid reCaptcha");
                }
            }
        }
    }

    public class ReCaptchaResponse
    {
        public bool success
        {
            get; set;
        }
        public string challenge_ts
        {
            get; set;
        }
        public string hostname
        {
            get; set;
        }
        public string[] errorcodes
        {
            get; set;
        }
    }

}
