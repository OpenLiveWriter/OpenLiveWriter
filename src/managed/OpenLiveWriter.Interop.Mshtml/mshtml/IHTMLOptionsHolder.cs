// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F378-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short) 0x1040)]
    public interface IHTMLOptionsHolder
    {
        [DispId(0x5df)]
        IHTMLDocument2 document { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5df), TypeLibFunc((short) 0x40)] get; }
        [DispId(0x5e0)]
        IHTMLFontNamesCollection fonts { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40), DispId(0x5e0)] get; }
        [DispId(0x5e1)]
        object execArg { [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e1)] set; [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e1)] get; }
        [DispId(0x5e2)]
        int errorLine { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e2)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e2)] get; }
        [DispId(0x5e3)]
        int errorCharacter { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e3)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e3)] get; }
        [DispId(0x5e4)]
        int errorCode { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e4)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e4)] get; }
        [DispId(0x5e5)]
        string errorMessage { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e5)] set; [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e5)] get; }
        [DispId(0x5e6)]
        bool errorDebug { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e6)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e6)] get; }
        [DispId(0x5e7)]
        IHTMLWindow2 unsecuredWindowOfDocument { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e7), TypeLibFunc((short) 0x40)] get; }
        [DispId(0x5e8)]
        string findText { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e8)] set; [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e8)] get; }
        [DispId(0x5e9)]
        bool anythingAfterFrameset { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e9)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5e9)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5ea)]
        IHTMLFontSizesCollection sizes([In, MarshalAs(UnmanagedType.BStr)] string fontName);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5eb)]
        string openfiledlg([In, Optional, MarshalAs(UnmanagedType.Struct)] object initFile, [In, Optional, MarshalAs(UnmanagedType.Struct)] object initDir, [In, Optional, MarshalAs(UnmanagedType.Struct)] object filter, [In, Optional, MarshalAs(UnmanagedType.Struct)] object title);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5ec)]
        string savefiledlg([In, Optional, MarshalAs(UnmanagedType.Struct)] object initFile, [In, Optional, MarshalAs(UnmanagedType.Struct)] object initDir, [In, Optional, MarshalAs(UnmanagedType.Struct)] object filter, [In, Optional, MarshalAs(UnmanagedType.Struct)] object title);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5ed)]
        int choosecolordlg([In, Optional, MarshalAs(UnmanagedType.Struct)] object initColor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5ee)]
        void showSecurityInfo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5ef)]
        bool isApartmentModel([In, MarshalAs(UnmanagedType.Interface)] IHTMLObjectElement @object);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5f0)]
        int getCharset([In, MarshalAs(UnmanagedType.BStr)] string fontName);
        [DispId(0x5f1)]
        string secureConnectionInfo { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x5f1)] get; }
    }
}

