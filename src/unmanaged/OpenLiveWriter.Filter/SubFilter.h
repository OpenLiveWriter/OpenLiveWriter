// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#pragma once

#include "stdafx.h"
#include <filter.h>

/*
Represents inner components that IFilters can use to delegate.
*/
class SubFilter
{
public:
	SubFilter(void) {}
	virtual ~SubFilter(void) {}

	virtual SCODE GetChunk(
		STAT_CHUNK * pStat
		) = 0;
	virtual SCODE GetText(
		ULONG * pcwcBuffer,
		WCHAR * awcBuffer
		) = 0;
	virtual SCODE GetValue(
		PROPVARIANT ** ppPropValue
		) = 0;

};
