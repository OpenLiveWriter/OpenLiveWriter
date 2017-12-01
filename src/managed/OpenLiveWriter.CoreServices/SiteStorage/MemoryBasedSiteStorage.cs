// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Implementation of ISiteStorage that uses memory as a backing store.
    /// </summary>
    public class MemoryBasedSiteStorage : SiteStorageBase
    {
        /// <summary>
        /// Do-nothing default constructor (initialize without specifying a RootFile).
        /// The RootFile must eventually be specified in order for the site to be valid.
        /// </summary>
        public MemoryBasedSiteStorage()
            : base()
        {
        }

        /// <summary>
        /// Initialize with the specified RootFile
        /// </summary>
        /// <param name="rootFile">Root file for the web site (e.g. index.htm)</param>
        public MemoryBasedSiteStorage(string rootFile)
            : base(rootFile)
        {
        }

        /// <summary>
        /// Test to see whether the specified file already exists
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>true if it exists, otherwise false</returns>
        public override bool Exists(string file)
        {
            // convert the path to lower-case
            string pathLower = file.ToLower(CultureInfo.InvariantCulture);

            // check if it exists
            return m_streams.ContainsKey(pathLower);
        }

        /// <summary>
        /// Retrieve a Stream for the given path (Read or Write access can be specified)
        /// Stream.Close() should be called when you are finished using the Stream.
        /// </summary>
        /// <param name="file">Hierarchical path designating stream location (uses "/" as
        /// path designator)</param>
        /// <param name="mode">Read or Write. Write will overwrite any existing path of
        /// the same name.</param>
        /// <returns>Stream that can be used to access the path (Stream.Close() must be
        /// called when you are finished using the Stream).</returns>
        public override Stream Open(string file, AccessMode mode)
        {
            // validate the path (throws an exception if it is invalid)
            ValidatePath(file);

            // convert the path to lower-case
            string pathLower = file.ToLower(CultureInfo.InvariantCulture);

            // return the appropriate stream
            switch (mode)
            {
                case AccessMode.Read:
                    return OpenMemoryStreamForRead(pathLower);

                case AccessMode.Write:
                    return OpenMemoryStreamForWrite(pathLower);

                default:
                    Debug.Assert(false, "Invalid AccessMode");
                    return null;
            }
        }

        /// <summary>
        /// Method called by base class SupportingFiles implementation
        /// </summary>
        /// <returns>New ArrayList containing the names all files in storage</returns>
        protected override ArrayList GetStoredFiles()
        {
            // copy the stream path names into an array list to be returned
            return new ArrayList(m_streams.Keys);
        }

        // open a stream for read access
        private Stream OpenMemoryStreamForRead(string path)
        {
            // check for the stream and return it
            if (m_streams.ContainsKey(path))
            {
                // grab the stream
                SiteStorageMemoryStream stream = m_streams[path] as SiteStorageMemoryStream;

                // confirm that the stream was closed prior to being opened for reading
                if (stream.FinalLength != -1)
                {
                    // return a new stream based on the buffer in the already
                    // written to stream
                    return new MemoryStream(stream.GetBuffer(), 0, stream.FinalLength, false);
                }

                // user writing the stream never closed it -- you can't read a stream that
                // has been written to until it has been closed
                else
                {
                    throw new SiteStorageException(
                        null, SiteStorageException.StreamNotClosed, path);
                }
            }
            else
            {
                // throw exception indicating we couldn't find the path
                throw new
                    SiteStorageException(null, SiteStorageException.PathNotFound, path);
            }
        }

        // open a stream for write access (overwrites existing stream if any)
        private Stream OpenMemoryStreamForWrite(string path)
        {
            // if the stream already exists then remove it
            // (issue: does memory 'leak' for a period of time here in the
            // absence of a very smart garbage collector?)
            if (m_streams.ContainsKey(path))
                m_streams.Remove(path);

            // create a new stream, store it away, and return a reference to it
            SiteStorageMemoryStream newStream = new SiteStorageMemoryStream();
            m_streams.Add(path, newStream);
            return newStream;
        }

        // hashtable containing paths and corresponding memory streams
        private Hashtable m_streams = new Hashtable();
    }

    /// <summary>
    /// Specialized version of MemoryStream that overrides the Close method and records the
    /// amount of data actually written to the stream. We do this so we can return a
    /// new MemoryStream based on the buffer in this one. Without recording the length of
    /// the stream it would be impossible to do this since once Close is called the only
    /// method that works on Stream is GetBuffer(). This allows us to get the buffer however
    /// since we don't know how much of the buffer is 'valid' and how much is garbage (extra
    /// unused buffer space) this won't work without explicitly specifying the part (length)
    /// of the buffer we are interested in.
    /// </summary>
    class SiteStorageMemoryStream : MemoryStream
    {
        /// <summary>
        /// Get the number of bytes contained in the stream. Returns -1
        /// if the stream is still open and thus doesn't have a final
        /// length recorded.
        /// </summary>
        public int FinalLength { get { return m_finalLength; } }

        /// <summary>
        /// Override of close method to record FinalLength
        /// </summary>
        public override void Close()
        {
            m_finalLength = Convert.ToInt32(Length);
            base.Close();
        }

        // length of the stream after it has been closed
        private int m_finalLength = -1;
    }

}
