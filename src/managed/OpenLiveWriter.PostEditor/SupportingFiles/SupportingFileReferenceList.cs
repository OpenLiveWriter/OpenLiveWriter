// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.PostEditor.SupportingFiles
{
    /// <summary>
    /// Helper for calculating active file references for various contexts.
    /// </summary>
    internal class SupportingFileReferenceList
    {
        Hashtable supportingFilesByUri = new Hashtable();
        Hashtable supportingFilesById = new Hashtable();
        Hashtable filesById = new Hashtable();
        IBlogPostEditingContext _editingContext;
        private SupportingFileReferenceList(IBlogPostEditingContext editingContext)
        {
            _editingContext = editingContext;
        }

        public static SupportingFileReferenceList CalculateReferencesForSave(IBlogPostEditingContext editingContext)
        {
            SupportingFileReferenceList list = new SupportingFileReferenceList(editingContext);
            list.CalculateReferencesForSave();
            return list;
        }

        public static SupportingFileReferenceList CalculateReferencesForPublish(IBlogPostEditingContext editingContext)
        {
            SupportingFileReferenceList list = new SupportingFileReferenceList(editingContext);
            list.CalculateReferencesForPublish();
            return list;
        }

        public ISupportingFile GetSupportingFileByUri(Uri uri)
        {
            return (ISupportingFile)supportingFilesByUri[uri];
        }

        public bool IsReferenced(Uri uri)
        {
            return supportingFilesByUri[uri] != null;
        }

        public bool IsReferenced(ISupportingFile supportingFile)
        {
            return supportingFilesById[supportingFile.FileId] != null;
        }

        public bool IsReferenced(SupportingFileFactory fileFactory)
        {
            return filesById[fileFactory.FileId] != null;
        }

        public ISupportingFile[] GetReferencedSupportingFiles()
        {
            ArrayList list = new ArrayList();
            foreach (ISupportingFile file in supportingFilesById.Values)
            {
                list.Add(file);
            }
            return (ISupportingFile[])list.ToArray(typeof(ISupportingFile));
        }

        private void CalculateReferencesForPublish()
        {
            //calculate references in the html content
            string postContents = _editingContext.BlogPost.Contents;
            HtmlReferenceFixer.FixReferences(postContents, new ReferenceFixer(AddHtmlReference));
        }

        private void CalculateReferencesForSave()
        {
            //calculate references in the html content
            string postContents = _editingContext.BlogPost.Contents;
            HtmlReferenceFixer.FixReferences(postContents, new ReferenceFixer(AddHtmlReference));

            //calculate the images referenced by the HTML
            AddImageReferences(_editingContext.ImageDataList);

            //calculate files referenced by plugins
            foreach (BlogPostExtensionData extensionData in _editingContext.ExtensionDataList.CalculateReferencedExtensionData(postContents))
            {
                foreach (string fileId in extensionData.FileIds)
                {
                    ISupportingFile file = extensionData.GetSupportingFile(fileId);
                    if (file != null)
                        AddReference(file);
                }
            }
        }

        private void AddReference(ISupportingFile supportingFile)
        {
            SupportingFileFactory.VersionedSupportingFile versionedSupportingFile =
                (SupportingFileFactory.VersionedSupportingFile)supportingFile;
            supportingFilesByUri[supportingFile.FileUri] = supportingFile;
            supportingFilesById[supportingFile.FileId] = supportingFile;
            filesById[versionedSupportingFile.SupportingFileFactory.FileId] = versionedSupportingFile.SupportingFileFactory;
        }

        private string AddHtmlReference(BeginTag tag, string reference)
        {
            if (UrlHelper.IsUrl(reference))
            {
                ISupportingFile supportingFile = _editingContext.SupportingFileService.GetFileByUri(new Uri(reference));
                if (supportingFile != null && supportingFile.Embedded)
                {
                    AddReference(supportingFile);
                }
            }
            return reference;
        }

        private void AddImageReferences(BlogPostImageDataList list)
        {
            //inline and target images are added while parsing the HTML references, but their associated
            //source images are not, so we need to explicitly add them.
            foreach (BlogPostImageData imageData in list)
            {
                if (imageData != null && imageData.InlineImageFile != null && imageData.InlineImageFile.SupportingFile != null)
                {
                    if (IsReferenced(imageData.InlineImageFile.SupportingFile))
                    {
                        AddReference(imageData.ImageSourceFile.SupportingFile);
                        if (imageData.ImageSourceShadowFile != null)
                            AddReference(imageData.ImageSourceShadowFile.SupportingFile);
                    }
                }
            }
        }
    }
}
