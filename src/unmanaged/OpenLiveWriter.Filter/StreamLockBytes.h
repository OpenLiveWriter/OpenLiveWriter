// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// StreamLockBytes.h : Declaration of the CStreamLockBytes

#pragma once
#include "resource.h"       // main symbols

#include "OpenLiveWriter.Filter.h"


// CStreamLockBytes

class ATL_NO_VTABLE CStreamLockBytes : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStreamLockBytes, &CLSID_StreamLockBytes>,
	public IStreamLockBytes,
	public ILockBytes
{
public:
	CStreamLockBytes()
	{
	}

#pragma warning ( disable : 4995 )
DECLARE_REGISTRY_RESOURCEID(IDR_STREAMLOCKBYTES)
#pragma warning ( default : 4995 )

DECLARE_NOT_AGGREGATABLE(CStreamLockBytes)

BEGIN_COM_MAP(CStreamLockBytes)
	COM_INTERFACE_ENTRY(IStreamLockBytes)
	COM_INTERFACE_ENTRY(ILockBytes)
END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}
	
	void FinalRelease() 
	{
	}

	CComPtr<IStream> _pStream;

public:
	// Init the StreamLockBytes with the underlying stream
	virtual HRESULT STDMETHODCALLTYPE Init(IStream *pStream);

	virtual /* [local] */ HRESULT STDMETHODCALLTYPE ReadAt( 
        /* [in] */ ULARGE_INTEGER ulOffset,
        /* [length_is][size_is][out] */ void *pv,
        /* [in] */ ULONG cb,
        /* [out] */ ULONG *pcbRead);
    
    virtual /* [local] */ HRESULT STDMETHODCALLTYPE WriteAt( 
        /* [in] */ ULARGE_INTEGER ulOffset,
        /* [size_is][in] */ const void *pv,
        /* [in] */ ULONG cb,
        /* [out] */ ULONG *pcbWritten);
    
    virtual HRESULT STDMETHODCALLTYPE Flush( void);
    
    virtual HRESULT STDMETHODCALLTYPE SetSize( 
        /* [in] */ ULARGE_INTEGER cb);
    
    virtual HRESULT STDMETHODCALLTYPE LockRegion( 
        /* [in] */ ULARGE_INTEGER libOffset,
        /* [in] */ ULARGE_INTEGER cb,
        /* [in] */ DWORD dwLockType);
    
    virtual HRESULT STDMETHODCALLTYPE UnlockRegion( 
        /* [in] */ ULARGE_INTEGER libOffset,
        /* [in] */ ULARGE_INTEGER cb,
        /* [in] */ DWORD dwLockType);
    
    virtual HRESULT STDMETHODCALLTYPE Stat( 
        /* [out] */ STATSTG *pstatstg,
        /* [in] */ DWORD grfStatFlag);
};

OBJECT_ENTRY_AUTO(__uuidof(StreamLockBytes), CStreamLockBytes)
