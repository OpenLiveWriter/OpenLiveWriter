// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#include "StdAfx.h"
#include <filterr.h>
#include ".\valuesubfilter.h"

ValueSubFilter::ValueSubFilter(const FULLPROPSPEC &aPropSpec) :
	propSpec(aPropSpec), done(false)
{

}

HRESULT ValueSubFilter::Init(const PROPVARIANT &aValue)
{
	HRESULT hr = PropVariantCopy(&value, &aValue);
	if(FAILED(hr))
		return hr;

	return S_OK;
}

ValueSubFilter::~ValueSubFilter(void)
{
	try
	{
		PropVariantClear(&value);
	}
	catch(...)
	{
	}
}

SCODE ValueSubFilter::GetChunk(
		STAT_CHUNK * pStat
		)
{
	if (done)
		return FILTER_E_END_OF_CHUNKS;
	done = TRUE;

	pStat->attribute = propSpec;
	pStat->idChunk = 0;
	pStat->breakType = CHUNK_NO_BREAK;
	pStat->flags = CHUNK_VALUE;
	pStat->locale = 1033; 
	pStat->idChunkSource = pStat->idChunk;
	pStat->cwcStartSource = 0;
	pStat->cwcLenSource = 0;

	return S_OK;
}

SCODE ValueSubFilter::GetText(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		)
{
	return FILTER_E_NO_TEXT;
}

SCODE ValueSubFilter::GetValue(
		PROPVARIANT ** ppPropValue
		)
{
	*ppPropValue = static_cast<PROPVARIANT*>(CoTaskMemAlloc(sizeof(PROPVARIANT)));
	HRESULT hr = PropVariantCopy(*ppPropValue, &value);
	if(FAILED(hr))
		return hr;
	return S_OK;
}
