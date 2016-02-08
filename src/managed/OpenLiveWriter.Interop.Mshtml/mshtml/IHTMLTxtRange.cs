// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F220-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLTxtRange
    {
        [DispId(0x3eb)]
        string htmlText {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3eb)] get; }
        [DispId(0x3ec)]
        string text {[param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ec)] set;[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ec)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ee)]
        IHTMLElement parentElement();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f0)]
        IHTMLTxtRange duplicate();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f2)]
        bool inRange([In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange range);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f3)]
        bool isEqual([In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange range);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f4)]
        void scrollIntoView([In, Optional] bool fStart /* = true */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f5)]
        void collapse([In, Optional] bool Start /* = true */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f6)]
        bool expand([In, MarshalAs(UnmanagedType.BStr)] string Unit);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f7)]
        int move([In, MarshalAs(UnmanagedType.BStr)] string Unit, [In, Optional] int Count /* = 1 */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f8)]
        int moveStart([In, MarshalAs(UnmanagedType.BStr)] string Unit, [In, Optional] int Count /* = 1 */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f9)]
        int moveEnd([In, MarshalAs(UnmanagedType.BStr)] string Unit, [In, Optional] int Count /* = 1 */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x400)]
        void select();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x402)]
        void pasteHTML([In, MarshalAs(UnmanagedType.BStr)] string html);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3e9)]
        void moveToElementText([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement element);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x401)]
        void setEndPoint([In, MarshalAs(UnmanagedType.BStr)] string how, [In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange SourceRange);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fa)]
        int compareEndPoints([In, MarshalAs(UnmanagedType.BStr)] string how, [In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange SourceRange);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fb)]
        bool findText([In, MarshalAs(UnmanagedType.BStr)] string String, [In, Optional] int Count /* = 0x3fffffff */, [In, Optional] int Flags /* = 0 */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fc)]
        void moveToPoint([In] int x, [In] int y);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fd)]
        string getBookmark();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f1)]
        bool moveToBookmark([In, MarshalAs(UnmanagedType.BStr)] string Bookmark);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x403)]
        bool queryCommandSupported([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x404)]
        bool queryCommandEnabled([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x405)]
        bool queryCommandState([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x406)]
        bool queryCommandIndeterm([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x407)]
        string queryCommandText([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x408)]
        object queryCommandValue([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x409)]
        bool execCommand([In, MarshalAs(UnmanagedType.BStr)] string cmdID, [In, Optional] bool showUI /* = false */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40a)]
        bool execCommandShowHelp([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
    }
}

