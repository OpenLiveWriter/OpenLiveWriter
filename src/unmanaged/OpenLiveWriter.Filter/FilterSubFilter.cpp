// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#include "StdAfx.h"
#include <strsafe.h>
#include <filterr.h>
#include ".\filtersubfilter.h"

FilterSubFilter::FilterSubFilter(const FULLPROPSPEC &aPropSpec, IFilter *ifilter, IUnknown *releaseOnDestruction, const CString& fileToDeleteOnDestruction) :
	propSpec(aPropSpec), filter(ifilter), releaseOnDestruction(releaseOnDestruction)
{
	HRESULT hr = S_OK;

	_fileToDeleteOnDestruction = fileToDeleteOnDestruction ;
	
	FULLPROPSPEC *dummy(NULL);
	ULONG flags;
	hr = ifilter->Init(IFILTER_INIT_INDEXING_ONLY, 0, NULL, &flags);
	if (FAILED(hr))
	{
		filter.Release();
	}
}

FilterSubFilter::~FilterSubFilter(void)
{
	try
	{
		if ( _fileToDeleteOnDestruction.GetLength() > 0 ) 
			::DeleteFile( _fileToDeleteOnDestruction ) ;	
	}
	catch(...)
	{
	}
}

SCODE FilterSubFilter::GetChunk(
		STAT_CHUNK * pStat
		)
{
	if (!filter)
		return FILTER_E_END_OF_CHUNKS;

	while (true)
	{
		SCODE hr = filter->GetChunk(pStat);
		if (hr != S_OK)
		{
			return hr;
		}

		// Only continue if CHUNK_TEXT.  Otherwise move on to the next chunk.
		if (pStat->flags & CHUNK_TEXT)
			break;
	}

	pStat->attribute = propSpec;

	return S_OK;
}

SCODE FilterSubFilter::GetText(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		)
{
	return filter->GetText(pcwcBuffer, awcBuffer);
}

SCODE FilterSubFilter::GetValue(
		PROPVARIANT ** ppPropValue
		)
{
	return FILTER_E_NO_VALUES;
}
