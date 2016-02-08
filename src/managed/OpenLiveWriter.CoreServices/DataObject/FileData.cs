// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Data transfer for a group of files
    /// </summary>
    public class FileData
    {

        /// <summary>
        /// Creates a new FileData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the new FileData</param>
        /// <returns>The FileData, null if none could be created</returns>
        public static FileData Create(IDataObject iDataObject)
        {
            // determine which file item format to use depending upon what the
            // contents of the data object are
            foreach (IFileItemFormat format in GetFormatsForDataObject(iDataObject))
            {
                if (format.CanCreateFrom(iDataObject))
                {
                    return new FileData(iDataObject, format);
                }
            }
            return null;
        }

        /// <summary>
        /// The FileItems contained in the data object.
        /// </summary>
        public FileItem[] Files
        {
            get
            {
                // create on demand and cache
                if (m_files == null)
                    m_files = m_fileItemFormat.CreateFileItems(m_dataObject);
                return m_files;
            }
        }

        /// <summary>
        /// FileData Constructor
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the FileData</param>
        private FileData(IDataObject iDataObject, IFileItemFormat format)
        {
            // save a reference to the data object
            m_dataObject = iDataObject;
            m_fileItemFormat = format;

            // debug-only sanity check on formats
            VerifyFormatsMutuallyExclusive(iDataObject);
        }

        /// <summary>
        /// Initialize list of FileItem formats that we support
        /// </summary>
        private static ArrayList fileItemFormats =
            ArrayList.Synchronized(new ArrayList(new IFileItemFormat[]
                {
                    new FileItemFileItemFormat(),
                    new FileItemFileDropFormat(),
                    new FileItemFileContentsFormat() }
            ));

        /// <summary>
        /// Initialize list of FileItem formats that we support
        /// </summary>
        private static ArrayList mozillaFileItemFormats =
            ArrayList.Synchronized(new ArrayList(new IFileItemFormat[]
                {
                    new FileItemFileContentsFormat(),
                    new FileItemFileItemFormat(),
                    new FileItemFileDropFormat()
                    }
            ));

        /// <summary>
        /// Returns a list of FileItemFormats for a given dataobject
        /// </summary>
        private static ArrayList GetFormatsForDataObject(IDataObject dataObject)
        {
            // Mozilla often time is very sketchy about their filedrops.  They
            // may place null in the filedrop format, or they may place paths
            // to files that are highly transient and may or may not be available
            // when you need then (many images will get reset to 0 byte files)
            // As a result of this, we prefer FileContents when using Mozilla
            if (OleDataObjectHelper.GetDataPresentSafe(dataObject, MOZILLA_MARKER))
                return mozillaFileItemFormats;
            else
                return fileItemFormats;

        }
        private const string MOZILLA_MARKER = "text/_moz_htmlinfo";

        // underlying data object
        private IDataObject m_dataObject = null;

        // file item format
        private IFileItemFormat m_fileItemFormat = null;

        // list of files
        private FileItem[] m_files = null;

        // Helper to verify that our IFileItemFormat instances are mutually
        // exclusive (more than one format shouldn't be supported by the
        // data inside a single IDataObject)
        [Conditional("DEBUG")]
        private static void VerifyFormatsMutuallyExclusive(IDataObject iDataObject)
        {
            // verify there is no case where multiple file items types can
            // be created from the same IDataObject (just a sanity/assumption check)
            int matches = 0;
            StringBuilder sb = new StringBuilder();
            foreach (IFileItemFormat format in fileItemFormats)
                if (format.CanCreateFrom(iDataObject))
                {
                    matches++;
                    sb.AppendLine(format.ToString());
                }
            Debug.Assert(matches == 1,
                "More than 1 file item format can be created from a data object " +
                "(potential ambiguity / order dependency\r\n" + sb.ToString());

        }
    }

}
