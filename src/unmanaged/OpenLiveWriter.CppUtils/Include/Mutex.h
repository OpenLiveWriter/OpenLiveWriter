// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.



#pragma once

// utility class for creating, accessing, freeing, and destroying mutexes

class Mutex
{
public:
	Mutex() 
	{
		// Create the mutex -- if this call fails then all of the
		// rest of the operations (Wait, Release, and the destructor)
		// will safely no-op by checking m_hMutex != NULL
		m_hMutex = ::CreateMutex( NULL, FALSE, NULL ) ;
		ATLASSERT( m_hMutex != NULL ) ;
	}
	
	virtual ~Mutex()
	{
		try
		{
			if ( m_hMutex )
			{
				BOOL closed = ::CloseHandle( m_hMutex ) ;
				ATLASSERT(closed) ;
			}
		}
		catch(...)
		{
			ATLASSERT(FALSE) ;
		}
	}	

private:
	friend class MutexLock ;
	void Wait()
	{
		if ( m_hMutex )
			::WaitForSingleObject( m_hMutex, INFINITE ) ;
	}
	void Release()
	{
		if ( m_hMutex )
		{
			BOOL released = ::ReleaseMutex( m_hMutex ) ;
			ATLASSERT(released) ;
		}
	}

private:
	HANDLE m_hMutex ;	
} ;

class MutexLock
{
public:
	MutexLock( Mutex& mutex )
		: m_mutex(mutex)
	{
		m_mutex.Wait() ;
	}
	virtual ~MutexLock()
	{
		try
		{
			m_mutex.Release() ;
		}
		catch(...)
		{
			ATLASSERT(FALSE) ;
		}
	}

private:
	Mutex& m_mutex ;
} ;