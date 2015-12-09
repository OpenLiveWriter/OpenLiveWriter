// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// WebPostFilter.cpp : Implementation of CWebPostFilter


#include "stdafx.h"
#include <ntquery.h>
#include <strsafe.h>
#include <filterr.h>
#include <atlfile.h>
#include "WebPostFilter.h"
#include "PostEditorFileConstants.h"
#include "ValueSubFilter.h"
#include "UnicodeTextStreamSubFilter.h"
#include "SafeBuffer.h"
#include "FilterSubFilter.h"


const int POS_PERCEIVEDTYPE = 0;
const int POS_TITLE = 1;
const int POS_PRIMARYDATE = 2;
const int POS_KEYWORDS = 3;
const int POS_BODY = 4;

inline const FULLPROPSPEC PropSpec(const GUID &guidPropSet, const LPWSTR &lpwstr);
inline const FULLPROPSPEC PropSpec(const GUID &guidPropSet, ULONG propid);
HRESULT GetLastModified(LPCTSTR filename, FILETIME *filetime);
HRESULT CopyStreamToTempFile(IStream *stream, LPCWSTR extension, LPWSTR fileName, int cbFileName);

// CWebPostFilter

STDMETHODIMP CWebPostFilter::Init(
									 ULONG grfFlags,
									 ULONG cAttributes,
									 FULLPROPSPEC const * aAttributes,
									 ULONG *	pdwFlags
									 )
{
	try
	{
		pos = 0;
		idChunkLastValue = -1;
		idChunkOffset = 0;
		CleanupSubFilter();
		return S_OK;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}	
}

void CWebPostFilter::CleanupSubFilter(void)
{
	SubFilter *localSubFilter = subFilter;
	if (localSubFilter)
	{
		subFilter = NULL;
		delete localSubFilter;
	}
}

STDMETHODIMP CWebPostFilter::GetChunk(
	STAT_CHUNK * pStat
	)
{
	try
	{
		HRESULT hr = S_OK;

		while (true)
		{
			while (!subFilter)
			{
				idChunkOffset = idChunkLastValue + 1;
				hr = NextSubFilter();
				// no more subfilters to load
				if (FILTER_E_END_OF_CHUNKS == hr)
				{
					return hr;
				}
				if (FAILED(hr))
					return hr;
			}

			// there is a subfilter--let's try it
			hr = subFilter->GetChunk(pStat);
			if (hr == FILTER_E_END_OF_CHUNKS)
			{
				CleanupSubFilter();
				continue;
			}
			if (SUCCEEDED(hr))
			{
				pStat->idChunk += idChunkOffset;
				pStat->idChunkSource += idChunkOffset;
				idChunkLastValue = pStat->idChunk;
			}
			return hr;
		}

		return hr;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}	
}

