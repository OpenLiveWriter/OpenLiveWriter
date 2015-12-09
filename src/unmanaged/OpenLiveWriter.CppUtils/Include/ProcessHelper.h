// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

class ProcessHelper
{
public:
		
	// launch the specified process and wait for input idle
	static _declspec(noinline) void LaunchProcessWithFeedback( LPCTSTR processName, LPCTSTR commandLine ) 
	{		
		// initialize startup info (provide startup feedback)
		STARTUPINFO si;
		ZeroMemory(&si,sizeof(STARTUPINFO));
		si.cb = sizeof(STARTUPINFO);
		si.dwFlags = STARTF_FORCEONFEEDBACK ;

		// initialize process info
		PROCESS_INFORMATION pi;
		ZeroMemory(&pi,sizeof(PROCESS_INFORMATION));

		// compute process name and command line
		LPCTSTR lpszProcess = NULL ;
		LPTSTR lpszCommandLine = NULL ;
		CString strCommandLine ;
		if ( commandLine == NULL )
		{
			lpszProcess = processName ;			
		}
		else
		{
			// build the command line (must include the process name and the actual command line)			
			strCommandLine.AppendFormat( _T("\"%s\" %s"), processName, commandLine ) ;		
			lpszCommandLine = strCommandLine.GetBuffer() ;
		}

		// create the process
		BOOL result = ::CreateProcess(
			lpszProcess,	// executable file
			lpszCommandLine,// command line
			NULL,			// no process security attributes
			NULL,			// no thread security 
			FALSE,			// don't inherit handles
			0,				// no special creation flags
			NULL,			// use environment of calling process
			NULL,			// use current directory of calling process
			&si,			// startup info
			&pi ) ;			// process info (handles returned in this structure)


		// check for error (shouldn't ever happen!)
		if ( !result )
			THROW_WIN32(::GetLastError()) ;
		
		
		// close handles
		CloseHandle( pi.hThread ) ;
		CloseHandle( pi.hProcess ) ;	
	}


};