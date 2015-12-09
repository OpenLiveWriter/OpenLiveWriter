// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#pragma once
#include "subfilter.h"

class ValueSubFilter :
	public SubFilter
{
	BOOL done;
	FULLPROPSPEC propSpec;
	PROPVARIANT value;
public:
	explicit ValueSubFilter(const FULLPROPSPEC &propSpec);
	virtual HRESULT Init(const PROPVARIANT &value);
	virtual ~ValueSubFilter(void);

	SCODE  GetChunk(
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
