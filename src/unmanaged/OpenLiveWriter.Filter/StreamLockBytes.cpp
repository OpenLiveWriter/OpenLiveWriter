// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// StreamLockBytes.cpp : Implementation of CStreamLockBytes

#include "stdafx.h"
#include "StreamLockBytes.h"


// CStreamLockBytes

HRESULT STDMETHODCALLTYPE CStreamLockBytes::Init(IStream *pStream)
{
	if (!pStream)
		return E_POINTER;
	_pStream = pStream;
	return S_OK;
}

LARGE_INTEGER ConvertToLargeInteger(const ULARGE_INTEGER& ulVal)
{
	LARGE_INTEGER lVal = {ulVal.LowPart, ulVal.HighPart};
	return lVal;
}

/* [local] */ HRESULT STDMETHODCALLTYPE CStreamLockBytes::ReadAt( 
            /* [in] */ ULARGE_INTEGER ulOffset,
            /* [length_is][size_is][out] */ void *pv,
            /* [in] */ ULONG cb,
            /* [out] */ ULONG *pcbRead)
{
	if (!_pStream.p)
		return E_POINTER;

	HRESULT hr = _pStream->Seek(ConvertToLargeInteger(ulOffset), STREAM_SEEK_SET, 0);
	if (FAILED(hr))
		return hr;
	return _pStream->Read(pv, cb, pcbRead);
}
        
/* [local] */ HRESULT STDMETHODCALLTYPE CStreamLockBytes::WriteAt( 
            /* [in] */ ULARGE_INTEGER ulOffset,
            /* [size_is][in] */ const void *pv,
            /* [in] */ ULONG cb,
            /* [out] */ ULONG *pcbWritten)
{
	if (!_pStream.p)
		return E_POINTER;

	HRESULT hr = _pStream->Seek(ConvertToLargeInteger(ulOffset), STREAM_SEEK_SET, 0);
	if (FAILED(hr))
		return hr;

	return _pStream->Write(pv, cb, pcbWritten);
}
        
HRESULT STDMETHODCALLTYPE CStreamLockBytes::Flush( void)
{
	if (!_pStream.p)
		return E_POINTER;

	return _pStream->Commit(STGC_DEFAULT);
}
        
HRESULT STDMETHODCALLTYPE CStreamLockBytes::SetSize( 
            /* [in] */ ULARGE_INTEGER cb)
{
	if (!_pStream.p)
		return E_POINTER;

	return _pStream->SetSize(cb);
}
        
HRESULT STDMETHODCALLTYPE CStreamLockBytes::LockRegion( 
            /* [in] */ ULARGE_INTEGER libOffset,
            /* [in] */ ULARGE_INTEGER cb,
            /* [in] */ DWORD dwLockType)
{
	if (!_pStream.p)
		return E_POINTER;

	return _pStream->LockRegion(libOffset, cb, dwLockType);
}
        
HRESULT STDMETHODCALLTYPE CStreamLockBytes::UnlockRegion( 
            /* [in] */ ULARGE_INTEGER libOffset,
            /* [in] */ ULARGE_INTEGER cb,
            /* [in] */ DWORD dwLockType)
{
	if (!_pStream.p)
		return E_POINTER;

	return _pStream->UnlockRegion(libOffset, cb, dwLockType);
}
        
HRESULT STDMETHODCALLTYPE CStreamLockBytes::Stat( 
            /* [out] */ STATSTG *pstatstg,
            /* [in] */ DWORD grfStatFlag)
{
	if (!_pStream.p)
		return E_POINTER;

	return _pStream->Stat(pstatstg, grfStatFlag);
}
