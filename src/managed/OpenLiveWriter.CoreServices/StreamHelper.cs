// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using JetBrains.Annotations;

    /// <summary>
    /// Class StreamHelper.
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Transfers the specified in stream.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="outStream">The out stream.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="flush">if set to <c>true</c> [flush].</param>
        /// <returns>A long.</returns>
        public static long Transfer(
            [NotNull] Stream inStream,
            [NotNull] Stream outStream,
            int bufferSize = 8192,
            bool flush = true)
        {
            long totalBytes = 0;
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = inStream.Read(buffer, 0, bufferSize)) != 0)
            {
                if (bytesRead < 0)
                {
                    Debug.Fail("bytesRead was negative! " + bytesRead);
                    break;
                }

                outStream.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }

            if (flush)
            {
                outStream.Flush();
            }

            return totalBytes;
        }

        /// <summary>
        /// Transfers the incremental.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="outStream">The out stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="byteCount">The byte count.</param>
        /// <returns>True if something was transferred, false if transferring is complete.</returns>
        public static bool TransferIncremental(
            [NotNull] Stream inStream,
            [NotNull] Stream outStream,
            [CanBeNull] ref byte[] buffer,
            out long byteCount)
        {
            if (buffer == null)
            {
                buffer = new byte[8192];
            }

            var bytesRead = inStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                byteCount = 0;
                return false;
            }

            outStream.Write(buffer, 0, bytesRead);
            byteCount = bytesRead;
            return true;
        }

        /// <summary>
        /// Read and discard the contents of the stream until EOF.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static void DiscardRest([NotNull] Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                var buf = new byte[4096];
                while (stream.Read(buf, 0, buf.Length) != 0)
                {
                    // do nothing
                }
            }
        }

        /// <summary>
        /// Converts a stream to a byte array by reading it.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <returns>A System.Byte[].</returns>
        [NotNull]
        public static byte[] AsBytes([NotNull] Stream inStream)
        {
            var outStr = new MemoryStream();
            StreamHelper.Transfer(inStream, outStr);
            return outStr.ToArray();
        }

        /// <summary>
        /// Reads a stream and returns a string in the given encoding.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="enc">The encoding.</param>
        /// <returns>A System.String.</returns>
        [CanBeNull]
        public static string AsString([NotNull] Stream inStream, [NotNull] Encoding enc)
        {
            using (var reader = new StreamReader(inStream, enc))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Return up to <c>count</c> bytes from the stream.
        /// If EOF has been reached, returns null. Otherwise,
        /// returns a byte array exactly long enough to hold
        /// the actual number of bytes read (will not exceed
        /// <c>count</c>).
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="count">The count.</param>
        /// <returns>A System.Byte[].</returns>
        [NotNull]
        public static byte[] AsBytes([NotNull] Stream inStream, int count)
        {
            var buf = new byte[count];
            var bytesRead = inStream.Read(buf, 0, count);

            if (bytesRead == buf.Length)
            {
                return buf;
            }

            if (bytesRead == -1)
            {
                return new byte[] { };
            }

            var shortBuf = new byte[bytesRead];
            Array.Copy(buf, shortBuf, shortBuf.Length);
            return shortBuf;
        }

        /// <summary>
        /// Returns a stream for the given byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A Stream.</returns>
        [NotNull]
        public static Stream AsStream([NotNull] byte[] data)
        {
            var inStr = new MemoryStream(data);
            return inStr;
        }

        /// <summary>
        /// Copies to memory stream.
        /// </summary>
        /// <param name="s">The source stream.</param>
        /// <returns>The destination stream.</returns>
        [NotNull]
        public static Stream CopyToMemoryStream([NotNull] Stream s)
        {
            if (s.CanSeek)
            {
                // If we can find out the stream length, we can copy it in a way
                // that only results in a single byte array getting instantiated.
                return new MemoryStream(StreamHelper.AsBytes(s, checked((int)s.Length)));
            }

            var memStream = new MemoryStream();
            StreamHelper.Transfer(s, memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        /// <summary>
        /// Returns the set of bytes between a sequence of start and end bytes (including the start/end bytes).
        /// </summary>
        /// <param name="startBytes">The start bytes.</param>
        /// <param name="endBytes">The end bytes.</param>
        /// <param name="s">The stream.</param>
        /// <returns>A System.Byte[].</returns>
        [NotNull]
        public static byte[] ExtractByteRegion(
            [NotNull] byte[] startBytes,
            [NotNull] byte[] endBytes,
            [NotNull] Stream s)
        {
            var b = s.ReadByte();
            long startIndex = -1;
            while (b != -1 && startIndex == -1)
            {
                if (b == startBytes[0])
                {
                    var position = s.Position;
                    var tokenMaybeFound = true;
                    for (var i = 1; tokenMaybeFound && i < startBytes.Length; i++)
                    {
                        b = s.ReadByte();
                        tokenMaybeFound = b == startBytes[i];
                    }

                    if (tokenMaybeFound)
                    {
                        b = s.ReadByte(); // move past the last byte in the startBytes
                        startIndex = position;
                    }
                    else
                    {
                        // move back to the last known good position
                        s.Seek(position, SeekOrigin.Begin);
                        b = s.ReadByte(); // advance to the next byte
                    }
                }
                else
                {
                    b = s.ReadByte();
                }
            }

            if (startIndex == -1)
            {
                return new byte[0];
            }

            using (var memStream = new MemoryStream())
            {
                memStream.Write(startBytes, 0, startBytes.Length);
                while (b != -1)
                {
                    memStream.WriteByte((byte)b);
                    if (b == endBytes[0])
                    {
                        var tokenMaybeFound = true;
                        for (var i = 1; tokenMaybeFound && i < endBytes.Length; i++)
                        {
                            b = s.ReadByte();
                            memStream.WriteByte((byte)b);
                            tokenMaybeFound = b == endBytes[i];
                        }

                        if (tokenMaybeFound)
                        {
                            break;
                        }
                    }

                    b = s.ReadByte();
                }

                return memStream.ToArray();
            }
        }
    }
}
