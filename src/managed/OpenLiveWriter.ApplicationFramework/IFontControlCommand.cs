// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.ApplicationFramework
{
    public interface IFontControlCommand
    {
        string FontFamily { get; }
        int FontSize { get; }
        FontProperties Bold { get; }
        FontProperties Italic { get; }
        FontProperties Underline { get; }
        FontProperties Strikethrough { get; }
        // @RIBBON TODO: Implement vertical positioning
        //FontPropertiesVerticalPositioning
        int ForegroundColor { get; }
        int BackgroundColor { get; }
    }
}
