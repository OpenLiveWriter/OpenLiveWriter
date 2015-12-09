// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

class TempFileHelper
{
public:
	__declspec(noinline) static CString GetUniqueTempFileName(LPCTSTR lpszExtension) 
	{
		// unique name to return
		CString strUniqueFileName ;

		// generate GUID and get a string version of it 
		GUID fileGuid ;
		::CoCreateGuid( &fileGuid ) ;
		const int buffGuidSize = 100 ;
		OLECHAR buffGuid[buffGuidSize] ;
		::StringFromGUID2( fileGuid, buffGuid, buffGuidSize ) ;

		// get the system temporary file path
		const int buffTempPathSize = MAX_PATH * 10 ;
		CString strTempPath ;		
		if ( ::GetTempPath( buffTempPathSize, strTempPath.GetBuffer(buffTempPathSize) ) == 0 )
		{
			THROW_WIN32(::GetLastError()) ;
		}
		else
		{
			strTempPath.ReleaseBuffer() ;

			// format the name
			if ( strTempPath.Right(1) != "\\" )
				strTempPath += "\\" ;
			strUniqueFileName.AppendFormat( _T("%s%s%s"), strTempPath, buffGuid, lpszExtension ) ;
		}

		// return the name
		return strUniqueFileName ;
	}

};