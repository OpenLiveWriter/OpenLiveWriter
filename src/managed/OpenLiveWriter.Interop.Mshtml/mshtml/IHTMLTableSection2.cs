// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F5C7-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLTableSection2
    {
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3eb)]
        object moveRow([In, Optional] int indexFrom /* = -1 */, [In, Optional] int indexTo /* = -1 */);
    }
}

