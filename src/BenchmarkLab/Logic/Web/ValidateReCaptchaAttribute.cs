using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;

namespace BenchmarkLab.Logic.Web
{
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        public const string ReCaptchaModelErrorKey = "ReCaptcha";
        private const string RecaptchaResponseTokenKey = "g-recaptcha-response";
        private const string ApiVerificationEndpoint = "https://www.google.com/recaptcha/api/siteverify";
        private readonly IConfiguration m_configuration;
        private readonly Lazy<string> ReCaptchaSecret;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string token = context.HttpContext.Request.Form[RecaptchaResponseTokenKey];

            if (string.IsNullOrWhiteSpace(token))
            {
                context.ModelState.AddModelError(ReCaptchaModelErrorKey, ReCaptchaValidationErrors.NoReCaptchaTokenFound.ToString());
            }
            else
            {
                /*
                 * var postData = string.Format("&secret={0}&remoteip={1}&response={2}",
                "58738UwyuasAAAAABe7C5s2HDGq3gmEHj2s2dGHGSp",
                userIP,
                Context.Request.Form["g-recaptcha-response"]);
                var postDataAsBytes = Encoding.UTF8.GetBytes(postData);
    
                WebClient webClient = new WebClient();
                webClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                var json = await webClient.UploadStringTaskAsync
                (new System.Uri("https://www.google.com/recaptcha/api/siteverify"),"POST",postData); 
                return JsonConvert.DeserializeObject<CaptchaResponse>(json).Success;
                 */
            }

            base.OnActionExecuting(context);
        }
    }

    public enum ReCaptchaValidationErrors
    {
        None,
        NoReCaptchaTokenFound,
        InvalidReCaptcha
    }
}
