using System.IO;

namespace BenchmarkLab.Logic.Web.Blog
{
    public static class BlogLocationUtil
    {
        public static string GetBlogFilesLocation()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("blog_source", "public"));
        }

        public static string SitemapLocation()
        {
            return Path.Combine(GetBlogFilesLocation(), "sitemap.xml");
        }
    }
}