HRESULT CWebPostFilter::NextSubFilter(void)
{
	static GUID SYSTEM_PROPSET = { 0xB725F130, 0x47EF, 0x101A, { 0xA5, 0xF1, 0x02, 0x60, 0x8C, 0x9E, 0xEB, 0xAC } };
	static GUID SHAREPOINT_PROPSET = { 0xF29F85E0, 0x4FF9, 0x1068, { 0xAB, 0x91, 0x08, 0x00, 0x2B, 0x27, 0xB3, 0xD9 } };
	static GUID WDS_PROPSET = { 0xD5CDD505, 0x2E9C, 0x101B, { 0x93, 0x97, 0x08, 0x00, 0x2B, 0x2C, 0xF9, 0xAE } };

	// FULLPROPSPEC AUTHOR_PROPSPEC = PropSpec(SHAREPOINT_PROPSET, 4);
	// FULLPROPSPEC PERCEIVEDTYPE_PROPSPEC = PropSpec(WDS_PROPSET, L"PerceivedType");

	HRESULT hr = S_OK;

	while (true)
	{
		CleanupSubFilter();

		switch (pos++)
		{
		case POS_PERCEIVEDTYPE:
			{
				PROPVARIANT var;
				PropVariantInit(&var);
				var.vt = VT_LPWSTR;
				var.pwszVal = L"document";
				ValueSubFilter* pValueSubFilter = new ValueSubFilter(PropSpec(WDS_PROPSET, L"PerceivedType"));
				if (!pValueSubFilter)
					return E_OUTOFMEMORY;
				hr = pValueSubFilter->Init(var);				
				if(FAILED(hr))
					return hr;
				subFilter = pValueSubFilter;
				break;
			}
		case POS_TITLE:
			{
				CComPtr<IStream> stream;
				hr = OpenTextStream(POST_TITLE, &stream);
				if (STG_E_FILENOTFOUND == hr)
				{
					continue;
				}
				if (FAILED(hr))
				{
					return hr;
				}

				subFilter = new UnicodeTextStreamSubFilter(PropSpec(SHAREPOINT_PROPSET, 2), stream.p);
				break;
			}
		case POS_PRIMARYDATE:
			{
				PROPVARIANT *var = static_cast<PROPVARIANT*>(CoTaskMemAlloc(sizeof(PROPVARIANT)));
				if (!var)
					return E_OUTOFMEMORY;
				PropVariantInit(var);

				var->vt = VT_FILETIME;
				var->filetime = lastModified;
				ValueSubFilter* pValueSubFilter = new ValueSubFilter(PropSpec(WDS_PROPSET, L"PrimaryDate"));
				hr = pValueSubFilter->Init(*var);
				PropVariantClear(var);
				if(FAILED(hr))
					return hr;
				subFilter = pValueSubFilter;
				break;
			}
		case POS_KEYWORDS:
			{
				CComPtr<IStream> stream;
				hr = OpenTextStream(POST_KEYWORDS, &stream);
				if (STG_E_FILENOTFOUND == hr)
					continue;
				if (FAILED(hr))
					return hr;

				subFilter = new UnicodeTextStreamSubFilter(PropSpec(SHAREPOINT_PROPSET, 5), stream.p);
				break;
			}
		case POS_BODY:
			{
				CComPtr<IStream> sourceStream;
				if (FAILED(hr = OpenTextStream(POST_CONTENTS, &sourceStream)))
				{
					if (hr == STG_E_FILENOTFOUND)
						continue;
					return hr;
				}

				// 
				CLSID htmlFilterClsid;
				if (FAILED(hr = CLSIDFromString(L"{E0CA5340-4534-11CF-B952-00AA0051FE20}", &htmlFilterClsid)))
					return hr;
				CComQIPtr<IFilter> htmlFilter;

				if (FAILED(hr = htmlFilter.CoCreateInstance(htmlFilterClsid)))
					return hr;

				// The HTML ifilter should implement IPersistStream,
				// but I'm not seeing it so far.

				CComQIPtr<IPersistStream> persistStream = htmlFilter;
				if (persistStream)
				{
					if (FAILED(hr = persistStream->Load(sourceStream.p)))
						return hr;
					subFilter = new FilterSubFilter(PropSpec(SYSTEM_PROPSET, 19), htmlFilter.p, sourceStream.p);
				}
				else
				{
					wchar_t fileName[MAX_PATH + 1];
					if (FAILED(hr = CopyStreamToTempFile(sourceStream.p, L".htm", fileName, sizeof(fileName))))
						return hr;
					CComQIPtr<IPersistFile> persistFile(htmlFilter);
					if (!persistFile)
						continue;
					if (FAILED(hr = persistFile->Load(fileName, 0)))
						return hr;

					subFilter = new FilterSubFilter(PropSpec(SYSTEM_PROPSET, 19), htmlFilter.p, NULL, fileName);
				}
				break;

			}
		default:
			{
				--pos;
				return FILTER_E_END_OF_CHUNKS;
			}
		}
		return hr;
	}
}

HRESULT CWebPostFilter::OpenTextStream(LPCOLESTR streamName, IStream **stream)
{
	return stg->OpenStream(streamName, NULL, STGM_READ | STGM_SHARE_EXCLUSIVE, 0, stream);
}

STDMETHODIMP CWebPostFilter::GetText(
										ULONG *	pcwcBuffer,
										WCHAR *	awcBuffer
										)
{
	try
	{
		if (subFilter != NULL)
			return subFilter->GetText(pcwcBuffer, awcBuffer);
		else
			return E_FAIL;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}
}

STDMETHODIMP CWebPostFilter::GetValue(
	PROPVARIANT	** ppPropValue
	)
{
	try
	{
		if (subFilter != NULL)
			return subFilter->GetValue(ppPropValue);
		else
			return E_FAIL;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}	
}

STDMETHODIMP CWebPostFilter::BindRegion(
	FILTERREGION origPos,
	REFIID riid,
	void **	ppunk
	)
{
	return E_NOTIMPL;
}

// IPersist
STDMETHODIMP CWebPostFilter::GetClassID(CLSID * pClassID)
{
	*pClassID = CLSID_WebPostFilter;
	return S_OK;
}

// IPersistFile
STDMETHODIMP CWebPostFilter::IsDirty(void)
{
	return S_FALSE;
}

STDMETHODIMP CWebPostFilter::Load(LPCOLESTR pszFileName, DWORD dwMode)
{
	try
	{
		ATLASSERT(!stg);
		
		if (stg)
			stg.Release();

		pos = 0;

		HRESULT hr = GetLastModified(pszFileName, &lastModified);
		if (FAILED(hr))
			return hr;

		hr = StgOpenStorage(
			pszFileName, 
			NULL, 
			STGM_DIRECT | STGM_READ | STGM_SHARE_DENY_WRITE, 
			NULL, 
			0, 
			&stg);

		return hr;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}	
}

