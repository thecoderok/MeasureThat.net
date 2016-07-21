using BenchmarkLab.Logic.Text.Unidecode;
using JetBrains.Annotations;
using System.Text.RegularExpressions;

namespace BenchmarkLab.Logic.Web
{
    public class SeoFriendlyStringConverter
    {
        private static readonly Regex InvalidCharsRegex = new Regex(@"[^a-z0-9\s-]", RegexOptions.Compiled);
        private static readonly Regex RemoveMultipleSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex SpaceRegex = new Regex(@"\s", RegexOptions.Compiled);

        // http://www.jerriepelser.com/blog/generate-seo-friendly-urls-aspnet-mvc
        public static string Convert([NotNull] string str)
        {
            str = str.Unidecode().ToLower();
            str = InvalidCharsRegex.Replace(str, "");
            str = RemoveMultipleSpacesRegex.Replace(str, " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = SpaceRegex.Replace(str, "-"); // hyphens   
            return str;
        }
    }
}
