// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#pragma once
#include "subfilter.h"

class FilterSubFilter :
	public SubFilter
{
	FULLPROPSPEC propSpec;
	CComPtr<IFilter> filter;
	CComPtr<IUnknown> releaseOnDestruction;
	CString _fileToDeleteOnDestruction ;
public:
	FilterSubFilter(const FULLPROPSPEC &propSpec, IFilter *ifilter, IUnknown *releaseOnDestruction = NULL, const CString& fileToDeleteOnDestruction = CString());
	~FilterSubFilter(void);

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
