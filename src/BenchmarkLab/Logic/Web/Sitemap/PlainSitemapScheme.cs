using System;

namespace BenchmarkLab.Logic.Web.Sitemap
{
    [Serializable, System.Xml.Serialization.XmlRoot("urlset")]
    public class Urlset
    {
        [System.Xml.Serialization.XmlElement("url")]
        public B5_Url[] urls;
    }
    [System.Xml.Serialization.XmlType("url")]
    public class B5_Url
    {
        [System.Xml.Serialization.XmlElement("loc")]
        public string loc;
        [System.Xml.Serialization.XmlElement("lastmod")]
        public string lastmod;
        [System.Xml.Serialization.XmlElement("changefreq")]
        public string changefreq;
    }
}
