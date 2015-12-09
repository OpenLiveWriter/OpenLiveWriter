// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

#include "HResultException.h"

class LogFile
{
public:
	LogFile(LPCTSTR applicationName, LPCSTR facility) 
		: m_facility(facility)
	{
		try
		{
			// determine the directory where we should write the log file
			TCHAR lpszAppDataPath[MAX_PATH*3] ;
	#ifdef DEBUG
			HRESULT hr = ::SHGetFolderPath(
				NULL,			// no owner window
				CSIDL_PERSONAL,
				NULL,
				SHGFP_TYPE_CURRENT,
				lpszAppDataPath ) ;
	#else
			HRESULT hr = ::SHGetFolderPathAndSubDir(
				NULL,			// no owner window
				CSIDL_LOCAL_APPDATA | CSIDL_FLAG_CREATE,
				NULL,
				SHGFP_TYPE_CURRENT,
				applicationName,
				lpszAppDataPath ) ;
	#endif
			
			if ( SUCCEEDED(hr) )
			{
				// form the path to the log file
				m_logFilePath.AppendFormat( _T("%s\\%s.log"), lpszAppDataPath, applicationName) ;
			}
			else
			{
				ATLASSERT(FALSE) ;
			}
		}
		catch(...)
		{
			// log file initialization is a nice to have, don't ever let
			// it halt or cause errors in the startup of IEXPLORE.EXE!
			ATLASSERT(FALSE) ;
		}
	}
	
	__declspec(noinline) void LogAssertFailed( int sourceLine, LPCSTR lpszSourceFile, LPCSTR lpszTimestamp ) 
	{
		try
		{
			CStringA strError ;
			strError.AppendFormat( "ASSERT Failed: Line %ld, %s (%s)", sourceLine, lpszSourceFile, lpszTimestamp ) ;
			AppendEntry( "Error", strError ) ;
		}
		catch(...)
		{
			ATLASSERT(FALSE) ;
		}
	}

	void LogError( const HResultException& e )
	{
		LogError( e.GetErrorCode(), e.GetLine(), e.GetFile(), e.GetTimestamp() ) ;
	}

	__declspec(noinline) void LogError( HRESULT hr, int sourceLine, LPCSTR lpszSourceFile, LPCSTR lpszTimestamp ) 
	{
		try
		{
			CStringA strError ;
			strError.AppendFormat( "HRESULT %#08x: Line %ld, %s (%s)", hr, sourceLine, lpszSourceFile, lpszTimestamp ) ;
			AppendEntry( "Error", strError ) ;
		}
		catch(...)
		{
			ATLASSERT(FALSE) ;
		}
	}
	
	

private:
	__declspec(noinline) void AppendEntry( LPCSTR lpszCategory, LPCSTR lpszMessage ) 
	{
		// must have initialized to write an entry
		if ( m_logFilePath.GetLength() == 0 )
		{
			ATLASSERT(FALSE) ;
			return ;
		}

		try
		{
			// get the current time
			SYSTEMTIME currentTime ;
			::GetSystemTime( &currentTime ) ;
			currentTime.wDay ;

			// calculate the log entry
			CStringA strLogEntry ;
			strLogEntry.AppendFormat( "%s,%lu,%s,%05ld,%02hu-%02hu-%4hu %02hu:%02hu:%02hu,\"%s\",\"\"\r\n",
				m_facility,
				::GetCurrentProcessId(),
				lpszCategory,
				::InterlockedIncrement( &s_dwSequenceNumber ),
				currentTime.wMonth,
				currentTime.wDay,
				currentTime.wYear,
				currentTime.wHour,
				currentTime.wMinute,
				currentTime.wSecond,
				lpszMessage ) ;


			// Try to write the message.  Incrementally back off, waiting for the file to 
			// become available.  (The first backoff is 0ms intentionally -- to give up our 
			// scheduling quantum -- allowing another thread to run. Subsequent backoffs 
			// increase linearly at 10ms intervals with up to 10 retries)
			for (int i = 0; i<10; i++)
			{
				if ( DoAppendEntry( strLogEntry ) )			
					break;
				
				//	Sleep. Back off linearly, but not more than 2s.
				::Sleep( min( i * 10, 2000 ) );
			}					
		}
		catch(...)
		{
			// writing an entry should always be safe!
			ATLASSERT(FALSE) ;
		}
	}

	__declspec(noinline) BOOL DoAppendEntry( const CStringA& strLogEntry ) 
	{
		// try to open the file		
		CAtlFile logFile ;
		HRESULT hr = logFile.Create( m_logFilePath, GENERIC_WRITE, FILE_SHARE_READ, OPEN_ALWAYS ) ;
		if ( SUCCEEDED(hr) )
		{
			DWORD dwBufferSize = strLogEntry.GetLength();
			DWORD dwBytesWritten = 0 ;
			if ( SUCCEEDED(logFile.Seek( 0, FILE_END )) )
				logFile.Write( (const void*)strLogEntry, dwBufferSize, &dwBytesWritten ) ;
			ATLASSERT( dwBufferSize == dwBytesWritten ) ;
			
			logFile.Flush() ;
			logFile.Close() ;

			return TRUE ;
		}	
		else
		{
			return FALSE ;
		}
	}

private:
	CString m_logFilePath ;
	CStringA m_facility ;
	volatile static LONG s_dwSequenceNumber ;
} ;

extern LogFile *_LogFile ;

#define LOGASSERT(condition) { ATLASSERT(condition); if (_LogFile) { if (!(condition)) _LogFile->LogAssertFailed(__LINE__, __FILE__, __TIMESTAMP__ ); } }
#define LOGERROR(e) { ATLASSERT(FALSE); if (_LogFile) { _LogFile->LogError( e ); } }
#define LOGERROR_HR(hr) { ATLASSERT(FALSE); if (_LogFile) { _LogFile->LogError( hr, __LINE__, __FILE__, __TIMESTAMP__ ); } }

#define DECLARE_LOGFILE(applicationName, facility) LogFile *_LogFile = new LogFile(applicationName, facility); volatile LONG LogFile::s_dwSequenceNumber = 10000 ;
#define DECLARE_NULL_LOGFILE LogFile *_LogFile = 0; volatile LONG LogFile::s_dwSequenceNumber = 10000 ;

