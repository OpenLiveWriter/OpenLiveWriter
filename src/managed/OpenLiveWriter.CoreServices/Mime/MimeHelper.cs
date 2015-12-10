// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for mimeHelper.
    ///
    ///  Mime-types was originally defined in RFC 1341. After that a series of
    ///  improvment has been made. Some of this can be found in RFC 1521
    ///  and RFC 1522.
    /// </summary>
    public class MimeHelper
    {
        /// <summary>
        /// This takes a file extension and returns the mime content type for
        /// the file
        /// </summary>
        /// <param name="fileExtension">the file extension (i.e. '.gif')</param>
        /// <returns>the content type (i.e. 'image/gif').  returns null if no contentType
        /// is found for the file extension.</returns>
        public static string GetContentType(string fileExtension)
        {
            return GetContentType(fileExtension, null);
        }

        /// <summary>
        /// This sets the content type value in the registry for a given file
        /// extension
        /// </summary>
        /// <param name="fileExtension">the file extension (i.e. '.gif')</param>
        /// <param name="contentType">the content type (i.e. 'image/gif')</param>
        public static void SetContentType(string fileExtension, string contentType)
        {
            // try to read the registry, throw mimehelper exception if it fails
            try
            {
                RegistryKey extKey = Registry.ClassesRoot.CreateSubKey(fileExtension);
                Debug.Assert(extKey != null, "Content Type Not Set");

                using (extKey)
                {
                    extKey.SetValue(CONTENT_TYPE_KEY, contentType);
                }
            }
            catch (Exception e)
            {
                throw MimeHelperException.ForUnableToSetContentType(contentType, e);
            }

        }

        /// <summary>
        /// Application/octet-stream content type, a good default for
        /// unrecognized content types.
        /// </summary>
        public const string APP_OCTET_STREAM = "application/octet-stream";

        /// <summary>
        /// Text/html content type
        /// </summary>
        public const string TEXT_HTML = "text/html";

        public const string TEXT_XML = "text/xml";

        /// <summary>
        /// Text/plain content type
        /// </summary>
        public const string TEXT_PLAIN = "text/plain";

        public const string IMAGE_JPG = "image/jpeg";

        public const string IMAGE_GIF = "image/gif";

        public const string IMAGE_PNG = "image/png";

        public const string IMAGE_TIF = "image/tiff";

        public const string IMAGE_BMP = "image/bmp";

        public const string IMAGE_XPNG = "image/x-png";

        public const string IMAGE_ICON = "image/x-icon";

        public const string CONTENTTYPE_UNKNOWN = "application/x-internal-unknown-type";

        /// <summary>
        /// This takes a file extension and returns the mime content type for
        /// the file.  Returns the default if no content type is found.
        /// </summary>
        /// <param name="fileExtension">the file extension (i.e. '.gif')</param>
        /// <param name="defaultValue">the content type value to be returned if no
        /// content type can be located for this file extension</param>
        /// <returns>the content type (i.e. 'image/gif')</returns>
        public static string GetContentType(string fileExtension, string defaultValue)
        {
            // Sometimes registries get corrupted and don't contain these
            // values, which causes Blogger image uploads to be rejected.
            switch (fileExtension.ToUpperInvariant())
            {
                case ".GIF":
                    return "image/gif";
                case ".JPE":
                case ".JPEG":
                case ".JPG":
                    return "image/jpeg";
                case ".PNG":
                    return "image/png";
            }

            // ENHANCEMENT: Some extension (.js, .ashx, .css) may slip through and
            // be base64 encoded even though they're text.  We could force those to be
            // QP encoded using a static hash of extensions...

            // Open the registry key for the extension and return the
            // content type, if possible
            RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(fileExtension, false);
            if (extKey != null)
            {
                using (extKey)
                {

                    object contentType = extKey.GetValue(CONTENT_TYPE_KEY, defaultValue);

                    // If there isn't a content type key for this file type
                    if (contentType == null)
                        return defaultValue;
                    else
                        return contentType.ToString();
                }
            }
            else
                return defaultValue;

        }

        /// <summary>
        /// Gets the appropriate extension for a file based upon its content type.
        /// </summary>
        /// <param name="contentType">The content type for which to locate the extension</param>
        /// <returns>The extension including the '.', null if no extension could be found</returns>
        public static string GetExtensionFromContentType(string contentType)
        {
            string extension = null;
            using (RegistryKey mimeKey = Registry.ClassesRoot.OpenSubKey("MIME\\Database\\Content Type"))
            {
                string[] subKeys = mimeKey.GetSubKeyNames();
                if (new ArrayList(subKeys).Contains(contentType))
                {
                    extension = (string)mimeKey.OpenSubKey(contentType).GetValue(EXTENSION);
                }
            }
            return extension;
        }
        private const string MIME_DATABASE_CONTENTTYPE = "MIME\\Database\\Content Type";
        private const string EXTENSION = "Extension";

        //
        // The below are an enumeration of the available encoding types
        //

        /// <summary>
        /// Base 64 encoding
        /// </summary>
        public const string Base64 = "base64";

        /// <summary>
        /// 7bit Encoding (used only for low ascii plain text)
        /// </summary>
        public const string LowAscii = "7Bit";

        /// <summary>
        /// Quoted-printable encoding (used for all text)
        /// </summary>
        public const string QuotedPrintable = "quoted-printable";

        /// <summary>
        /// Gets the encoding type to use for this content type
        /// </summary>
        /// <param name="contentType">contentType to find encoding for</param>
        /// <returns>encoding type</returns>
        public static string GetEncodingType(string contentType, Stream input)
        {
            // if we recognize the mime type as text, use quoted printable
            // which is more efficient for text
            if (contentType.Trim().StartsWith("text", StringComparison.OrdinalIgnoreCase))
            {
                string encodingType = MimeHelper.QuotedPrintable;

                // Look for any unicode characters
                StreamReader reader = new StreamReader(input);
                char[] chars = new char[1];
                int ch;
                while (reader.Peek() != -1)
                {
                    ch = reader.Read();
                    if (ch > 127)
                    {
                        encodingType = MimeHelper.Base64;
                        break;
                    }
                }
                input.Position = 0;

                //return encodingType;
                return encodingType;
            }
            // if we're not sure its text, use base64, which can handle anything
            // but is less efficient for text encoding
            else
            {
                return MimeHelper.Base64;
            }

        }

        /// <summary>
        /// Write base64 encoded data
        /// </summary>
        /// <param name="input">the stream to base64 encode</param>
        /// <param name="output">the output stream to write the MIME header to</param>
        public static void EncodeBase64(Stream input, Stream output)
        {
            // Get a writer (use ASCII encoding since this will write 7 bit ascii text)
            StreamWriter writer = new StreamWriter(output, ASCIIEncoding.ASCII);

            // Each 3 byte sequence in the source data becomes a 4 byte
            // sequence in the character array. (so the byte buffer is 3/4 the
            // size of a line of characters
            const int BUFFER_SIZE = (int)(MimeHelper.MAX_LINE_LENGTH * (3.0d / 4.0d));

            // Set up the buffers for read/write operations
            byte[] unEncodedBytes = new byte[BUFFER_SIZE];
            char[] encodedChars = new char[MimeHelper.MAX_LINE_LENGTH];

            // do the encoding
            int bytesRead = 0;
            do
            {
                // put the bytes into the input stream
                bytesRead = input.Read(unEncodedBytes, 0, BUFFER_SIZE);

                if (bytesRead > 0)
                {
                    // encode the bytes into the character array
                    int charsEncoded = Convert.ToBase64CharArray(
                            unEncodedBytes, 0, bytesRead, encodedChars, 0);

                    // write the line to output
                    writer.WriteLine(encodedChars, 0, charsEncoded);
                }

            } while (bytesRead == BUFFER_SIZE);

            // Flush this writer to make sure any internal buffer is empty.
            writer.Flush();

        }

        /// <summary>
        /// Writes quoted printable encoded characters to the output stream
        /// Uses a state table defined in QpStateTable to drive the encoding.
        /// </summary>
        /// <param name="input">stream to encode</param>
        /// <param name="output">streamwriter to write to</param>
        public static void EncodeQuotedPrintable(Stream input, Stream output)
        {
            // Get a reader and writer on the streams
            // (use ASCII encoding for the writer since this will write 7 bit ascii text)
            StreamReader reader = new StreamReader(input);
            StreamWriter writer = new StreamWriter(output, ASCIIEncoding.ASCII);

            // init line position and current state
            int linePosition = 1;
            int currentState = SCANNING;

            // do the encoding
            int ch = reader.Peek();
            while (ch != -1)
            {
                // get the character type
                int charType = GetQpCharType(ch);

                // get the actions for the current state and character type
                int actions = QPStateTable[currentState, charType, ACTION];

                // execute the actions for this character
                if ((actions & OUTPUT) > 0)
                {
                    writer.Write((char)ch);
                    linePosition += 1;
                }

                if ((actions & OUTPUT_ESCAPED) > 0)
                {
                    writer.Write("={0:X2}", ch);        // the format :X2 forces to hex encoding w/ a minimum of 2 characters
                    linePosition += 3;
                }

                if ((actions & OUTPUT_SOFT_CRLF) > 0)
                {
                    writer.Write("=\r\n");
                    linePosition = 1;
                }

                if ((actions & OUTPUT_CRLF) > 0)
                {
                    writer.Write("\r\n");
                    linePosition = 1;
                }

                // If there is no putback, read the character to dispose of it
                // If there is a putback, do nothing, leaving the character in the stream
                if ((actions & PUTBACK) == 0)
                {
                    reader.Read();
                }

                // Get the new state
                currentState = QPStateTable[currentState, charType, NEW_STATE];

                // If the state is pending, we need to use the line position
                // to determine what the new state should be be.  The red zone
                // means we're near the end of a line (76 characters wide) and
                // so need to be careful.
                if (currentState == PENDING)
                {
                    if (linePosition >= REDZONE_LINE_POSITION)
                        currentState = REDZONE;
                    else
                        currentState = SCANNING;
                }

                // Lookahead to the next character
                ch = reader.Peek();

            }
            // Flush this writer to make sure any internal buffer is empty.
            writer.Flush();

        }

        /// <summary>
        /// Determines whether the given content type is an image
        /// </summary>
        /// <param name="contentType">The content type string</param>
        /// <returns>true if it is an image, otherwise false</returns>
        public static bool IsContentTypeImage(string contentType)
        {
            if (contentType == null)
                return false;

            foreach (string imageContentType in m_imageContentTypes)
                if (contentType.StartsWith(imageContentType, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether the given content type is a page
        /// </summary>
        /// <param name="contentType">The content type string</param>
        /// <returns>true if it is a page, otherwise false</returns>
        public static bool IsContentTypeWebPage(string contentType)
        {
            if (contentType == null)
                return false;

            foreach (string webPageContentType in m_webPageContentTypes)
                if (contentType.StartsWith(webPageContentType, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether the given content type is a document (this is a loose definition based on our experience)
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <returns>true if it is a document, otherwise false</returns>
        public static bool IsContentTypeDocument(string contentType)
        {
            if (contentType == null)
                return false;

            foreach (string documentContentType in m_documentContentTypes)
                if (contentType.StartsWith(documentContentType, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether the given content type is a file
        /// </summary>
        /// <param name="contentType">The content type string</param>
        /// <returns>true if it is a file, otherwise false</returns>
        public static bool IsContentTypeFile(string contentType)
        {
            if (contentType == null)
                return false;

            return !(IsContentTypeImage(contentType) || IsContentTypeWebPage(contentType)) && (contentType != CONTENTTYPE_UNKNOWN);
        }

        private static string[] m_imageContentTypes = new string[] { MimeHelper.IMAGE_GIF, MimeHelper.IMAGE_JPG, MimeHelper.IMAGE_PNG, MimeHelper.IMAGE_XPNG, MimeHelper.IMAGE_TIF, MimeHelper.IMAGE_ICON, "image/x-xbitmap" };
        private static string[] m_webPageContentTypes = new string[] { "text/html", "html document", "application/x-httpd-php", "application/xhtml+xml", "htm file" };
        private static string[] m_documentContentTypes =
            new string[] {  "application/pdf",
                            "application/postscript",
                            "application/msword",
                            "application/x-msexcel",
                            "application/x-mspowerpoint",
                            "application/vnd.ms-excel",
                            "application/vnd.ms-powerpoint",
                            "application/vnd.visio",
                            "text/rtf",
                            "application/rtf",
                            "application/vnd.ms-publisher",
                            "application/x-mspublisher",
                            "text/plain"};

        /// <summary>
        /// Returns the quoted printable character type for a given chacacter
        /// </summary>
        /// <param name="ascii">the character to determine type</param>
        /// <returns>the character type</returns>
        private static int GetQpCharType(int ascii)
        {
            // legal ascii-
            if ((ascii >= 33 && ascii <= 60) ||
                (ascii >= 62 && ascii <= 126) ||
                (ascii == 0))
                return LEGAL_ASCII;

            // tab or space
            else if (ascii == 9 || ascii == 32)
                return WHITESPACE;

            // lineFeed
            else if (ascii == 10)
                return LINEFEED;

            // carriage Return
            else if (ascii == 13)
                return CARRIAGE_RETURN;

            // high or low illegal ascii value
            else if ((ascii < 255 && ascii > 126) || (ascii > 0 && ascii < 33) || (ascii == 61))
                return ILLEGAL_ASCII;

            // ascii value that is unsupportable by this encoding
            else
                // Throw illegal character exception
                throw MimeHelperException.ForIllegalEncodingCharacter((char)ascii, "quoted-printable", null);
        }

        // The states, actions, and inputs used in the quoted printable encoder
        #region Quoted Printable State Table

        /// states
        private const int SCANNING = 0;
        private const int LOOK_FOR_LF = 1;
        private const int REDZONE = 2;
        private const int END_OF_LINE = 3;
        private const int PENDING = 4;

        ///  inputs
        private const int LEGAL_ASCII = 0;
        private const int ILLEGAL_ASCII = 1;
        private const int WHITESPACE = 2;
        private const int CARRIAGE_RETURN = 3;
        private const int LINEFEED = 4;

        /// actions
        private const int NONE = 0;
        private const int OUTPUT = 1;
        private const int OUTPUT_ESCAPED = 2;
        private const int OUTPUT_SOFT_CRLF = 4;
        private const int OUTPUT_CRLF = 8;
        private const int PUTBACK = 16;

        /// aggregate actions
        private const int CRLF_AND_PUTBACK = OUTPUT_CRLF | PUTBACK;
        private const int SOFT_CRLF_AND_PUTBACK = OUTPUT_SOFT_CRLF | PUTBACK;

        /// action / state pair
        private const int ACTION = 0;
        private const int NEW_STATE = 1;

        /// determines which line position begins the redzone
        private const int REDZONE_LINE_POSITION = MAX_LINE_LENGTH - 3;

        /// <summary>
        /// This is the overall state table for the quoted printable encoding
        /// </summary>
        private static int[,,] QPStateTable =
            {
                // SCANNING
                {
                    {OUTPUT,PENDING},			// Legal Ascii
                    {OUTPUT_ESCAPED,PENDING},	// Illegal Ascii
                    {OUTPUT,PENDING},			// WriteSpace
                    {NONE,LOOK_FOR_LF},		// CarriageReturn
                    {OUTPUT_CRLF, SCANNING}		// Linefeed
                },

                // LOOK_FOR_LF
                {
                    {CRLF_AND_PUTBACK, SCANNING},	// Legal Ascii
                    {CRLF_AND_PUTBACK, SCANNING},	// Illegal Ascii
                    {CRLF_AND_PUTBACK, SCANNING},	// WhiteSpace
                    {CRLF_AND_PUTBACK, SCANNING},	// CarriageReturn
                    {OUTPUT_CRLF, SCANNING}			// Linefeed
                },

                // REDZONE
                {
                    {OUTPUT, END_OF_LINE},			// Legal Ascii
                    {OUTPUT_ESCAPED, END_OF_LINE},	// Illegal Ascii
                    {OUTPUT_ESCAPED, END_OF_LINE},	// WhiteSpace
                    {NONE, LOOK_FOR_LF},			// CarriageReturn
                    {OUTPUT_CRLF, SCANNING}			// Linefeed
                },

                // END_OF_LINE
                {
                    {SOFT_CRLF_AND_PUTBACK, SCANNING},	// Legal Ascii
                    {SOFT_CRLF_AND_PUTBACK, SCANNING},	// Illegal Ascii
                    {SOFT_CRLF_AND_PUTBACK, SCANNING},	// WhiteSpace
                    {NONE, LOOK_FOR_LF},				// CarriageReturn
                    {OUTPUT_CRLF, SCANNING}				// Linefeed
                }
            };
        #endregion

        /// <summary>
        /// Parses content-type header values of the form
        ///		mainValue;param1=value1;param2=value2
        /// but taking into account folding whitespace, comments, quoting,
        /// quoted-char, and all the other stuff that RFC2045/RFC2822
        /// force us to deal with.
        ///
        /// To get the mainValue, look for the key "" in the result table.
        /// </summary>
        public static IDictionary ParseContentType(string contentType, bool caseInsensitive)
        {
            IDictionary result = new Hashtable();

            // get main value
            string[] mainAndRest = contentType.Split(new char[] { ';' }, 2);
            result[""] = mainAndRest[0].Trim().ToLower(CultureInfo.InvariantCulture);
            if (mainAndRest.Length > 1)
            {
                char[] chars = mainAndRest[1].ToCharArray();
                StringBuilder paramName = new StringBuilder();
                StringBuilder paramValue = new StringBuilder();

                const byte READY_FOR_NAME = 0;
                const byte IN_NAME = 1;
                const byte READY_FOR_VALUE = 2;
                const byte IN_VALUE = 3;
                const byte IN_QUOTED_VALUE = 4;
                const byte VALUE_DONE = 5;
                const byte ERROR = 99;

                byte state = READY_FOR_NAME;
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];

                    switch (state)
                    {
                        case ERROR:
                            if (c == ';')
                                state = READY_FOR_NAME;
                            break;

                        case READY_FOR_NAME:
                            if (c == '=')
                            {
                                Debug.Fail("Expected header param name, got '='");
                                state = ERROR;
                                break;
                            }

                            paramName = new StringBuilder();
                            paramValue = new StringBuilder();

                            state = IN_NAME;
                            goto case IN_NAME;

                        case IN_NAME:
                            if (c == '=')
                            {
                                if (paramName.Length == 0)
                                    state = ERROR;
                                else
                                    state = READY_FOR_VALUE;
                                break;
                            }

                            // not '='... just add to name
                            paramName.Append(c);
                            break;

                        case READY_FOR_VALUE:
                            bool gotoCaseInValue = false;
                            switch (c)
                            {
                                case ' ':
                                case '\t':
                                    break;  // ignore spaces, especially before '"'

                                case '"':
                                    state = IN_QUOTED_VALUE;
                                    break;

                                default:
                                    state = IN_VALUE;
                                    gotoCaseInValue = true;
                                    break;
                            }
                            if (gotoCaseInValue)
                                goto case IN_VALUE;
                            else
                                break;

                        case IN_VALUE:
                            switch (c)
                            {
                                case ';':
                                    result[paramName.ToString().Trim()] = paramValue.ToString().Trim();
                                    state = READY_FOR_NAME;
                                    break;

                                default:
                                    paramValue.Append(c);
                                    break;
                            }
                            break;

                        case IN_QUOTED_VALUE:
                            switch (c)
                            {
                                case '"':
                                    // don't trim quoted strings; the spaces could be intentional.
                                    result[paramName.ToString().Trim()] = paramValue.ToString();
                                    state = VALUE_DONE;
                                    break;

                                case '\\':
                                    // quoted-char
                                    if (i == chars.Length - 1)
                                    {
                                        state = ERROR;
                                    }
                                    else
                                    {
                                        i++;
                                        paramValue.Append(chars[i]);
                                    }
                                    break;

                                default:
                                    paramValue.Append(c);
                                    break;
                            }
                            break;

                        case VALUE_DONE:
                            switch (c)
                            {
                                case ';':
                                    state = READY_FOR_NAME;
                                    break;

                                case ' ':
                                case '\t':
                                    break;

                                default:
                                    state = ERROR;
                                    break;
                            }
                            break;
                    }
                }

                // done looping.  check if anything is left over.
                if (state == IN_VALUE || state == READY_FOR_VALUE)
                {
                    result[paramName.ToString().Trim()] = paramValue.ToString().Trim();
                }
            }

            if (caseInsensitive)
                return CollectionsUtil.CreateCaseInsensitiveHashtable(result);
            else
                return result;
        }

        /// <summary>
        /// The maximum length of a line in a Mime Multipart Document
        /// </summary>
        private const int MAX_LINE_LENGTH = 76;

        // The registry key that stores the content type value
        private const string CONTENT_TYPE_KEY = "Content Type";

    }

}
