using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSitePage : StaticSitePost
    {
        public StaticSitePage(StaticSiteConfig config) : base(config)
        {
        }

        public StaticSitePage(StaticSiteConfig config, BlogPost blogPost) : base(config, blogPost)
        {
        }


    }
}
