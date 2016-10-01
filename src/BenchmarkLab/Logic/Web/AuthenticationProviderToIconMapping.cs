using Microsoft.AspNetCore.Http.Authentication;

namespace MeasureThat.Net.Logic.Web
{
    public class AuthenticationProviderToIconMapper
    {
        public static string GetIconClass(AuthenticationDescription provider)
        {
            if (provider.DisplayName == "Microsoft")
            {
                return "windows";
            }

            return provider.DisplayName.ToLower();
        }
    }
}
