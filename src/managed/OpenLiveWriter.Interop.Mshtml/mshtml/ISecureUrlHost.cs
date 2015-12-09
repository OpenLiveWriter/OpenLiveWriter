// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("C81984C4-74C8-11D2-BAA9-00C04FC2040E"), InterfaceType((short) 1)]
    public interface ISecureUrlHost
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ValidateSecureUrl(out int pfAllow, [In] ref ushort pchUrlInQuestion, [In] uint dwFlags);
    }
}

