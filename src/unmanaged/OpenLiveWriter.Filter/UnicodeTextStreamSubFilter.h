// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#pragma once
#include "subfilter.h"

class UnicodeTextStreamSubFilter :
	public SubFilter
{
	BOOL done;
	FULLPROPSPEC propSpec;
	CComPtr<IStream> stream;
public:
	UnicodeTextStreamSubFilter(const FULLPROPSPEC &propSpec, IStream *sourceStream);
	virtual ~UnicodeTextStreamSubFilter(void);

	SCODE GetChunk(
		STAT_CHUNK * pStat
		);
	SCODE GetText(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		);
	SCODE GetValue(
		PROPVARIANT ** ppPropValue
		);
};
