using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BenchmarkLab.Logic.Web.Blog
{
    public class BlogLocationUtil
    {
        public static string GetBlogFilesLocation(IHostingEnvironment env)
        {
            return Path.Combine(env.WebRootPath, Path.Combine("blog_source", "public"));
        }

        public static string SitemapLocation(IHostingEnvironment env)
        {
            return Path.Combine(GetBlogFilesLocation(env), "sitemap.xml");
        }
    }
}
