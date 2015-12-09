// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// WebPostFilter.h : Declaration of the CWebPostFilter

#pragma once
#include "resource.h"       // main symbols
#include <filter.h>

#include "OpenLiveWriter.Filter.h"
#include "SubFilter.h"


// CWebPostFilter

class ATL_NO_VTABLE CWebPostFilter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CWebPostFilter, &CLSID_WebPostFilter>,
	public IDispatchImpl<IWebPostFilter, &IID_IWebPostFilter, &LIBID_OpenLiveWriterFilterLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IFilter,
	//public IPersist,
	public IPersistStream,
	public IPersistFile
{
	CComPtr<IStorage> stg;
	CComPtr<ILockBytes> plkbyt;
	FILETIME lastModified;
	int pos;
	int idChunkOffset;
	int idChunkLastValue;
	SubFilter *subFilter;

	HRESULT OpenTextStream(LPCOLESTR streamName, IStream **stream);
	HRESULT NextSubFilter(void);
	void CleanupSubFilter(void);

public:
	CWebPostFilter() :
	  stg(NULL), subFilter(NULL), m_pUnkMarshaler(NULL), pos(0), idChunkOffset(0), idChunkLastValue(-1)
	{
		ZeroMemory(&lastModified, sizeof(FILETIME));
	}

#pragma warning ( disable : 4995 )
DECLARE_REGISTRY_RESOURCEID(IDR_WEBPOSTFILTER)
#pragma warning ( default : 4995 )


DECLARE_NOT_AGGREGATABLE(CWebPostFilter)

BEGIN_COM_MAP(CWebPostFilter)
	COM_INTERFACE_ENTRY(IWebPostFilter)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY_AGGREGATE(IID_IMarshal, m_pUnkMarshaler.p)
	COM_INTERFACE_ENTRY(IFilter)
	COM_INTERFACE_ENTRY2(IPersist, IPersistStream)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IPersistFile)
END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()
	DECLARE_GET_CONTROLLING_UNKNOWN()

	HRESULT FinalConstruct()
	{
		return CoCreateFreeThreadedMarshaler(
			GetControllingUnknown(), &m_pUnkMarshaler.p);
	}

	void FinalRelease()
	{
		CleanupSubFilter();
		m_pUnkMarshaler.Release();
	}

	CComPtr<IUnknown> m_pUnkMarshaler;

public:
	// IFilter
	STDMETHOD(Init)(
		ULONG grfFlags,
		ULONG cAttributes,
		FULLPROPSPEC const * aAttributes,
		ULONG * pdwFlags
		);
	STDMETHOD(GetChunk)(
		STAT_CHUNK * pStat
		);
	STDMETHOD(GetText)(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		);
	STDMETHOD(GetValue)(
		PROPVARIANT ** ppPropValue
		);
	STDMETHOD(BindRegion)(
		FILTERREGION origPos,
		REFIID riid,
		void ** ppunk
		);

	// IPersist
	STDMETHOD(GetClassID)(CLSID * pClassID);

	// IPersistFile
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(
		LPCOLESTR pszFileName, 
		DWORD dwMode
		);
	STDMETHOD(Save)(
		LPCOLESTR pszFileName, 
		BOOL fRemember
		);
	STDMETHOD(SaveCompleted)(
		LPCOLESTR pszFileName
		);
	STDMETHOD(GetCurFile)(
		LPOLESTR * ppszFileName
		);

	// IPersistStream
	STDMETHOD(Load)(
		IStream * pStm
		);
	STDMETHOD(Save)(
		IStream * pStm, 
		BOOL fClearDirty
		);
	STDMETHOD(GetSizeMax)(
		ULARGE_INTEGER * pcbSize
		);
};

OBJECT_ENTRY_AUTO(__uuidof(WebPostFilter), CWebPostFilter)
