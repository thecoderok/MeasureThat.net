using System.Collections.Generic;

namespace MeasureThat.Net.Logic.Web
{
    public class SecurityHeadersPolicy
    {
        public IDictionary<string, string> SetHeaders { get; }
             = new Dictionary<string, string>();

        public ISet<string> RemoveHeaders { get; }
            = new HashSet<string>();
    }
}
