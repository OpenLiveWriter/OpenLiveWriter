// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.Video;
using ContentAlignment = OpenLiveWriter.Api.Alignment;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    public class SmartContent : ISmartContent, ILayoutStyle, ISupportingFiles, IInternalContent
    {
        private readonly static string SETTING_ALIGNMENT = "microsoft.internal.alignment";
        private readonly static string SETTING_PADDING_TOP = "microsoft.internal.paddingTop";
        private readonly static string SETTING_PADDING_RIGHT = "microsoft.internal.paddingRight";
        private readonly static string SETTING_PADDING_BOTTOM = "microsoft.internal.paddingBottom";
        private readonly static string SETTING_PADDING_LEFT = "microsoft.internal.paddingLeft";

        private IExtensionData _extensionData;

        public SmartContent(IExtensionData extensionData)
        {
            _extensionData = extensionData;
        }

        public DateTime? RefreshCallback
        {
            get
            {
                return _extensionData.RefreshCallBack;
            }
            set
            {
                _extensionData.RefreshCallBack = value;
            }
        }

        public Object ObjectState
        {
            get
            {
                return _extensionData.ObjectState;
            }
            set
            {
                _extensionData.ObjectState = value;
            }
        }

        public IExtensionData ExtensionData
        {
            get
            {
                return _extensionData;
            }
        }

        public IProperties Properties
        {
            get
            {
                return _extensionData.Settings;
            }
        }

        public Alignment Alignment
        {
            get
            {
                try
                {
                    string alignment = Properties.GetString(SETTING_ALIGNMENT, Api.Alignment.None.ToString());
                    Alignment cAlignment = (Alignment)Api.Alignment.Parse(typeof(Alignment), alignment);
                    return cAlignment;
                }
                catch (Exception)
                {
                    return Alignment.None;
                }
            }
            set { Properties.SetString(SETTING_ALIGNMENT, value.ToString()); }
        }

        public ILayoutStyle Layout
        {
            get { return this; }
        }

        public ISupportingFiles Files
        {
            get { return this; }
        }

        public string[] SupportingFileIds
        {
            get { return (this as ISupportingFiles).Filenames; }
        }

        public Stream GetFileStream(string fileId)
        {
            return (this as ISupportingFiles).Open(fileId);
        }

        Stream ISupportingFiles.Open(string fileName)
        {
            return (this as ISupportingFiles).Open(fileName, false);
        }

        Stream ISupportingFiles.Open(string fileName, bool create)
        {
            ISupportingFiles files = this as ISupportingFiles;

            if (!files.Contains(fileName))
            {
                using (MemoryStream emptyStream = new MemoryStream())
                    _extensionData.AddFile(fileName, emptyStream);
            }

            return _extensionData.GetFileStream(fileName);
        }

        void ISupportingFiles.AddFile(string fileName, string sourceFilePath)
        {
            //Writer Beta1 has a bug that will cause AddFile to overwrite the source file with a zero byte file.
            //To prevent users from ever encountering this bug, this method has been marked obsolete and throws
            //an exception so no developers will ever use it.
            throw new NotSupportedException("ISupportingFiles.AddFile() is not supported, use ISupportingFiles.Add() instead");
        }

        void ISupportingFiles.Add(string fileName, string sourceFilePath)
        {
            using (FileStream fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                _extensionData.AddFile(fileName, fileStream);
        }

        void ISupportingFiles.AddImage(string fileName, Image image, ImageFormat imageFormat)
        {
            // convert the bitmap into a stream
            using (MemoryStream imageStream = new MemoryStream())
            {
                // save the image into the strea
                if (image is Bitmap)
                    ImageHelper2.SaveImage((Bitmap)image, imageFormat, imageStream);
                else
                    image.Save(imageStream, imageFormat);

                // add the stream to the supporting file
                imageStream.Seek(0, SeekOrigin.Begin);
                _extensionData.AddFile(fileName, imageStream);
            }
        }

        void ISupportingFiles.Remove(string fileName)
        {
            _extensionData.RemoveFile(fileName);
        }

        void ISupportingFiles.RemoveAll()
        {
            ISupportingFiles files = this as ISupportingFiles;
            foreach (string fileName in files.Filenames)
                files.Remove(fileName);
        }

        Uri ISupportingFiles.GetUri(string fileName)
        {
            return _extensionData.GetFileUri(fileName);
        }

        bool ISupportingFiles.Contains(string fileName)
        {
            ISupportingFiles files = this as ISupportingFiles;
            foreach (string existingFile in files.Filenames)
                if (fileName == existingFile)
                    return true; // found the target file

            // didn't find the target file
            return false;
        }

        string[] ISupportingFiles.Filenames
        {
            get
            {
                return _extensionData.FileIds;
            }
        }

        int ILayoutStyle.TopMargin
        {
            get { return Properties.GetInt(SETTING_PADDING_TOP, 0); }
            set { Properties.SetInt(SETTING_PADDING_TOP, value); }
        }

        int ILayoutStyle.RightMargin
        {
            get { return Properties.GetInt(SETTING_PADDING_RIGHT, 0); }
            set { Properties.SetInt(SETTING_PADDING_RIGHT, value); }
        }

        int ILayoutStyle.BottomMargin
        {
            get { return Properties.GetInt(SETTING_PADDING_BOTTOM, 0); }
            set { Properties.SetInt(SETTING_PADDING_BOTTOM, value); }
        }

        int ILayoutStyle.LeftMargin
        {
            get { return Properties.GetInt(SETTING_PADDING_LEFT, 0); }
            set { Properties.SetInt(SETTING_PADDING_LEFT, value); }
        }

        public string Id
        {
            get { return _extensionData.Id; }
        }

    }
}
