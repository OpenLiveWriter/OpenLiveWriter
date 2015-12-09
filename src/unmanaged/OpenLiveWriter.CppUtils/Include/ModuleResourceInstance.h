// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once

// Helper class for getting the current ModuleResourceInstance and making
// it available as an HINSTANCE and a ULONG (the format required by calls
// to ITBServices)

class ModuleResourceInstance
{
public:
	ModuleResourceInstance()
	{	
		// get the resource instance
		CComModule* pCurrentModule = static_cast<CComModule*>(_pAtlModule) ;
		m_hResInst = pCurrentModule->GetResourceInstance() ;
		
		// ugly cast to ULONG so it is the format expected by ITBServices::addButton    
		#pragma warning ( disable : 4311 )	
		m_lResInst = reinterpret_cast<ULONG>(m_hResInst) ;
		#pragma warning ( default : 4311 )
	}

	operator HINSTANCE() { return m_hResInst; }
	operator ULONG() { return m_lResInst ; }
	

private:
	HINSTANCE m_hResInst ;
	ULONG m_lResInst ;
};