// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Class which contains helper methods used for cracking CF_FILECONTENTS
    /// </summary>
    public class FileContentsHelper
    {
        /// <summary>
        /// Utility function to extract an array of file contents file descriptors from
        /// an IDataObject instance
        /// </summary>
        /// <param name="dataObject">data object to extract descriptors from</param>
        /// <returns>array of file descriptors</returns>
        public static FileDescriptor[] GetFileDescriptors(IDataObject dataObject)
        {
            // Use an OleDataObject
            OleDataObject oleDataObject = OleDataObject.CreateFrom(dataObject);
            if (oleDataObject == null)
            {
                const string message = "DataObject not valid for FileContents!";
                Debug.Fail(message);
                throw
                    new InvalidOperationException(message);
            }

            // Try to get the data as FileGroupDescriptorW then try to get it
            // as FileGroupDescriptor
            bool bFileNameIsWide;
            OleStgMediumHGLOBAL stgMedium = (OleStgMediumHGLOBAL)oleDataObject.GetData(
                                                                      DataFormatsEx.FileGroupDescriptorWFormat, TYMED.HGLOBAL);
            if (stgMedium != null)
            {
                bFileNameIsWide = true;
            }
            else
            {
                stgMedium = (OleStgMediumHGLOBAL)oleDataObject.GetData(
                                                      DataFormatsEx.FileGroupDescriptorFormat, TYMED.HGLOBAL);

                if (stgMedium != null)
                {
                    bFileNameIsWide = false;
                }
                else
                {
                    const string message = "File group descriptor not available!";
                    Debug.Fail(message);
                    throw
                        new InvalidOperationException(message);
                }
            }

            // Copy the descriptors
            using (stgMedium)
            {
                using (HGlobalLock globalMem = new HGlobalLock(stgMedium.Handle))
                {
                    // get a pointer to the count
                    IntPtr pCount = globalMem.Memory;

                    // determine the number of file descriptors
                    Int32 count = Marshal.ReadInt32(pCount);

                    // get a pointer to the descriptors (x64-safe pointer arithmetic)
                    IntPtr pDescriptors = IntPtr.Add(globalMem.Memory, Marshal.SizeOf(count));

                    // allocate the array of structures that will be returned
                    FileDescriptor[] descriptors = new FileDescriptor[count];

                    // determine the sizes of the various data elements
                    const int FILENAME_BUFFER_SIZE = 260;
                    int headerSize = Marshal.SizeOf(typeof(FILEDESCRIPTOR_HEADER));
                    int fileNameSize = bFileNameIsWide ? FILENAME_BUFFER_SIZE * 2 : FILENAME_BUFFER_SIZE;
                    int totalSize = headerSize + fileNameSize;

                    // iterate through the memory block copying the FILEDESCRIPTOR structures
                    for (int i = 0; i < count; i++)
                    {
                        // determine the addresses of the various data elements (x64-safe)
                        IntPtr pAddr = IntPtr.Add(pDescriptors, i * totalSize);
                        IntPtr pFileNameAddr = IntPtr.Add(pAddr, headerSize);

                        // copy the header
                        descriptors[i].header = (FILEDESCRIPTOR_HEADER)
                                                Marshal.PtrToStructure(pAddr, typeof(FILEDESCRIPTOR_HEADER));

                        // copy the file name (use Unicode or Ansi depending upon descriptor type)
                        if (bFileNameIsWide)
                            descriptors[i].fileName = Marshal.PtrToStringUni(pFileNameAddr);
                        else
                            descriptors[i].fileName = Marshal.PtrToStringAnsi(pFileNameAddr);
                    }

                    // return the descriptors
                    return descriptors;
                }
            }
        }

        /// <summary>
        /// TYMEDs supported by CF_FILECONTENTS
        /// </summary>
        private const TYMED FILE_CONTENTS_TYMEDS =
            TYMED.ISTORAGE | TYMED.ISTREAM | TYMED.FILE | TYMED.HGLOBAL;
    }

    /// <summary>
    /// Type used to describe FileContents files
    /// </summary>
    public struct FileDescriptor
    {
        /// <summary>
        /// Standard file metainfo
        /// </summary>
        public FILEDESCRIPTOR_HEADER header;

        /// <summary>
        /// Name of file
        /// </summary>
        public string fileName;
    }
}
