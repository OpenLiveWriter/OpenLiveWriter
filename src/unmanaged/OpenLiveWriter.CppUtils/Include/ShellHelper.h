// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

class ShellHelper
{
public:
	/// <summary>
	/// For a file extension (with leading period) and a verb (or NULL for default
	/// verb), returns the full path to the executable file that is assigned to
	/// that extension/verb. Returns null if an error occurs.
	/// </summary>
	__declspec(noinline) static CString GetExecutablePath(LPCTSTR extension, LPCTSTR verb)
	{
		const DWORD BUFFER_SIZE = 1024; // This should be big enough for anything
		DWORD capacity = BUFFER_SIZE;
		TCHAR path[BUFFER_SIZE];
		path[0] = '\0';

		HRESULT hr = ::AssocQueryString(ASSOCF_NOTRUNCATE, ASSOCSTR_EXECUTABLE, extension, verb, path, &capacity);

		switch (hr)
		{
			case S_OK:
				return CString(path);  // success; return the path

			// failure; buffer was too small
			case E_POINTER:
			case S_FALSE:
			case E_INVALIDARG:
			case E_FAIL:
			default:
				THROW_HRESULT(hr) ;								
		}
	}


};