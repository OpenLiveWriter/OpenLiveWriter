// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for TextData.
    /// </summary>
    public class TextData
    {

        /// <summary>
        /// Creates a TextData from an IDataObject
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create a TextData</param>
        /// <returns>The TextData, null if no TextData could be created.</returns>
        public static TextData Create(IDataObject iDataObject)
        {
            // Note: Using getData or getDataPresent here will sometimes return text
            // even when the text isn't in the list of formats exposed by getFormats
            foreach (string format in iDataObject.GetFormats())
            {
                if (format == DataFormats.UnicodeText || format == DataFormats.Text || format == DataFormats.StringFormat)
                {
                    // Suppress the text from an outlook data object, the text is useless
                    if (OleDataObjectHelper.GetDataPresentSafe(iDataObject, OUTLOOK_FORMAT_IGNORE_TEXT))
                        return null;

                    return new TextData(iDataObject);
                }
            }
            return null;
        }

        /// <summary>
        /// The title of the Text DataObject
        /// </summary>
        public string Title
        {
            get
            {
                if (m_title == null)
                    m_title = TextHelper.GetTitleFromText(Text, TextHelper.TITLE_LENGTH, TextHelper.Units.Characters);
                return m_title;
            }
        }
        private string m_title;

        /// <summary>
        /// The actual textual data from the IDataObject
        /// </summary>
        public string Text
        {
            get
            {
                if (m_text == null)
                {
                    m_text = (string)m_dataObject.GetData(DataFormats.UnicodeText);

                    if (m_text == null)
                        m_text = (string)m_dataObject.GetData(DataFormats.Text);

                    if (m_text == null)
                        m_text = (string)m_dataObject.GetData(DataFormats.StringFormat);

                    if (m_text == null)
                        m_text = string.Empty;
                }
                return m_text;
            }
        }
        private string m_text;

        /// <summary>
        /// Constructor for TextData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the TextData</param>
        private TextData(IDataObject iDataObject)
        {
            m_dataObject = iDataObject;
        }
        private IDataObject m_dataObject;

        private const string OUTLOOK_FORMAT_IGNORE_TEXT = "RenPrivateMessages";

    }

}
