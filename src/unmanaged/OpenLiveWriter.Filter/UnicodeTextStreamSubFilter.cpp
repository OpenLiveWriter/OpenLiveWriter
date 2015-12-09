// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#include "StdAfx.h"
#include <filterr.h>
#include ".\unicodetextstreamsubfilter.h"

UnicodeTextStreamSubFilter::UnicodeTextStreamSubFilter(const FULLPROPSPEC &aPropSpec, IStream *sourceStream) :
	propSpec(aPropSpec), stream(sourceStream), done(false)
{
}

UnicodeTextStreamSubFilter::~UnicodeTextStreamSubFilter(void)
{
}

SCODE UnicodeTextStreamSubFilter::GetChunk(
		STAT_CHUNK * pStat
		)
{
	if (done)
		return FILTER_E_END_OF_CHUNKS;
	done = TRUE;

	pStat->attribute = propSpec;
	pStat->idChunk = 0;
	pStat->breakType = CHUNK_NO_BREAK;
	pStat->flags = CHUNK_TEXT;
	pStat->locale = 1033;  
	pStat->idChunkSource = pStat->idChunk;
	pStat->cwcStartSource = 0;
	pStat->cwcLenSource = 0;

	return S_OK;
}

SCODE UnicodeTextStreamSubFilter::GetText(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		)
{
	ULONG countBytes = *pcwcBuffer * 2;
	HRESULT hr = stream->Read(awcBuffer, countBytes, &countBytes);
	switch (hr)
	{
	case S_OK:
		*pcwcBuffer = countBytes / 2;
		return countBytes ? S_OK : FILTER_E_NO_MORE_TEXT;
	case E_PENDING:
		*pcwcBuffer = 0;
		return S_OK;
	case S_FALSE:
		return FILTER_E_NO_MORE_TEXT;
	default:
		return hr;
	}
}

SCODE UnicodeTextStreamSubFilter::GetValue(
		PROPVARIANT ** ppPropValue
		)
{
	return FILTER_E_NO_VALUES;
}
