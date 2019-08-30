using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSiteConfigDetector
    {
        private static string[] IMG_DIR_CANDIDATES = new string[] { "images", "image", "img", "assets/img", "assets/images" };

        private StaticSiteConfig config;
        private string localSitePath;

        public StaticSiteConfigDetector(StaticSiteConfig config)
        {
            this.config = config;
            localSitePath = config.LocalSitePath;
        }

        public bool DoDetect()
        {
            if (DoJekyllDetect()) return true;
            // More detection methods would be added here for the various static site generators

            return false;
        }

        /// <summary>
        /// Attempt detection for a Jekyll project
        /// </summary>
        /// <returns>True if Jekyll detection succeeded</returns>
        public bool DoJekyllDetect()
        {
            // First, check for a Gemfile specifying jekyll
            var gemfilePath = Path.Combine(localSitePath, "Gemfile");
            if (!File.Exists(gemfilePath)) return false;
            if (!File.ReadAllText(gemfilePath).Contains("jekyll")) return false;

            // Find the config file
            var configPath = Path.Combine(localSitePath, "_config.yml");
            if (!File.Exists(configPath)) return false;

            // Jekyll site detected, set defaults
            // Posts path is almost always _posts, check that it exists before setting
            if (Directory.Exists(Path.Combine(localSitePath, "_posts"))) config.PostsPath = "_posts";
            // Pages enabled and in root dir
            config.PagesEnabled = true;
            config.PagesPath = ".";
            // If a _site dir exists, assume site is locally built
            if (Directory.Exists(Path.Combine(localSitePath, "_site")))
            {
                config.BuildingEnabled = true;
                config.OutputPath = "_site";
            }
            // Check for all possible image upload directories
            foreach(var dir in IMG_DIR_CANDIDATES)
            {
                if (Directory.Exists(Path.Combine(localSitePath, dir)))
                {
                    config.ImagesEnabled = true;
                    config.ImagesPath = dir;
                    break;
                }
            }

            var yaml = new YamlStream();
            try
            {
                // Attempt to load the YAML document
                yaml.Load(new StringReader(File.ReadAllText(configPath)));
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                // Fill values from config
                // Site title
                var titleNode = mapping.Where(kv => kv.Key.ToString() == "title");
                if (titleNode.Count() > 0) config.SiteTitle = titleNode.First().Value.ToString();

                // Homepage
                // Check for url node first
                var urlNode = mapping.Where(kv => kv.Key.ToString() == "url");
                if (urlNode.Count() > 0)
                {
                    config.SiteUrl = urlNode.First().Value.ToString();
                    // Now check for baseurl to apply to url
                    var baseurlNode = mapping.Where(kv => kv.Key.ToString() == "baseurl");
                    // Combine base url
                    if (baseurlNode.Count() > 0) config.SiteUrl = UrlHelper.UrlCombine(config.SiteUrl, baseurlNode.First().Value.ToString());
                }

                // Destination
                // If specified, local site building can be safely assumed to be enabled
                var destinationNode = mapping.Where(kv => kv.Key.ToString() == "destination");
                if(destinationNode.Count() > 0)
                {
                    config.BuildingEnabled = true;
                    config.OutputPath = destinationNode.First().Value.ToString();
                }
            } catch(Exception)
            {
                // YAML may be malformed, defaults are still set from above so return true
            }

            return true;
        }

        public static bool AttmeptAutoDetect(StaticSiteConfig config)
            => new StaticSiteConfigDetector(config).DoDetect();
    }
}
