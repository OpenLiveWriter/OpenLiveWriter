// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


#pragma once


class XmlUtf16Writer
{
public:
	XmlUtf16Writer()
		: NEWLINE(L"\r\n")
	{
	}

	void OpenFile( const CString& fileName )
	{
		ATLASSERT(m_file.m_h == NULL) ;

		HRESULT hr = m_file.Create( fileName, GENERIC_WRITE, FILE_SHARE_READ, OPEN_ALWAYS ) ;
		if ( FAILED(hr) )
			THROW_HRESULT(hr) ;
	}

	void CloseFile()
	{
		ATLASSERT(m_file.m_h != NULL) ;
		
		// flush the file
		HRESULT hr = m_file.Flush() ;
		if (FAILED(hr))
			THROW_HRESULT(hr) ;
		
		// close the file
		m_file.Close() ;		
	}

	void WriteHeader() 
	{
		// write UTF-16 encoding mark
		BYTE bufferUTF16[] = { 0xFF, 0xFE } ;
		HRESULT hr = m_file.Write( bufferUTF16, 2 ) ;
		if (FAILED(hr))
			THROW_HRESULT(hr) ;

		// write xml document header			
		WriteLine( L"<?xml version=\"1.0\" encoding=\"UTF-16\"?>" ) ;
	}

	void WriteBeginTag( const CStringW& tagName ) 
	{
		WriteBeginTag( tagName, CString() ) ;		
	}

	void WriteBeginTag( const CStringW& tagName, const CStringW& attributes )
	{
		CStringW beginTag ;
		beginTag.AppendFormat( L"<%s", tagName ) ;		
		if ( attributes.GetLength() > 0 )
			beginTag.AppendFormat( L" %s", attributes ) ;		
		beginTag.Append( L">" ) ;
		WriteText( beginTag ) ;
	}

	void WriteEndTag( const CStringW& tagName ) 
	{
		CStringW endTag ;
		endTag.AppendFormat( L"</%s>", tagName ) ;
		WriteText( endTag ) ;
	}	

	void WriteCDATA( const CStringW& strText ) 
	{
		CStringW cdata ; 
		cdata.AppendFormat( L"<![CDATA[%s]]>", strText ) ;
		WriteText( cdata ) ;
	}

	void WriteTag( const CStringW& tagName, const CStringW& contents )
	{
		WriteTag( tagName, CString(), contents ) ;
	}


	void WriteTag( const CStringW& tagName, const CStringW& attributes, const CStringW& contents )
	{
		WriteBeginTag( tagName, attributes ) ;
		WriteCDATA( contents ) ;
		WriteEndTag( tagName ) ;
		WriteNewline() ;	
	}

	void WriteLine( const CStringW& strText ) 
	{
		WriteText( strText ) ;
		WriteNewline() ;		
	}

	void WriteNewline()
	{
		WriteText( NEWLINE ) ;		
	}

	void WriteText( const CStringW& strText )
	{
		ATLASSERT(m_file.m_h != NULL) ;
		
		HRESULT hr = m_file.Write( strText, strText.GetLength() * sizeof(WCHAR) ) ;
		if (FAILED(hr))
			THROW_HRESULT(hr) ;
	}


private:
	CAtlFile m_file ;
	CString NEWLINE ;
};

