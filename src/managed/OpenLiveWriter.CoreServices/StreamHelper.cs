// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class StreamHelper
    {
        private StreamHelper()
        {
        }

        public static long Transfer(Stream inStream, Stream outStream)
        {
            return Transfer(inStream, outStream, 8192, true);
        }

        public static long Transfer(Stream inStream, Stream outStream, int bufferSize, bool flush)
        {
            long totalBytes = 0;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while (0 != (bytesRead = inStream.Read(buffer, 0, bufferSize)))
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
                outStream.Flush();

            return totalBytes;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inStream"></param>
        /// <param name="outStream"></param>
        /// <param name="buffer"></param>
        /// <returns>True if something was transferred, false if transferring is complete.</returns>
        public static bool TransferIncremental(Stream inStream, Stream outStream, ref byte[] buffer, out long byteCount)
        {
            if (buffer == null)
                buffer = new byte[8192];

            int bytesRead;
            bytesRead = inStream.Read(buffer, 0, buffer.Length);
            if (bytesRead != 0)
            {
                outStream.Write(buffer, 0, bytesRead);
                byteCount = bytesRead;
                return true;
            }
            else
            {
                byteCount = 0;
                return false;
            }
        }

        /// <summary>
        /// Read and discard the contents of the stream until EOF.
        /// </summary>
        public static void DiscardRest(Stream stream)
        {
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.End);
            else
            {
                byte[] buf = new byte[4096];
                while (stream.Read(buf, 0, buf.Length) != 0)
                    ;  // do nothing
            }
        }

        public static byte[] AsBytes(Stream inStream)
        {
            MemoryStream outStr = new MemoryStream();
            Transfer(inStream, outStr);
            return outStr.ToArray();
        }

        public static string AsString(Stream inStream, Encoding enc)
        {
            using (StreamReader reader = new StreamReader(inStream, enc))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Return up to <c>count</c> bytes from the stream.
        ///
        /// If EOF has been reached, returns null. Otherwise,
        /// returns a byte array exactly long enough to hold
        /// the actual number of bytes read (will not exceed
        /// <c>count</c>).
        /// </summary>
        public static byte[] AsBytes(Stream inStream, int count)
        {
            byte[] buf = new byte[count];
            int bytesRead = inStream.Read(buf, 0, count);

            if (bytesRead == buf.Length)
            {
                return buf;
            }
            else if (bytesRead == -1)
            {
                return null;
            }
            else
            {
                byte[] shortBuf = new byte[bytesRead];
                Array.Copy(buf, shortBuf, shortBuf.Length);
                return shortBuf;
            }
        }

        public static Stream AsStream(byte[] data)
        {
            MemoryStream inStr = new MemoryStream(data);
            return inStr;
        }

        public static Stream CopyToMemoryStream(Stream s)
        {
            if (s.CanSeek)
            {
                // If we can find out the stream length, we can copy it in a way
                // that only results in a single byte array getting instantiated.
                return new MemoryStream(AsBytes(s, checked((int)s.Length)));
            }
            else
            {
                MemoryStream memStream = new MemoryStream();
                Transfer(s, memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
        }

        /// <summary>
        /// Returns the set of bytes between a sequence of start and end bytes (including the start/end bytes).
        /// </summary>
        /// <param name="startBytes"></param>
        /// <param name="endBytes"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] ExtractByteRegion(byte[] startBytes, byte[] endBytes, Stream s)
        {
            int b = s.ReadByte();
            long startIndex = -1;
            while (b != -1 && startIndex == -1)
            {
                if (b == startBytes[0])
                {
                    long position = s.Position;
                    bool tokenMaybeFound = true;
                    for (int i = 1; tokenMaybeFound && i < startBytes.Length; i++)
                    {
                        b = s.ReadByte();
                        tokenMaybeFound = b == startBytes[i];
                    }
                    if (tokenMaybeFound)
                    {
                        b = s.ReadByte(); //move past the last byte in the startBytes
                        startIndex = position;
                    }
                    else
                    {
                        //move back to the last known good position
                        s.Seek(position, SeekOrigin.Begin);
                        b = s.ReadByte(); //advance to the next byte
                    }
                }
                else
                    b = s.ReadByte();
            }
            if (startIndex == -1)
                return new byte[0];

            MemoryStream memStream = new MemoryStream();
            memStream.Write(startBytes, 0, startBytes.Length);
            while (b != -1)
            {
                memStream.WriteByte((byte)b);
                if (b == endBytes[0])
                {
                    bool tokenMaybeFound = true;
                    for (int i = 1; tokenMaybeFound && i < endBytes.Length; i++)
                    {
                        b = s.ReadByte();
                        memStream.WriteByte((byte)b);
                        tokenMaybeFound = b == endBytes[i];
                    }
                    if (tokenMaybeFound)
                        break;
                }
                b = s.ReadByte();
            }
            return memStream.ToArray();
        }
    }
}
