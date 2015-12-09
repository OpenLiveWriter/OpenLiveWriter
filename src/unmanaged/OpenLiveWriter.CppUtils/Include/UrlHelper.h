// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.



#pragma once

class UrlHelper
{
public:
	
	__declspec(noinline) static CString CreateUrlFromPath( const CString& path )
	{
		// allocate string to hold url
		CString url ;
		DWORD dwBufferSize = 4096 ;
		
		// normalize the url
		HRESULT hr = ::UrlCreateFromPath( path, url.GetBufferSetLength(dwBufferSize), &dwBufferSize, 0 ) ;
		url.ReleaseBuffer() ;

		// returns S_OK for successful conversion and S_FALSE if it didn't need conversion
		if ( hr == S_OK || hr == S_FALSE )
		{
			return url ;
		}
		else
		{
			// couldn't convert!
			LOGASSERT(FALSE) ;
			return path ;
		}		
	}
};