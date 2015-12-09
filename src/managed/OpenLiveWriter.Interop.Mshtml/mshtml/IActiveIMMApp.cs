// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), ComConversionLoss, Guid("08C0E040-62D1-11D1-9326-0060B067B86E")]
    public interface IActiveIMMApp
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AssociateContext([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint hIME, out uint phPrev);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConfigureIMEA([In] IntPtr hKL, [In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint dwMode, [In] ref __MIDL___MIDL_itf_mshtml_0250_0001 pData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConfigureIMEW([In] IntPtr hKL, [In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint dwMode, [In] ref __MIDL___MIDL_itf_mshtml_0250_0002 pData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateContext(out uint phIMC);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DestroyContext([In] uint hIME);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumRegisterWordA([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPStr)] string szRegister, [In] IntPtr pData, [MarshalAs(UnmanagedType.Interface)] out IEnumRegisterWordA pEnum);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumRegisterWordW([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPWStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPWStr)] string szRegister, [In] IntPtr pData, [MarshalAs(UnmanagedType.Interface)] out IEnumRegisterWordW pEnum);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EscapeA([In] IntPtr hKL, [In] uint hIMC, [In] uint uEscape, [In, Out] IntPtr pData, [ComAliasName("mshtml.LONG_PTR")] out int plResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EscapeW([In] IntPtr hKL, [In] uint hIMC, [In] uint uEscape, [In, Out] IntPtr pData, [ComAliasName("mshtml.LONG_PTR")] out int plResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCandidateListA([In] uint hIMC, [In] uint dwIndex, [In] uint uBufLen, out __MIDL___MIDL_itf_mshtml_0250_0007 pCandList, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCandidateListW([In] uint hIMC, [In] uint dwIndex, [In] uint uBufLen, out __MIDL___MIDL_itf_mshtml_0250_0007 pCandList, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCandidateListCountA([In] uint hIMC, out uint pdwListSize, out uint pdwBufLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCandidateListCountW([In] uint hIMC, out uint pdwListSize, out uint pdwBufLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCandidateWindow([In] uint hIMC, [In] uint dwIndex, out __MIDL___MIDL_itf_mshtml_0250_0005 pCandidate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCompositionFontA([In] uint hIMC, out __MIDL___MIDL_itf_mshtml_0250_0003 plf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCompositionFontW([In] uint hIMC, out __MIDL___MIDL_itf_mshtml_0250_0004 plf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCompositionStringA([In] uint hIMC, [In] uint dwIndex, [In] uint dwBufLen, out int plCopied, [Out] IntPtr pBuf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCompositionStringW([In] uint hIMC, [In] uint dwIndex, [In] uint dwBufLen, out int plCopied, [Out] IntPtr pBuf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCompositionWindow([In] uint hIMC, out __MIDL___MIDL_itf_mshtml_0250_0006 pCompForm);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetContext([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, out uint phIMC);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConversionListA([In] IntPtr hKL, [In] uint hIMC, [In, MarshalAs(UnmanagedType.LPStr)] string pSrc, [In] uint uBufLen, [In] uint uFlag, out __MIDL___MIDL_itf_mshtml_0250_0007 pDst, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConversionListW([In] IntPtr hKL, [In] uint hIMC, [In, MarshalAs(UnmanagedType.LPWStr)] string pSrc, [In] uint uBufLen, [In] uint uFlag, out __MIDL___MIDL_itf_mshtml_0250_0007 pDst, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConversionStatus([In] uint hIMC, out uint pfdwConversion, out uint pfdwSentence);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDefaultIMEWnd([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [Out, ComAliasName("mshtml.wireHWND")] IntPtr phDefWnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDescriptionA([In] IntPtr hKL, [In] uint uBufLen, [Out, MarshalAs(UnmanagedType.LPStr)] string szDescription, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDescriptionW([In] IntPtr hKL, [In] uint uBufLen, [Out, MarshalAs(UnmanagedType.LPWStr)] string szDescription, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetGuideLineA([In] uint hIMC, [In] uint dwIndex, [In] uint dwBufLen, [Out, MarshalAs(UnmanagedType.LPStr)] string pBuf, out uint pdwResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetGuideLineW([In] uint hIMC, [In] uint dwIndex, [In] uint dwBufLen, [Out, MarshalAs(UnmanagedType.LPWStr)] string pBuf, out uint pdwResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIMEFileNameA([In] IntPtr hKL, [In] uint uBufLen, [Out, MarshalAs(UnmanagedType.LPStr)] string szFileName, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIMEFileNameW([In] IntPtr hKL, [In] uint uBufLen, [Out, MarshalAs(UnmanagedType.LPWStr)] string szFileName, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOpenStatus([In] uint hIMC);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetProperty([In] IntPtr hKL, [In] uint fdwIndex, out uint pdwProperty);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRegisterWordStyleA([In] IntPtr hKL, [In] uint nItem, out __MIDL___MIDL_itf_mshtml_0250_0008 pStyleBuf, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRegisterWordStyleW([In] IntPtr hKL, [In] uint nItem, out __MIDL___MIDL_itf_mshtml_0250_0009 pStyleBuf, out uint puCopied);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStatusWindowPos([In] uint hIMC, out tagPOINT pptPos);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetVirtualKey([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, out uint puVirtualKey);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InstallIMEA([In, MarshalAs(UnmanagedType.LPStr)] string szIMEFileName, [In, MarshalAs(UnmanagedType.LPStr)] string szLayoutText, out IntPtr phKL);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InstallIMEW([In, MarshalAs(UnmanagedType.LPWStr)] string szIMEFileName, [In, MarshalAs(UnmanagedType.LPWStr)] string szLayoutText, out IntPtr phKL);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsIME([In] IntPtr hKL);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsUIMessageA([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWndIME, [In] uint msg, [In, ComAliasName("mshtml.UINT_PTR")] uint wParam, [In, ComAliasName("mshtml.LONG_PTR")] int lParam);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsUIMessageW([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWndIME, [In] uint msg, [In, ComAliasName("mshtml.UINT_PTR")] uint wParam, [In, ComAliasName("mshtml.LONG_PTR")] int lParam);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void NotifyIME([In] uint hIMC, [In] uint dwAction, [In] uint dwIndex, [In] uint dwValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterWordA([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPStr)] string szRegister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterWordW([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPWStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPWStr)] string szRegister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReleaseContext([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint hIMC);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCandidateWindow([In] uint hIMC, [In] ref __MIDL___MIDL_itf_mshtml_0250_0005 pCandidate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCompositionFontA([In] uint hIMC, [In] ref __MIDL___MIDL_itf_mshtml_0250_0003 plf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCompositionFontW([In] uint hIMC, [In] ref __MIDL___MIDL_itf_mshtml_0250_0004 plf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCompositionStringA([In] uint hIMC, [In] uint dwIndex, [In] IntPtr pComp, [In] uint dwCompLen, [In] IntPtr pRead, [In] uint dwReadLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCompositionStringW([In] uint hIMC, [In] uint dwIndex, [In] IntPtr pComp, [In] uint dwCompLen, [In] IntPtr pRead, [In] uint dwReadLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCompositionWindow([In] uint hIMC, [In] ref __MIDL___MIDL_itf_mshtml_0250_0006 pCompForm);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetConversionStatus([In] uint hIMC, [In] uint fdwConversion, [In] uint fdwSentence);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOpenStatus([In] uint hIMC, [In] int fOpen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetStatusWindowPos([In] uint hIMC, [In] ref tagPOINT pptPos);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SimulateHotKey([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint dwHotKeyID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnregisterWordA([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPStr)] string szUnregister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnregisterWordW([In] IntPtr hKL, [In, MarshalAs(UnmanagedType.LPWStr)] string szReading, [In] uint dwStyle, [In, MarshalAs(UnmanagedType.LPWStr)] string szUnregister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Activate([In] int fRestoreLayout);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Deactivate();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OnDefWindowProc([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint msg, [In, ComAliasName("mshtml.UINT_PTR")] uint wParam, [In, ComAliasName("mshtml.LONG_PTR")] int lParam, [ComAliasName("mshtml.LONG_PTR")] out int plResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FilterClientWindows([In] ref ushort aaClassList, [In] uint uSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCodePageA([In] IntPtr hKL, out uint uCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLangId([In] IntPtr hKL, out ushort plid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AssociateContextEx([In, ComAliasName("mshtml.wireHWND")] ref _RemotableHandle hWnd, [In] uint hIMC, [In] uint dwFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DisableIME([In] uint idThread);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetImeMenuItemsA([In] uint hIMC, [In] uint dwFlags, [In] uint dwType, [In] ref __MIDL___MIDL_itf_mshtml_0250_0010 pImeParentMenu, out __MIDL___MIDL_itf_mshtml_0250_0010 pImeMenu, [In] uint dwSize, out uint pdwResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetImeMenuItemsW([In] uint hIMC, [In] uint dwFlags, [In] uint dwType, [In] ref __MIDL___MIDL_itf_mshtml_0250_0011 pImeParentMenu, out __MIDL___MIDL_itf_mshtml_0250_0011 pImeMenu, [In] uint dwSize, out uint pdwResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumInputContext([In] uint idThread, [MarshalAs(UnmanagedType.Interface)] out IEnumInputContext ppEnum);
    }
}

