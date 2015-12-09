// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using DATADIR = System.Runtime.InteropServices.ComTypes.DATADIR;
using FORMATETC = System.Runtime.InteropServices.ComTypes.FORMATETC;
using IEnumFORMATETC = System.Runtime.InteropServices.ComTypes.IEnumFORMATETC;
using STGMEDIUM = System.Runtime.InteropServices.ComTypes.STGMEDIUM;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// We wrap the IHTMLDocument2 interface that we hand off to mail in order
    /// to provide of our own implementation for IDataObject::GetData when being
    /// asked for a CF_UNICODETEXT stream.
    /// If and when we move to .NET 4 we should use ICustomQueryInterface instead.
    /// </summary>
    public class HtmlDocument2Wrapper : IHTMLDocument2, IDataObject, IPersistStreamInit
    {
        private IHTMLDocument2 _innerDoc2;
        private IContentSourceSidebarContext _contentSourceSidebarContext;
        public HtmlDocument2Wrapper(IHTMLDocument2 doc2, IContentSourceSidebarContext contentSourceSidebarContext)
        {
            _innerDoc2 = doc2;
            _contentSourceSidebarContext = contentSourceSidebarContext;
        }

        #region IHTMLDocument2 implementation
        object IHTMLDocument.Script
        {
            get { return ((IHTMLDocument)_innerDoc2).Script; }
        }

        public IHTMLElementCollection all
        {
            get { return _innerDoc2.all; }
        }

        public IHTMLElement body
        {
            get { return _innerDoc2.body; }
        }

        public IHTMLElement activeElement
        {
            get { return _innerDoc2.activeElement; }
        }

        public IHTMLElementCollection images
        {
            get { return _innerDoc2.images; }
        }

        public IHTMLElementCollection applets
        {
            get { return _innerDoc2.applets; }
        }

        public IHTMLElementCollection links
        {
            get { return _innerDoc2.links; }
        }

        public IHTMLElementCollection forms
        {
            get { return _innerDoc2.forms; }
        }

        public IHTMLElementCollection anchors
        {
            get { return _innerDoc2.anchors; }
        }

        public string title
        {
            get { return _innerDoc2.title; }
            set { _innerDoc2.title = value; }
        }

        public IHTMLElementCollection scripts
        {
            get { return _innerDoc2.scripts; }
        }

        public string designMode
        {
            get { return _innerDoc2.designMode; }
            set { _innerDoc2.designMode = value; }
        }

        public IHTMLSelectionObject selection
        {
            get { return _innerDoc2.selection; }
        }

        public string readyState
        {
            get { return _innerDoc2.readyState; }
        }

        public FramesCollection frames
        {
            get { return _innerDoc2.frames; }
        }

        public IHTMLElementCollection embeds
        {
            get { return _innerDoc2.embeds; }
        }

        public IHTMLElementCollection plugins
        {
            get { return _innerDoc2.plugins; }
        }

        public object alinkColor
        {
            get { return _innerDoc2.alinkColor; }
            set { _innerDoc2.alinkColor = value; }
        }

        public object bgColor
        {
            get { return _innerDoc2.bgColor; }
            set { _innerDoc2.bgColor = value; }
        }

        public object fgColor
        {
            get { return _innerDoc2.fgColor; }
            set { _innerDoc2.fgColor = value; }
        }

        public object linkColor
        {
            get { return _innerDoc2.linkColor; }
            set { _innerDoc2.linkColor = value; }
        }

        public object vlinkColor
        {
            get { return _innerDoc2.vlinkColor; }
            set { _innerDoc2.vlinkColor = value; }
        }

        public string referrer
        {
            get { return _innerDoc2.referrer; }
        }

        public HTMLLocation location
        {
            get { return _innerDoc2.location; }
        }

        public string lastModified
        {
            get { return _innerDoc2.lastModified; }
        }

        public string url
        {
            get { return _innerDoc2.url; }
            set { _innerDoc2.url = value; }
        }

        public string domain
        {
            get { return _innerDoc2.domain; }
            set { _innerDoc2.domain = value; }
        }

        public string cookie
        {
            get { return _innerDoc2.cookie; }
            set { _innerDoc2.cookie = value; }
        }

        public bool expando
        {
            get { return _innerDoc2.expando; }
            set { _innerDoc2.expando = value; }
        }

        public string charset
        {
            get { return _innerDoc2.charset; }
            set { _innerDoc2.charset = value; }
        }

        public string defaultCharset
        {
            get { return _innerDoc2.defaultCharset; }
            set { _innerDoc2.defaultCharset = value; }
        }

        public string mimeType
        {
            get { return _innerDoc2.mimeType; }
        }

        public string fileSize
        {
            get { return _innerDoc2.fileSize; }
        }

        public string fileCreatedDate
        {
            get { return _innerDoc2.fileCreatedDate; }
        }

        public string fileModifiedDate
        {
            get { return _innerDoc2.fileModifiedDate; }
        }

        public string fileUpdatedDate
        {
            get { return _innerDoc2.fileUpdatedDate; }
        }

        public string security
        {
            get { return _innerDoc2.security; }
        }

        public string protocol
        {
            get { return _innerDoc2.protocol; }
        }

        public string nameProp
        {
            get { return _innerDoc2.nameProp; }
        }

        public void write(params object[] psarray)
        {
            _innerDoc2.write(psarray);
        }

        public void writeln(params object[] psarray)
        {
            _innerDoc2.writeln(psarray);
        }

        public object open(string urlParam, object name, object features, object replace)
        {
            return _innerDoc2.open(urlParam, name, features, replace);
        }

        public void close()
        {
            _innerDoc2.close();
        }

        public void clear()
        {
            _innerDoc2.clear();
        }

        public bool queryCommandSupported(string cmdID)
        {
            return _innerDoc2.queryCommandSupported(cmdID);
        }

        public bool queryCommandEnabled(string cmdID)
        {
            return _innerDoc2.queryCommandEnabled(cmdID);
        }

        public bool queryCommandState(string cmdID)
        {
            return _innerDoc2.queryCommandState(cmdID);
        }

        public bool queryCommandIndeterm(string cmdID)
        {
            return _innerDoc2.queryCommandIndeterm(cmdID);
        }

        public string queryCommandText(string cmdID)
        {
            return _innerDoc2.queryCommandText(cmdID);
        }

        public object queryCommandValue(string cmdID)
        {
            return _innerDoc2.queryCommandValue(cmdID);
        }

        public bool execCommand(string cmdID, bool showUI, object value)
        {
            return _innerDoc2.execCommand(cmdID, showUI, value);
        }

        public bool execCommandShowHelp(string cmdID)
        {
            return _innerDoc2.execCommandShowHelp(cmdID);
        }

        public IHTMLElement createElement(string eTag)
        {
            return _innerDoc2.createElement(eTag);
        }

        public object onhelp
        {
            get { return _innerDoc2.onhelp; }
            set { _innerDoc2.onhelp = value; }
        }

        public object onclick
        {
            get { return _innerDoc2.onclick; }
            set { _innerDoc2.onclick = value; }
        }

        public object ondblclick
        {
            get { return _innerDoc2.ondblclick; }
            set { _innerDoc2.ondblclick = value; }
        }

        public object onkeyup
        {
            get { return _innerDoc2.onkeyup; }
            set { _innerDoc2.onkeyup = value; }
        }

        public object onkeydown
        {
            get { return _innerDoc2.onkeydown; }
            set { _innerDoc2.onkeydown = value; }
        }

        public object onkeypress
        {
            get { return _innerDoc2.onkeypress; }
            set { _innerDoc2.onkeypress = value; }
        }

        public object onmouseup
        {
            get { return _innerDoc2.onmouseup; }
            set { _innerDoc2.onmouseup = value; }
        }

        public object onmousedown
        {
            get { return _innerDoc2.onmousedown; }
            set { _innerDoc2.onmousedown = value; }
        }

        public object onmousemove
        {
            get { return _innerDoc2.onmousemove; }
            set { _innerDoc2.onmousemove = value; }
        }

        public object onmouseout
        {
            get { return _innerDoc2.onmouseout; }
            set { _innerDoc2.onmouseout = value; }
        }

        public object onmouseover
        {
            get { return _innerDoc2.onmouseover; }
            set { _innerDoc2.onmouseover = value; }
        }

        public object onreadystatechange
        {
            get { return _innerDoc2.onreadystatechange; }
            set { _innerDoc2.onreadystatechange = value; }
        }

        public object onafterupdate
        {
            get { return _innerDoc2.onafterupdate; }
            set { _innerDoc2.onafterupdate = value; }
        }

        public object onrowexit
        {
            get { return _innerDoc2.onrowexit; }
            set { _innerDoc2.onrowexit = value; }
        }

        public object onrowenter
        {
            get { return _innerDoc2.onrowenter; }
            set { _innerDoc2.onrowenter = value; }
        }

        public object ondragstart
        {
            get { return _innerDoc2.ondragstart; }
            set { _innerDoc2.ondragstart = value; }
        }

        public object onselectstart
        {
            get { return _innerDoc2.onselectstart; }
            set { _innerDoc2.onselectstart = value; }
        }

        public IHTMLElement elementFromPoint(int x, int y)
        {
            return _innerDoc2.elementFromPoint(x, y);
        }

        public IHTMLWindow2 parentWindow
        {
            get { return _innerDoc2.parentWindow; }
        }

        public HTMLStyleSheetsCollection styleSheets
        {
            get { return _innerDoc2.styleSheets; }
        }

        public object onbeforeupdate
        {
            get { return _innerDoc2.onbeforeupdate; }
            set { _innerDoc2.onbeforeupdate = value; }
        }

        public object onerrorupdate
        {
            get { return _innerDoc2.onerrorupdate; }
            set { _innerDoc2.onerrorupdate = value; }
        }

        public string toString()
        {
            return _innerDoc2.toString();
        }

        public IHTMLStyleSheet createStyleSheet(string bstrHref, int lIndex)
        {
            return _innerDoc2.createStyleSheet(bstrHref, lIndex);
        }

        object IHTMLDocument2.Script
        {
            get { return _innerDoc2.Script; }
        }

        #endregion // IHTMLDocument2 implementation

        #region IDataObject implementation

        public void GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            switch (format.cfFormat)
            {
                case CF.TEXT:
                case CF.UNICODETEXT:
                    PlainTextDataObject.GetData(ref format, out medium);
                    break;
                default:
                    DataObject.GetData(ref format, out medium);
                    break;
            }
        }

        public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            switch (format.cfFormat)
            {
                case CF.TEXT:
                case CF.UNICODETEXT:
                    PlainTextDataObject.GetDataHere(ref format, ref medium);
                    break;
                default:
                    DataObject.GetDataHere(ref format, ref medium);
                    break;
            }
        }

        public int QueryGetData(ref FORMATETC format)
        {
            switch (format.cfFormat)
            {
                case CF.TEXT:
                case CF.UNICODETEXT:
                    return PlainTextDataObject.QueryGetData(ref format);
                default:
                    return DataObject.QueryGetData(ref format);
            }
        }

        public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            switch (formatIn.cfFormat)
            {
                case CF.TEXT:
                case CF.UNICODETEXT:
                    return PlainTextDataObject.GetCanonicalFormatEtc(ref formatIn, out formatOut);
                default:
                    return DataObject.GetCanonicalFormatEtc(ref formatIn, out formatOut);
            }
        }

        public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            switch (formatIn.cfFormat)
            {
                case CF.TEXT:
                case CF.UNICODETEXT:
                    PlainTextDataObject.SetData(ref formatIn, ref medium, release);
                    break;
                default:
                    DataObject.SetData(ref formatIn, ref medium, release);
                    break;
            }
        }

        public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
        {
            return DataObject.EnumFormatEtc(direction);
        }

        public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            return DataObject.DAdvise(ref pFormatetc, advf, adviseSink, out connection);
        }

        public void DUnadvise(int connection)
        {
            DataObject.DUnadvise(connection);
        }

        public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            return DataObject.EnumDAdvise(out enumAdvise);
        }

        #endregion // IDataObject implementation

        #region IPersistStreamInit implementation
        public int GetClassID(out Guid pClassID)
        {
            return PersistStreamInit.GetClassID(out pClassID);
        }

        public int IsDirty()
        {
            return PersistStreamInit.IsDirty();
        }

        public void Load(IStream pStm)
        {
            PersistStreamInit.Load(pStm);
        }

        public void Save(IStream pStm, bool fClearDirty)
        {
            PersistStreamInit.Save(pStm, fClearDirty);
        }

        public void GetSizeMax(out ulong pcbSize)
        {
            PersistStreamInit.GetSizeMax(out pcbSize);
        }

        public void InitNew()
        {
            PersistStreamInit.InitNew();
        }

        #endregion // IPersistStreamInit implementation

        private IPersistStreamInit PersistStreamInit
        {
            get { return (IPersistStreamInit)_innerDoc2; }
        }

        private IDataObject DataObject
        {
            get { return (IDataObject)_innerDoc2; }
        }

        private IDataObject _plainTextDataObject;
        private IDataObject PlainTextDataObject
        {
            get
            {
                if (_plainTextDataObject == null)
                {
                    string plainTextHtml = SmartContentWorker.PerformOperation(_innerDoc2.body.parentElement.outerHTML,
                                                                               PlainTextTransform,
                                                                               false,
                                                                               _contentSourceSidebarContext,
                                                                               null);

                    _plainTextDataObject = (IDataObject)HTMLDocumentHelper.StringToHTMLDoc(plainTextHtml, "");
                }

                return _plainTextDataObject;
            }
        }

        internal static void PlainTextTransform(IPublishingContext site, SmartContentSource source, ISmartContent sContent, ref string content)
        {
            if (source is IInternalSmartContentSource)
            {
                try
                {
                    content = ((IInternalSmartContentSource)source).GeneratePlainTextHtml(sContent, site);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Content Source failed to generate plain text html: " + ex);
                    Trace.Flush();
                    throw;
                }

            }
        }
    }
}
