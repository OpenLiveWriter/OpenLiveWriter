// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F489-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementBehaviorSiteOM
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int RegisterEvent([In, MarshalAs(UnmanagedType.LPWStr)] string pchEvent, [In] int lFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetEventCookie([In, MarshalAs(UnmanagedType.LPWStr)] string pchEvent);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FireEvent([In] int lCookie, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObject);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        IHTMLEventObj CreateEventObject();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterName([In, MarshalAs(UnmanagedType.LPWStr)] string pchName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterUrn([In, MarshalAs(UnmanagedType.LPWStr)] string pchUrn);
    }
}

