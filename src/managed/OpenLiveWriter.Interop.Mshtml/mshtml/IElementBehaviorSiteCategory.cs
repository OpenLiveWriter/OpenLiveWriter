// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F4EE-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IElementBehaviorSiteCategory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        IEnumUnknown GetRelatedBehaviors([In] int lDirection, [In, MarshalAs(UnmanagedType.LPWStr)] string pchCategory);
    }
}

