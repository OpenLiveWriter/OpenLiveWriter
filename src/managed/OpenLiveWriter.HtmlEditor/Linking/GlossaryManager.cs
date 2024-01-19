// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    /// <summary>
    /// Summary description for GlossaryManager.
    /// </summary>
    public class GlossaryManager
    {
        private static readonly object _lock = new object();
        private static GlossaryManager _instance;

        private readonly XmlDocument glossaryDocument;
        private readonly XmlNode mainNode;
        private readonly Hashtable _entries;
        private GlossaryUrlSuggester finder = new GlossaryUrlSuggester();

        private GlossaryManager()
        {
            //load up the hashmap of items
            _entries = Hashtable.Synchronized(new Hashtable(StringComparer.CurrentCultureIgnoreCase));

            InitializeDocument();
            glossaryDocument = new XmlDocument();

            try
            {
                glossaryDocument.Load(_glossaryFile);
            }
            catch (XmlException e)
            {
                Trace.Fail("Error loading Glossary.xml: " + e);
                return;
            }

            // get the list of glossary items from the xml
            mainNode = glossaryDocument.SelectSingleNode("//glossary");
            if (mainNode == null)
            {
                Trace.Fail("Invalid Glossary.xml file detected: Couldn't find root glossary node.");
                return;
            }

            XmlNodeList entryNodes = glossaryDocument.SelectNodes("//glossary/entry");

            try
            {
                if (!GlossarySettings.Initialized) // Initialize the default
                {
                    if (entryNodes == null || entryNodes.Count == 0)
                    {
                        AddEntry(Res.Get(StringId.GlossaryExampleTitle), "http://www.OpenLiveWriter.com", "", "", false);
                    }
                }
            }
            catch (ArgumentException e)
            {
                Trace.Fail("Failed to initialize default glossary entries: " + e);
            }

            if (entryNodes != null)
            {
                foreach (XmlNode entryNode in entryNodes)
                {
                    string linktext;
                    GlossaryLinkItem item;

                    try
                    {
                        linktext = NodeText(entryNode.SelectSingleNode(TEXT));
                        item = GlossaryLinkItemFromXml(entryNode);
                    }
                    catch (ArgumentException e)
                    {
                        Trace.Fail("Failed parsing glossary entry: " + e);
                        continue;
                    }

                    try
                    {
                        _entries.Add(linktext, item);
                        finder.Add(linktext, item);
                    }
                    catch (ArgumentException e)
                    {
                        Trace.Fail("Duplicate entries in Glossary.xml detected: " + e);
                    }
                }
            }

        }

        #region Public Methods
        /// <summary>
        /// Returns the singleton plugin manager instance.
        /// </summary>
        public static GlossaryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GlossaryManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public ICollection GetItems()
        {
            lock (_lock)
            {
                ArrayList valuesList = new ArrayList(_entries.Values);
                valuesList.Sort(Comparer.DefaultInvariant);
                return valuesList;
            }
        }

        public void DeleteEntry(string text)
        {
            lock (_lock)
            {
                RemoveEntry(text);
            }
        }

        public GlossaryLinkItem AddFromForm(Form parent)
        {
            //show form to get entry data
            using (AddLinkDialog addForm = new AddLinkDialog(_entries.Keys))
            {
                if (DialogResult.OK == addForm.ShowDialog(parent))
                {
                    return AddEntry(addForm.LinkText, addForm.Url, addForm.Title, addForm.Rel, addForm.OpenInNewWindow);
                }
            }
            return null;
        }

        public GlossaryLinkItem EditEntry(Form parent, string text)
        {
            //shows form to edit existing entry
            using (AddLinkDialog editForm = new AddLinkDialog(_entries.Keys))
            {
                editForm.Edit = true;
                GlossaryLinkItem editItem = (GlossaryLinkItem)_entries[text];
                editForm.LinkText = editItem.Text;
                editForm.Url = editItem.Url;
                editForm.Title = editItem.Title;
                editForm.OpenInNewWindow = editItem.OpenInNewWindow;
                editForm.Rel = editItem.Rel;
                if (DialogResult.OK == editForm.ShowDialog(parent))
                {
                    //if the link text was changed, make sure to delete the original entry
                    if (!editItem.Text.Equals(editForm.LinkText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (_entries.ContainsKey(editForm.LinkText))
                        {
                            if (DisplayMessage.Show(MessageId.ConfirmReplaceEntry) == DialogResult.Yes)
                                RemoveEntry(text);
                            else
                                return null;
                        }
                        lock (_lock)
                        {
                            DeleteEntry(editItem.Text);
                            return AddEntry(editForm.LinkText, editForm.Url, editForm.Title, editForm.Rel, editForm.OpenInNewWindow);
                        }
                    }
                    else
                    {
                        return AddEntry(editForm.LinkText, editForm.Url, editForm.Title, editForm.Rel, editForm.OpenInNewWindow);
                    }

                }
            }
            return null;

        }

        // the match length is the number of matching characters from the end of the string
        public GlossaryLinkItem SuggestLink(string text, out int matchLength)
        {
            lock (_lock)
            {
                return finder.FindMatch(text, out matchLength);
            }
        }

        public GlossaryLinkItem FindEntry(string text)
        {
            lock (_lock)
            {
                if (_entries.ContainsKey(text))
                {
                    return (GlossaryLinkItem)_entries[text];
                }
                return null;
            }
        }

        public bool FindExactEntry(string text, string url, string title)
        {
            lock (_lock)
            {
                if (_entries.ContainsKey(text))
                {
                    GlossaryLinkItem foundItem = (GlossaryLinkItem)_entries[text];
                    return (foundItem.Url.Equals(url, StringComparison.OrdinalIgnoreCase) &&
                        foundItem.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                }
                return false;
            }
        }

        public bool ContainsEntry(string text)
        {
            lock (_lock)
            {
                return _entries.ContainsKey(text);
            }
        }

        public GlossaryLinkItem AddEntry(string text, string url, string title, string rel, bool openInNewWindow)
        {
            if (title == null)
                title = String.Empty;

            if (url == null)
                throw new ArgumentException("url cannot be null");

            if (text == null)
                throw new ArgumentException("text cannot be null");

            lock (_lock)
            {
                RemoveEntry(text);
                GlossaryLinkItem entry = new GlossaryLinkItem(text, url, title, rel, openInNewWindow);

                _entries.Add(text, entry);

                XmlNode newEntry = GlossaryLinkXmlFromItem(entry);
                mainNode.AppendChild(newEntry);
                SaveGlossary();

                finder.Add(text, entry);

                return entry;
            }
        }

        #endregion

        #region private

        private void RemoveEntry(string text)
        {
            if (_entries.ContainsKey(text))
            {
                XmlNodeList entryNodes = glossaryDocument.SelectNodes("//glossary/entry/text");
                bool removedXmlEntry = false;

                if (entryNodes != null)
                {
                    foreach (XmlNode node in entryNodes)
                    {
                        if (text.Equals(NodeText(node), StringComparison.CurrentCultureIgnoreCase))
                        {
                            mainNode.RemoveChild(node.ParentNode);
                            removedXmlEntry = true;
                        }
                    }
                }

                Debug.Assert(removedXmlEntry, "Glossary entry existed in Hashtable but not the XML file!");

                _entries.Remove(text);

                SaveGlossary();

                finder = new GlossaryUrlSuggester();
                foreach (DictionaryEntry entry in _entries)
                {
                    finder.Add((string)entry.Key, (GlossaryLinkItem)entry.Value);
                }

            }
        }

        private static GlossaryLinkItem GlossaryLinkItemFromXml(XmlNode node)
        {
            string text = NodeText(node.SelectSingleNode(TEXT));
            if (text.Length == 0)
                throw new ArgumentException("Missing text parameter");

            string url = NodeText(node.SelectSingleNode(URL));
            if (url.Length == 0)
                throw new ArgumentException("Missing URL parameter");

            string title = NodeText(node.SelectSingleNode(TITLE));

            string rel = NodeText(node.SelectSingleNode(REL));

            string openInNewWindowText = NodeText(node.SelectSingleNode(NEWWINDOW));
            bool openInNewWindow;
            if (!bool.TryParse(openInNewWindowText, out openInNewWindow))
            {
                // Default to true if unable to parse it.
                openInNewWindow = true;
            }

            return new GlossaryLinkItem(text, url, title, rel, openInNewWindow);
        }

        private const string TEXT = "text";
        private const string URL = "url";
        private const string TITLE = "title";
        private const string REL = "rel";
        private const string NEWWINDOW = "openInNewWindow";

        private XmlNode GlossaryLinkXmlFromItem(GlossaryLinkItem item)
        {
            XmlElement entryNode = glossaryDocument.CreateElement("entry");

            AppendNode(entryNode, TEXT, item.Text);
            AppendNode(entryNode, URL, item.Url);
            AppendNode(entryNode, TITLE, item.Title);
            AppendNode(entryNode, REL, item.Rel);
            AppendNode(entryNode, NEWWINDOW, item.OpenInNewWindow.ToString());

            return entryNode;
        }

        private void AppendNode(XmlNode entryNode, string elementName, string value)
        {
            XmlElement subNode = glossaryDocument.CreateElement(elementName);
            subNode.InnerText = value;
            entryNode.AppendChild(subNode);
        }

        private static void InitializeDocument()
        {
            if (!Directory.Exists(_glossaryDirectory))
                Directory.CreateDirectory(_glossaryDirectory);
            if (!File.Exists(_glossaryFile))
            {
                XmlDocument emptyGlossary = new XmlDocument();
                XmlDeclaration declaration = emptyGlossary.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlElement rootNode = emptyGlossary.CreateElement("glossary");
                emptyGlossary.InsertBefore(declaration, emptyGlossary.DocumentElement);
                emptyGlossary.AppendChild(rootNode);
                emptyGlossary.Save(_glossaryFile);
            }
        }

        private static string _glossaryDirectory
        {
            get
            {
                string appData = ApplicationEnvironment.ApplicationDataDirectory;
                if (appData == null)
                    appData = Path.GetTempPath();
                return Path.Combine(appData, "LinkGlossary");
            }
        }

        private static string _glossaryFile
        {
            get
            {
                return Path.Combine(_glossaryDirectory, "linkglossary.xml");
            }
        }

        public int MaxLengthHint
        {
            get { return finder.MaxLengthHint; }
        }

        private void SaveGlossary()
        {
            glossaryDocument.Save(_glossaryFile);
        }

        private static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

        #endregion

    }

    public class GlossaryLinkItem : IComparable
    {
        private readonly string _text;
        private readonly string _url;
        private readonly string _title;
        private readonly bool _openInNewWindow;
        private readonly string _rel;

        public GlossaryLinkItem(string text, string url, string title, string rel, bool openInNewWindow)
        {
            if (text == null)
            {
                throw new ArgumentException("text cannot be null");
            }

            if (url == null)
            {
                throw new ArgumentException("url cannot be null");
            }

            if (title == null)
            {
                throw new ArgumentException("title cannot be null");
            }

            _text = text.Trim();
            _url = url.Trim();
            _title = title.Trim();
            _rel = rel;
            _openInNewWindow = openInNewWindow;
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }
        public string Url
        {
            get
            {
                return _url;
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
        }

        public bool OpenInNewWindow
        {
            get
            {
                return _openInNewWindow;
            }
        }

        public string Rel
        {
            get
            {
                return _rel;
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return String.Compare(Text, ((GlossaryLinkItem)obj).Text, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }

    public class GlossaryUrlSuggester
    {
        public void Add(string text, GlossaryLinkItem value)
        {
            _maxLengthHint = Math.Max(text.Length, _maxLengthHint);
            _trie.AddReverse(text.ToLower(CultureInfo.CurrentCulture
                ), value);
        }

        public GlossaryLinkItem FindMatch(string text, out int length)
        {
            if (text == null)
            {
                length = -1;
                return null;
            }
            return _trie.Find(StringHelper.Reverse(text).ToLower(CultureInfo.CurrentCulture), IsAtWordBreak, out length);
        }

        public int MaxLengthHint
        {
            get
            {

                return _maxLengthHint + 1;
            }
        }
        private int _maxLengthHint = -1;

        private readonly Trie<GlossaryLinkItem> _trie = new Trie<GlossaryLinkItem>();

        private static bool IsAtWordBreak(string text, int charactersMatched)
        {
            return (charactersMatched >= text.Length || !char.IsLetterOrDigit(text[charactersMatched]));
        }
    }

}
