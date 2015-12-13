// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    internal sealed class BlogProviderAccountWizard
    {
        public static IBlogProviderAccountWizardDescription[] InstalledAccountWizards
        {
            get
            {
                lock (_classLock)
                {
                    if (_installedAccountWizards == null)
                    {
                        ArrayList installedAccountWizards = new ArrayList();

                        // load account wizards from registered xml descriptors
                        // (disabled for now)
                        //installedAccountWizards.AddRange(LoadAccountWizardsFromXml()) ;

                        _installedAccountWizards = installedAccountWizards.ToArray(typeof(IBlogProviderAccountWizardDescription)) as IBlogProviderAccountWizardDescription[];
                    }
                    return _installedAccountWizards;
                }
            }
        }
        private static IBlogProviderAccountWizardDescription[] _installedAccountWizards;

        private static ArrayList LoadAccountWizardsFromXml()
        {
            ArrayList accountWizardsFromXml = new ArrayList();

            try
            {
                using (SettingsPersisterHelper settingsKey = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("AccountWizard\\Custom"))
                {
                    foreach (string customizationName in settingsKey.GetNames())
                    {
                        IBlogProviderAccountWizardDescription wizardDescription = LoadAccountWizardFromXml(settingsKey.GetString(customizationName, String.Empty));
                        if (wizardDescription != null)
                            accountWizardsFromXml.Add(wizardDescription);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in LoadAccountWizardsFromXml: " + ex.ToString());
            }

            return accountWizardsFromXml;
        }

        private static IBlogProviderAccountWizardDescription LoadAccountWizardFromXml(string xmlDocumentPath)
        {
            try
            {
                return new BlogProviderAccountWizardDescriptionFromXml(xmlDocumentPath);
            }
            catch (Exception ex)
            {
                Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected exception loading account wizard {0}: {1}", xmlDocumentPath, ex.ToString()));
                return null;
            }
        }

        private static readonly object _classLock = new object();
    }

    internal interface IBlogProviderAccountWizardDescription
    {
        // identification
        string ServiceName { get; }

        // welcome page
        IBlogProviderWelcomePage WelcomePage { get; }

        // account creation link
        IBlogProviderAccountCreationLink AccountCreationLink { get; }
    }

    internal interface IBlogProviderWelcomePage
    {
        string Caption { get; }
        string Text { get; }
    }

    internal interface IBlogProviderAccountCreationLink
    {
        Image Icon { get; }
        string Text { get; }
        string Url { get; }
    }

    internal class BlogProviderAccountWizardDescription : IBlogProviderAccountWizardDescription
    {
        protected BlogProviderAccountWizardDescription()
        {
        }

        protected void Init(string serviceName, IBlogProviderWelcomePage welcomePage, IBlogProviderAccountCreationLink accountCreationLink)
        {
            if (serviceName == null)
                throw new ArgumentNullException("serviceName");

            _serviceName = serviceName;
            _welcomePage = welcomePage;
            _accountCreationLink = accountCreationLink;
        }

        public string ServiceName
        {
            get
            {
                return _serviceName;
            }
        }
        private string _serviceName;

        public IBlogProviderWelcomePage WelcomePage
        {
            get
            {
                return _welcomePage;
            }
        }
        private IBlogProviderWelcomePage _welcomePage;

        public IBlogProviderAccountCreationLink AccountCreationLink
        {
            get
            {
                return _accountCreationLink;
            }
        }
        private IBlogProviderAccountCreationLink _accountCreationLink;

    }

    internal class BlogProviderWelcomePage : IBlogProviderWelcomePage
    {
        public BlogProviderWelcomePage(string caption, string text)
        {
            _caption = caption;
            _text = text;
        }

        public string Caption
        {
            get
            {
                return _caption;
            }
        }
        private string _caption;

        public string Text
        {
            get
            {
                return _text;
            }
        }
        private string _text;
    }

    internal abstract class BlogProviderAccountCreationLink : IBlogProviderAccountCreationLink
    {
        public BlogProviderAccountCreationLink(string text, string url)
        {
            _text = text;
            _url = url;
        }

        public abstract Image Icon
        {
            get;
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }
        private string _text;

        public string Url
        {
            get
            {
                return _url;
            }
        }
        private string _url;
    }

    internal class BlogProviderAccountCreationLinkFromResource : BlogProviderAccountCreationLink
    {
        public BlogProviderAccountCreationLinkFromResource(string imagePath, string text, string url)
            : base(text, url)
        {
            _imagePath = imagePath;
        }

        public override Image Icon
        {
            get
            {
                try
                {
                    return ResourceHelper.LoadAssemblyResourceBitmap(_imagePath, true);
                }
                catch
                {
                    Trace.Fail("accountCreationLink image path not found: " + _imagePath);
                    return null;
                }
            }
        }

        private string _imagePath;

    }

    internal class BlogProviderAccountCreationLinkFromFile : BlogProviderAccountCreationLink
    {
        public BlogProviderAccountCreationLinkFromFile(string imagePath, string text, string url)
            : base(text, url)
        {
            _imagePath = imagePath;
        }

        public override Image Icon
        {
            get
            {
                try
                {
                    return new Bitmap(_imagePath);
                }
                catch
                {
                    Trace.Fail("accountCreationLink image path not found: " + _imagePath);
                    return null;
                }
            }
        }

        private string _imagePath;
    }

    internal class BlogProviderAccountWizardDescriptionFromXml : BlogProviderAccountWizardDescription
    {
        public BlogProviderAccountWizardDescriptionFromXml(string wizardDocumentPath)
        {
            // load the xml document
            XmlDocument wizardDocument = new XmlDocument();
            wizardDocument.Load(wizardDocumentPath);

            // custom account wizard node
            XmlNode customAccountWizardNode = wizardDocument.SelectSingleNode("//customAccountWizard");
            if (customAccountWizardNode == null)
                throw new Exception("Required root element customAccountWizard not specified");

            // service name
            string serviceName = NodeText(customAccountWizardNode.SelectSingleNode("serviceName"));
            if (serviceName == String.Empty)
                throw new Exception("Required element serviceName is not specified or empty");

            // welcome page is optional
            BlogProviderWelcomePage welcomePage = null;
            string welcomePageCaption = NodeText(customAccountWizardNode.SelectSingleNode("welcomePage/caption"));
            string welcomePageText = NodeText(customAccountWizardNode.SelectSingleNode("welcomePage/text"));
            if (welcomePageCaption != String.Empty && welcomePageText != String.Empty)
                welcomePage = new BlogProviderWelcomePage(welcomePageCaption, welcomePageText);

            // account creation link is optional
            BlogProviderAccountCreationLink accountCreationLink = null;
            string imagePath = NodeText(customAccountWizardNode.SelectSingleNode("accountCreationLink/imagePath"));
            if (imagePath != String.Empty)
            {
                if (!Path.IsPathRooted(imagePath))
                    imagePath = Path.Combine(Path.GetDirectoryName(wizardDocumentPath), imagePath);
            }
            string caption = NodeText(customAccountWizardNode.SelectSingleNode("accountCreationLink/caption"));
            string link = NodeText(customAccountWizardNode.SelectSingleNode("accountCreationLink/link"));
            if (imagePath != String.Empty && caption != String.Empty && link != String.Empty)
                accountCreationLink = new BlogProviderAccountCreationLinkFromFile(imagePath, caption, link);

            // initialize
            Init(serviceName, welcomePage, accountCreationLink);
        }

        private string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }
    }

}
