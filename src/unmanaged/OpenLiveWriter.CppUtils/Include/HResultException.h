// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

class HResultException
{
public:
	HResultException(HRESULT hr, const CStringA& file, int line, const CStringA& timestamp)
		: m_hr(hr), m_file(file), m_line(line), m_timestamp(timestamp)
	{
	}

	HRESULT GetErrorCode() const { return m_hr; } 
	CStringA GetFile() const { return m_file; } 
	int GetLine() const { return m_line; } 
	CStringA GetTimestamp() const { return m_timestamp; } 

private:
	HRESULT m_hr ;
	CStringA m_file ;
	int m_line ;
	CStringA m_timestamp ;
};


#define THROW_HRESULT(hr) throw HResultException(hr, __FILE__, __LINE__, __TIMESTAMP__)
#define THROW_WIN32(dwError) THROW_HRESULT(HRESULT_FROM_WIN32(dwError))
#define CHECK_HRESULT(hr) _INLINE_CHECK_HRESULT(hr, __FILE__, __LINE__, __TIMESTAMP__)

inline void _INLINE_CHECK_HRESULT(DWORD hr, const CStringA& file, int line, const CStringA& timestamp)
{
	if (FAILED(hr))
		throw HResultException(hr, file, line, timestamp);
}