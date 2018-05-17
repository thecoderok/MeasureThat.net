using Microsoft.AspNetCore.Authentication;

namespace MeasureThat.Net.Logic.Web
{
    public class AuthenticationProviderToIconMapper
    {
        public static string GetIconClass(AuthenticationScheme provider)
        {
            if (provider.DisplayName == "Microsoft")
            {
                return "windows";
            }

            return provider.DisplayName.ToLower();
        }
    }
}
