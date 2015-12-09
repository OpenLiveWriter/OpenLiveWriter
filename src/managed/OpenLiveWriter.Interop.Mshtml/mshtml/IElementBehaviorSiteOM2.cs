// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F659-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IElementBehaviorSiteOM2 : IElementBehaviorSiteOM
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new int RegisterEvent([In, MarshalAs(UnmanagedType.LPWStr)] string pchEvent, [In] int lFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new int GetEventCookie([In, MarshalAs(UnmanagedType.LPWStr)] string pchEvent);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void FireEvent([In] int lCookie, [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObject);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new IHTMLEventObj CreateEventObject();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void RegisterName([In, MarshalAs(UnmanagedType.LPWStr)] string pchName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void RegisterUrn([In, MarshalAs(UnmanagedType.LPWStr)] string pchUrn);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        IHTMLElementDefaults GetDefaults();
    }
}