STDMETHODIMP CWebPostFilter::Save(LPCOLESTR pszFileName, BOOL fRemember)
{
	return E_NOTIMPL;
}

STDMETHODIMP CWebPostFilter::SaveCompleted(LPCOLESTR pszFileName)
{
	return E_NOTIMPL;
}

STDMETHODIMP CWebPostFilter::GetCurFile(LPOLESTR * ppszFileName)
{
	return E_NOTIMPL;
}


// IPersistStream
STDMETHODIMP CWebPostFilter::Load(IStream * pStm)
{
	try
	{
		CComQIPtr<IStreamLockBytes> pSlb;
		CHECK_HRESULT(pSlb.CoCreateInstance(CLSID_StreamLockBytes));
		CHECK_HRESULT(pSlb->Init(pStm));
		
		plkbyt = pSlb;
		STATSTG statstg;
		CHECK_HRESULT(plkbyt->Stat(&statstg, STATFLAG_NONAME));
		lastModified = statstg.mtime;

		CHECK_HRESULT(StgOpenStorageOnILockBytes(
			plkbyt,
			NULL,
			STGM_DIRECT | STGM_READ | STGM_SHARE_DENY_WRITE,
			NULL,
			0,
			&stg));

		return S_OK;
	}
	catch(HResultException e)
	{
		LOGERROR(e) ;		
		return e.GetErrorCode() ;
	}
	catch(...)
	{
		LOGASSERT(FALSE) ;
		return E_UNEXPECTED ;
	}	
}

STDMETHODIMP CWebPostFilter::Save(IStream * pStm, BOOL fClearDirty)
{
	return E_NOTIMPL;
}

STDMETHODIMP CWebPostFilter::GetSizeMax(ULARGE_INTEGER * pcbSize)
{
	return E_NOTIMPL;
}




// Constructor for string-based FULLPROPSPECs
inline const FULLPROPSPEC PropSpec(const GUID &guidPropSet, const LPWSTR &lpwstr)
{
	FULLPROPSPEC propSpec;
	propSpec.guidPropSet = guidPropSet;
	propSpec.psProperty.ulKind = PRSPEC_LPWSTR;
	propSpec.psProperty.lpwstr = lpwstr;
	return propSpec;
}

// Constructor for id-based FULLPROPSPECs
inline const FULLPROPSPEC PropSpec(const GUID &guidPropSet, ULONG propid)
{
	FULLPROPSPEC propSpec;
	propSpec.guidPropSet = guidPropSet;
	propSpec.psProperty.ulKind = PRSPEC_PROPID;
	propSpec.psProperty.propid = propid;
	return propSpec;
}

HRESULT GetLastModified(LPCTSTR filename, FILETIME *filetime)
{
	HRESULT hr = S_OK;

	HANDLE hFile = CreateFile(filename, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
	if (INVALID_HANDLE_VALUE == hFile)
		hr = HRESULT_FROM_WIN32(GetLastError());
	else
	{
		FILETIME creationTime, lastAccessTime;
		if (!GetFileTime(hFile, &creationTime, &lastAccessTime, filetime))
			hr = HRESULT_FROM_WIN32(GetLastError());
		CloseHandle(hFile);
	}
	return hr;
}

HRESULT CopyStreamToTempFile(IStream *stream, LPCWSTR extension, LPWSTR fileName, int cbFileName)
{
	HRESULT hr;

	CAtlTemporaryFile tempFile;
	if (FAILED(hr = tempFile.Create()))
		return hr;
#define BUF_SIZE 0x2000
	{
		SafeBuffer buf(BUF_SIZE);
		if (buf.p == 0)
			return E_OUTOFMEMORY;
		while (hr == S_OK)
		{
			ULONG bytesRead;
			// will return S_FALSE if end of stream?
			hr = stream->Read(buf.p, BUF_SIZE, &bytesRead);
			if (hr == S_OK)
			{
				if (bytesRead == 0)
					break;
				hr = tempFile.Write(buf.p, bytesRead);
			}
		}
	}
	if (FAILED(hr))
		return hr;
	hr = S_OK;  // in case previously set to S_FALSE

	LPCTSTR tempFileName = tempFile.TempFileName();

	if (FAILED(hr = StringCbCopy(fileName, cbFileName, tempFileName)))
		return hr;
	if (FAILED(hr = StringCbCat(fileName, cbFileName, L".htm")))
		return hr;

	hr = tempFile.Close(fileName);
	if (FAILED(hr))
		return hr;

	return hr;
}