// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// ContentEditorAccountAdapter takes a IContentTarget provided by hosting applications and
    /// converts it into something that can be consumed by ContentEditor while hiding "implementation details"
    /// from hosting applications.
    /// </summary>
    public class ContentEditorAccountAdapter : IEditorAccount, IEditorOptions
    {
        /// <summary>
        ///
        /// </summary>
        public ContentEditorAccountAdapter()
        {
            // WinLive 182295: Set all the EditorOptions just once so they are immutable.
            RequiresXHTML = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.XHTML);
            MaxPostTitleLength = Int32.MaxValue;
            IsRTLTemplate = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.RTLDirectionDefault);
            HasRTLFeatures = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.RTLFeatures);
            SupportsImageUpload = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ImageUpload) ? SupportsFeature.Yes : SupportsFeature.No;
            SupportsScripts = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.Script) ? SupportsFeature.Yes : SupportsFeature.No;
            SupportsEmbeds = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.Embeds) ? SupportsFeature.Yes : SupportsFeature.No;
            PostBodyBackgroundColor = Color.White.ToArgb();
            DhtmlImageViewer = null;
        }

        #region IEditorAccount Members

        public string HomepageBaseUrl
        {
            get { return null; }
        }

        public static string AccountId = new Guid("EB22109F-B2D6-43EB-9167-ABA093310492").ToString();
        public string Id
        {
            get
            {
                return AccountId;
            }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string HomepageUrl
        {
            get { return null; }
        }

        public string ServiceName
        {
            get { throw new NotImplementedException(); }
        }

        public string ProviderId
        {
            get { return new Guid("E693C6CA-963D-46aa-904B-70CE92EF5C80").ToString(); }
        }

        public IEditorOptions EditorOptions
        {
            get { return this; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        #region IEditorOptions Members

        public bool RequiresXHTML
        {
            get;
            private set;
        }

        public int MaxPostTitleLength
        {
            get;
            private set;
        }

        public bool IsRTLTemplate
        {
            get;
            private set;
        }

        public bool HasRTLFeatures
        {
            get;
            private set;
        }

        public SupportsFeature SupportsImageUpload
        {
            get;
            private set;
        }

        public SupportsFeature SupportsScripts
        {
            get;
            private set;
        }

        public SupportsFeature SupportsEmbeds
        {
            get;
            private set;
        }

        public int PostBodyBackgroundColor
        {
            get;
            private set;
        }

        public string DhtmlImageViewer
        {
            get;
            private set;
        }

        #endregion
    }
}
