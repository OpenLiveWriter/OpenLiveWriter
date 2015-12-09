// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.



#pragma once


enum HtmlSelectionType { HtmlSelectionNone, HtmlSelectionText, HtmlSelectionControl } ;

class HtmlDocumentHelper
{
public:
	static __declspec(noinline) HtmlSelectionType GetSelectionType(CComPtr<IHTMLSelectionObject> pHTMLSelection) 
	{
		if ( pHTMLSelection != NULL )
		{
			CComBSTR type ;
			if ( SUCCEEDED(pHTMLSelection->get_type( &type )))
			{
				CString strType(type) ; 
				strType.MakeLower() ;
				if ( strType == _T("text") )
					return HtmlSelectionText ;
				else if ( strType == _T("control") )
					return HtmlSelectionControl ;
				else if ( strType == _T("none") )
					return HtmlSelectionNone ;
				else
				{
					LOGASSERT(FALSE) ; // unexpected selection type
					return HtmlSelectionNone ;
				}
			}
			else
			{
				return HtmlSelectionNone ;
			}
		}
		else
		{
			return HtmlSelectionNone ;
		}
	}

	static bool HasSelection(CComPtr<IHTMLSelectionObject> pHTMLSelectionObject ) 
	{
		return GetSelectionType(pHTMLSelectionObject) != HtmlSelectionNone ;	
	}


	static __declspec(noinline) CComPtr<IHTMLDocument2> GetHtmlDocument(CComPtr<IWebBrowser2> pWebBrowser) 
	{	
		CComQIPtr<IHTMLDocument2> pHTMLDocument ;
		try
		{
			// check for PDF to avoid bug which happens when referencing IDispatch of a PDF
			CComBSTR location ;
			if ( FAILED(pWebBrowser->get_LocationURL( &location ) ) )
				return pHTMLDocument ;
			CString strLocation(location) ;		
			strLocation.Trim(); strLocation.MakeLower() ;
			if ( strLocation.Right(4) == CString(_T(".pdf") ) ) 
				return pHTMLDocument ;
			
			// get the document and QI for IHTMLDocument2
			CComQIPtr<IHTMLDocument2> pHTMLDocument ;
			CComPtr<IDispatch> pDocument ;
			if ( S_OK == pWebBrowser->get_Document( &pDocument ) )
				pHTMLDocument = pDocument ;

			// return the document
			return pHTMLDocument ;
		}
		catch( ... )
		{
			LOGASSERT(FALSE) ;
			return pHTMLDocument ;
		}	
	}


	static __declspec(noinline) CString GetSelectedHtml(CComPtr<IHTMLSelectionObject> pHTMLSelection) 
	{
		CString strSelectedHtml ;
		CComPtr<IDispatch> pRangeDispatch ;
		if ( SUCCEEDED(pHTMLSelection->createRange( &pRangeDispatch ) ) )
		{
			CComQIPtr<IHTMLTxtRange> pTextRange = pRangeDispatch ;
			if ( pTextRange )
			{
				CComBSTR htmlText ;
				if ( SUCCEEDED(pTextRange->get_htmlText( &htmlText )) )
				{
					strSelectedHtml = CString(htmlText) ;				
				}
				else
				{
					LOGASSERT(FALSE) ;
				}
			}
			else
			{
				LOGASSERT(FALSE) ;
			}
		}
		else
		{
			LOGASSERT(FALSE) ;
		}

		// return the selected HTML (returns empty string if an error occurs getting the selected html)
		return strSelectedHtml ;
	}


	static __declspec(noinline) void GetTitleAndUrl( CComPtr<IWebBrowser2> pWebBrowser, CString& strTitle, CString& strUrl ) 
	{
		CComBSTR title ;	
		if ( SUCCEEDED(pWebBrowser->get_LocationName( &title )) )
			strTitle = title ;

		CComBSTR url ;
		if ( SUCCEEDED(pWebBrowser->get_LocationURL( &url )) )
			strUrl = url ;
	}


};