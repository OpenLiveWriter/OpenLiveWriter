// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using STGMEDIUM = OpenLiveWriter.Interop.Com.STGMEDIUM;
using TYMED = OpenLiveWriter.Interop.Com.TYMED;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Abstract base class for OleStgMedium sub-types. Implements IDisposable
    /// so callers can release the underlying storage medium when they are
    /// finished with it. Callers MUST call either Release or Dispose when
    /// finished with the object because timely release of STGMEDIUM objects
    /// is critical --- this requirement is validated via an assertion in
    /// the destructor.
    /// </summary>
    public abstract class OleStgMedium : IDisposable
    {
        /// <summary>
        /// Create an OleStgMedium that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMedium(STGMEDIUM stg)
        {
            m_stg = stg;
        }

        /// <summary>
        /// Destructor uses an assertion to verify that the user called
        /// Release() or Dispose() when they are finished using the object
        /// </summary>
        ~OleStgMedium()
        {
            Debug.Assert(m_stg.tymed == TYMED.NULL,
                "You must call Release() or Dispose() on OleStgMedium when " +
                "finished using it!");
        }

        /// <summary>
        /// Release the underlying STGMEDIUM
        /// </summary>
        public virtual void Release()
        {
            if (m_stg.tymed != TYMED.NULL)
            {
                Ole32.ReleaseStgMedium(ref m_stg);
                m_stg.tymed = TYMED.NULL;
            }
        }

        /// <summary>
        /// Release the underlying STGMEDIUM
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// Helper function used by subclasses to validate that the type
        /// of the storage medium passed to them matches their type
        /// </summary>
        /// <param name="type">type of STGMEDIUM</param>
        protected void ValidateType(TYMED type)
        {
            if (m_stg.tymed != type)
            {
                const string msg = "Invalid TYMED passed to OleStgMedium";
                Debug.Assert(false, msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Underlying STGMEDIUM
        /// </summary>
        private STGMEDIUM m_stg;
    }

    /// <summary>
    /// Abstract base class for OleStgMedium types that are represented
    /// by a Win32 handle.
    /// </summary>
    public abstract class OleStgMediumHandle : OleStgMedium
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumHandle(STGMEDIUM stg)
            : base(stg)
        {
            m_handle = stg.contents;
        }

        /// <summary>
        /// Underlying Win32 handle
        /// </summary>
        public IntPtr Handle
        {
            get { return m_handle; }
        }
        private IntPtr m_handle;
    }

    /// <summary>
    /// OleStgMedium that contains an HGLOBAL
    /// </summary>
    public class OleStgMediumHGLOBAL : OleStgMediumHandle
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumHGLOBAL(STGMEDIUM stg)
            : base(stg)
        {
            ValidateType(TYMED.HGLOBAL);
        }
    }

    /// <summary>
    /// OleStgMedium that contains an HBITMAP
    /// </summary>
    public class OleStgMediumGDI : OleStgMediumHandle
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumGDI(STGMEDIUM stg)
            : base(stg)
        {
            ValidateType(TYMED.GDI);
        }
    }

    /// <summary>
    /// OleStgMedium that contains an standard metafile (HMETAFILE)
    /// </summary>
    public class OleStgMediumMFPICT : OleStgMediumHandle
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumMFPICT(STGMEDIUM stg)
            : base(stg)
        {
            ValidateType(TYMED.MFPICT);
        }
    }

    /// <summary>
    /// OleStgMedium that contains an enhanced metafile (HMETAFILE)
    /// </summary>
    public class OleStgMediumENHMF : OleStgMediumHandle
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumENHMF(STGMEDIUM stg)
            : base(stg)
        {
            ValidateType(TYMED.ENHMF);
        }
    }

    /// <summary>
    /// OleStgMedium that contains a file path
    /// </summary>
    public class OleStgMediumFILE : OleStgMedium
    {
        /// <summary>
        /// Create an OleStgMediumHandle that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumFILE(STGMEDIUM stg)
            : base(stg)
        {
            // validate that the correct type has been passed in
            ValidateType(TYMED.FILE);

            // marshall the file path into a .NET string
            m_path = Marshal.PtrToStringAuto(stg.contents);
        }

        /// <summary>
        /// Path to the file
        /// </summary>
        public string Path
        {
            get { return m_path; }
        }
        private string m_path;
    }

    /// <summary>
    /// Base class for OleStgMedium instances that are com objects
    /// </summary>
    public class OleStgMediumCOMOBJECT : OleStgMedium
    {
        /// <summary>
        /// Create an OleStgMediumCOMOBJECT that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumCOMOBJECT(STGMEDIUM stg)
            : base(stg)
        {
            // store the com object
            m_comObject = Marshal.GetObjectForIUnknown(stg.contents);
            Debug.Assert(m_comObject != null);
        }

        /// <summary>
        /// Underlying com object
        /// </summary>
        protected object m_comObject;
    }

    /// <summary>
    /// OleStgMedium that contains a stream
    /// </summary>
    public class OleStgMediumISTREAM : OleStgMediumCOMOBJECT
    {
        /// <summary>
        /// Create an OleStgMediumSTREAM that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumISTREAM(STGMEDIUM stg)
            : base(stg)
        {
            // validate that the correct type has been passed in
            ValidateType(TYMED.ISTREAM);

            // initialize the .NET stream
            m_stream = new ComStream((IStream)m_comObject);
        }

        /// <summary>
        /// Release the underlying STGMEDIUM (override)
        /// </summary>
        public override void Release()
        {
            // close the stream
            m_stream.Close();

            // release underlying storage
            base.Release();
        }

        /// <summary>
        /// Get the underlying stream as a .NET stream. The lifetime of the stream
        /// is tied to the lifetime of the OleStgMediumISTORAGE (it will be automatically
        /// disposed when the stg medium is disposed)
        /// </summary>
        public Stream Stream
        {
            get { return m_stream; }
        }
        private Stream m_stream = null;
    }

    /// <summary>
    /// OleStgMedium that contains a storage
    /// </summary>
    public class OleStgMediumISTORAGE : OleStgMediumCOMOBJECT
    {
        /// <summary>
        /// Create an OleStgMediumSTORAGE that encapsulates the passed STGMEDIUM
        /// </summary>
        /// <param name="stg">Underlying STGMEDIUM</param>
        public OleStgMediumISTORAGE(STGMEDIUM stg)
            : base(stg)
        {
            // validate that the correct type has been passed in
            ValidateType(TYMED.ISTORAGE);

            // initialize the storage
            m_storage = new Storage((IStorage)m_comObject, false);
        }

        /// <summary>
        /// Release the underlying STGMEDIUM (override)
        /// </summary>
        public override void Release()
        {
            // close the structured storage
            m_storage.Close();

            // release underlying storage
            base.Release();
        }

        /// <summary>
        /// Get the underlying storage as a .NET storage. The lifetime of the storage
        /// is tied to the lifetime of the OleStgMediumISTORAGE (it will be
        /// automatically disposed when the stg medium is disposed)
        /// </summary>
        public Storage Storage
        {
            get { return m_storage; }
        }
        private Storage m_storage = null;
    }

}
