// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient
{
    public class DhtmlImageViewers
    {
        public static ImageViewer GetImageViewer(string name)
        {
            if (name == null)
                return null;
            foreach (ImageViewer v in imageViewers.Value)
                if (v.Name == name)
                    return v;
            return null;
        }

        public static ImageViewer DetectImageViewer(string html, string sourceUrl)
        {
            List<ImageViewer> viewers = imageViewers;
            LazyLoader<List<Regex>> regexes = new LazyLoader<List<Regex>>(delegate
              {
                  List<Regex> regexList = new List<Regex>(viewers.Count);
                  foreach (ImageViewer v in viewers)
                  {
                      regexList.Add(new Regex(v.Pattern, RegexOptions.CultureInvariant));
                  }
                  return regexList;
              });

            HtmlExtractor ex = new HtmlExtractor(html);
            while (ex.Seek("<script src>").Success)
            {
                BeginTag tag = (BeginTag)ex.Element;
                string src = tag.GetAttributeValue("src");

                if (String.IsNullOrEmpty(src))
                {
                    continue;
                }

                try
                {
                    if (!UrlHelper.IsUrl(src))
                    {
                        // We need absolute URLs.
                        src = UrlHelper.EscapeRelativeURL(sourceUrl, src);
                    }

                    Uri srcUri = new Uri(src);
                    if (srcUri.IsAbsoluteUri)
                    {
                        // WinLive 248276: We want just the path portion since there could be an additional query or
                        // fragment on the URL that our regexs can't handle.
                        src = srcUri.GetLeftPart(UriPartial.Path);
                    }
                }
                catch (UriFormatException)
                {
                    // We'll just use the regex on the raw attribute value.
                }

                List<Regex> regexList = regexes.Value;
                for (int i = 0; i < regexList.Count; i++)
                {
                    if (regexList[i].IsMatch(src))
                        return viewers[i];
                }
            }
            return null;
        }

        internal List<ImageViewer> ImageViewers { get { return imageViewers; } }
        private static readonly LazyLoader<List<ImageViewer>> imageViewers = new LazyLoader<List<ImageViewer>>(_ImageViewers);
        static List<ImageViewer> _ImageViewers()
        {
            return (List<ImageViewer>)CabbedXmlResourceFileDownloader.Instance.
                                          ProcessLocalResource(
                                          Assembly.GetExecutingAssembly(),
                                          "ImageViewers.DhtmlImageViewers.xml",
                                          XmlLoader);
        }

        private static object XmlLoader(XmlDocument xmlDoc)
        {
            List<ImageViewer> viewers = new List<ImageViewer>();

            foreach (XmlElement el in xmlDoc.SelectNodes("/viewers/viewer"))
            {
                try
                {
                    ImageViewer viewer = new ImageViewer(
                        XmlHelper.NodeText(el.SelectSingleNode("name")),
                        XmlHelper.NodeText(el.SelectSingleNode("filePattern")),
                        (XmlElement)el.SelectSingleNode("single/a"),
                        (XmlElement)el.SelectSingleNode("group/a"));

                    viewers.Add(viewer);
                }
                catch (ArgumentException ex)
                {
                    Trace.Fail(ex.ToString());
                }
            }

            return viewers;
        }

        public static string GetLocalizedName(string name)
        {
            return name;
        }

    }

    public enum ImageViewerGroupSupport
    {
        None,
        Supported,
        Required
    }

    public class ImageViewer
    {
        private readonly string name;
        private readonly string pattern;
        private readonly XmlElement single;
        private readonly XmlElement group;

        public ImageViewer(string name, string pattern, XmlElement single, XmlElement group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException();

            if (single == null && group == null)
                throw new ArgumentNullException();

            this.name = name;
            this.pattern = pattern;
            this.single = single;
            this.group = group;
        }

        public string Name
        {
            get { return name; }
        }

        public string Pattern
        {
            get { return pattern; }
        }

        public ImageViewerGroupSupport GroupSupport
        {
            get
            {
                if (group == null)
                    return ImageViewerGroupSupport.None;
                if (single == null)
                    return ImageViewerGroupSupport.Required;
                return ImageViewerGroupSupport.Supported;
            }
        }

        public void Apply(IHTMLAnchorElement anchor, string groupName)
        {
            Remove(anchor);
            if (string.IsNullOrEmpty(groupName) || group == null)
            {
                foreach (XmlAttribute attr in single.Attributes)
                {
                    ((IHTMLElement)anchor).setAttribute(attr.Name, attr.Value, 0);
                }
            }
            else
            {
                foreach (XmlAttribute attr in group.Attributes)
                    ((IHTMLElement)anchor).setAttribute(attr.Name, attr.Value.Replace("{group-name}", groupName), 0);
            }
        }

        public void Detect(IHTMLAnchorElement anchor, ref bool useImageViewer, ref string groupName)
        {
            if (single != null)
            {
                bool isMatch = true;
                foreach (XmlAttribute attr in single.Attributes)
                {
                    string attrVal = ((IHTMLElement)anchor).getAttribute(attr.Name, 2) as string;
                    if (attrVal == null || attrVal != attr.Value)
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                {
                    useImageViewer = true;
                    groupName = null;
                    return;
                }
            }

            if (group != null)
            {
                bool isMatch = true;
                string detectedGroup = null;
                foreach (XmlAttribute attr in group.Attributes)
                {
                    Regex reverseRegex = new Regex(Regex.Escape(attr.Value).Replace(Regex.Escape("{group-name}"), "(.+?)"));
                    Match m;

                    string attrVal = ((IHTMLElement)anchor).getAttribute(attr.Name, 2) as string;
                    if (attrVal == null || !(m = reverseRegex.Match(attrVal)).Success)
                    {
                        isMatch = false;
                        break;
                    }

                    if (m.Groups[1].Success)
                        detectedGroup = m.Groups[1].Value;
                }

                if (isMatch)
                {
                    useImageViewer = true;
                    groupName = detectedGroup;
                    return;
                }
            }

            useImageViewer = false;
            groupName = null;
        }

        public void Remove(IHTMLAnchorElement element)
        {
            bool useImageViewer = false;
            string groupName = null;
            Detect(element, ref useImageViewer, ref groupName);
            if (!useImageViewer)
                return;

            foreach (XmlAttribute attr in ((groupName != null) ? group : single).Attributes)
                ((IHTMLElement)element).removeAttribute(attr.Name, 0);
        }
    }

}
