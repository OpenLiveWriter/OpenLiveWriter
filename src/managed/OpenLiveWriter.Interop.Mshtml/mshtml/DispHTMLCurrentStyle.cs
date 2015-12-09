// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1010), InterfaceType((short) 2), Guid("3050F557-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface DispHTMLCurrentStyle
    {
        [DispId(-2147413022)]
        string position { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413022), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413042)]
        string styleFloat { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413042), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413110)]
        object color { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413110)] get; }
        [DispId(-501)]
        object backgroundColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-501), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413094)]
        string fontFamily { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413094), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413088)]
        string fontStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413088)] get; }
        [DispId(-2147413087)]
        string fontVariant { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x54), DispId(-2147413087)] get; }
        [DispId(-2147413085)]
        object fontWeight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413085)] get; }
        [DispId(-2147413093)]
        object fontSize { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413093), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413111)]
        string backgroundImage { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413111), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413079)]
        object backgroundPositionX { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413079), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413078)]
        object backgroundPositionY { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413078)] get; }
        [DispId(-2147413068)]
        string backgroundRepeat { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413068)] get; }
        [DispId(-2147413054)]
        object borderLeftColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413054)] get; }
        [DispId(-2147413057)]
        object borderTopColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413057)] get; }
        [DispId(-2147413056)]
        object borderRightColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413056), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413055)]
        object borderBottomColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413055), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413047)]
        string borderTopStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413047)] get; }
        [DispId(-2147413046)]
        string borderRightStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413046)] get; }
        [DispId(-2147413045)]
        string borderBottomStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413045)] get; }
        [DispId(-2147413044)]
        string borderLeftStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413044), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413052)]
        object borderTopWidth { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413052)] get; }
        [DispId(-2147413051)]
        object borderRightWidth { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413051), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413050)]
        object borderBottomWidth { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413050)] get; }
        [DispId(-2147413049)]
        object borderLeftWidth { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413049)] get; }
        [DispId(-2147418109)]
        object left { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418109)] get; }
        [DispId(-2147418108)]
        object top { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418108), TypeLibFunc((short) 20)] get; }
        [DispId(-2147418107)]
        object width { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418107)] get; }
        [DispId(-2147418106)]
        object height { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418106), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413097)]
        object paddingLeft { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413097), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413100)]
        object paddingTop { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413100)] get; }
        [DispId(-2147413099)]
        object paddingRight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413099), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413098)]
        object paddingBottom { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413098)] get; }
        [DispId(-2147418040)]
        string textAlign { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418040)] get; }
        [DispId(-2147413077)]
        string textDecoration { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413077)] get; }
        [DispId(-2147413041)]
        string display { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413041), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413032)]
        string visibility { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413032), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413021)]
        object zIndex { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413021), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413104)]
        object letterSpacing { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413104), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413106)]
        object lineHeight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413106)] get; }
        [DispId(-2147413105)]
        object textIndent { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413105)] get; }
        [DispId(-2147413064)]
        object verticalAlign { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413064)] get; }
        [DispId(-2147413067)]
        string backgroundAttachment { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413067), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413075)]
        object marginTop { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413075)] get; }
        [DispId(-2147413074)]
        object marginRight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413074)] get; }
        [DispId(-2147413073)]
        object marginBottom { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413073), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413072)]
        object marginLeft { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413072), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413096)]
        string clear { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413096), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413040)]
        string listStyleType { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413040)] get; }
        [DispId(-2147413039)]
        string listStylePosition { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413039)] get; }
        [DispId(-2147413038)]
        string listStyleImage { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413038)] get; }
        [DispId(-2147413019)]
        object clipTop { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413019)] get; }
        [DispId(-2147413018)]
        object clipRight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413018), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413017)]
        object clipBottom { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413017)] get; }
        [DispId(-2147413016)]
        object clipLeft { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413016)] get; }
        [DispId(-2147413102)]
        string overflow { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413102)] get; }
        [DispId(-2147413035)]
        string pageBreakBefore { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413035)] get; }
        [DispId(-2147413034)]
        string pageBreakAfter { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413034)] get; }
        [DispId(-2147413010)]
        string cursor { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413010)] get; }
        [DispId(-2147413014)]
        string tableLayout { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413014)] get; }
        [DispId(-2147413028)]
        string borderCollapse { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413028)] get; }
        [DispId(-2147412993)]
        string direction { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412993)] get; }
        [DispId(-2147412997)]
        string behavior { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412997)] get; }
        [return: MarshalAs(UnmanagedType.Struct)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417610)]
        object getAttribute([In, MarshalAs(UnmanagedType.BStr)] string strAttributeName, [In, Optional] int lFlags /* = 0 */);
        [DispId(-2147412994)]
        string unicodeBidi { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412994)] get; }
        [DispId(-2147418035)]
        object right { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418035), TypeLibFunc((short) 20)] get; }
        [DispId(-2147418034)]
        object bottom { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418034)] get; }
        [DispId(-2147412992)]
        string imeMode { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412992), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412991)]
        string rubyAlign { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412991)] get; }
        [DispId(-2147412990)]
        string rubyPosition { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412990), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412989)]
        string rubyOverhang { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412989)] get; }
        [DispId(-2147412980)]
        string textAutospace { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412980)] get; }
        [DispId(-2147412979)]
        string lineBreak { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412979), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412978)]
        string wordBreak { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412978), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412977)]
        string textJustify { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412977)] get; }
        [DispId(-2147412976)]
        string textJustifyTrim { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412976), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412975)]
        object textKashida { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412975), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412995)]
        string blockDirection { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412995), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412985)]
        object layoutGridChar { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412985), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412984)]
        object layoutGridLine { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412984)] get; }
        [DispId(-2147412983)]
        string layoutGridMode { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412983)] get; }
        [DispId(-2147412982)]
        string layoutGridType { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412982)] get; }
        [DispId(-2147413048)]
        string borderStyle { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413048)] get; }
        [DispId(-2147413058)]
        string borderColor { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413058)] get; }
        [DispId(-2147413053)]
        string borderWidth { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413053)] get; }
        [DispId(-2147413101)]
        string padding { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413101), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413076)]
        string margin { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413076)] get; }
        [DispId(-2147412965)]
        string accelerator { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412965)] get; }
        [DispId(-2147412973)]
        string overflowX { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412973)] get; }
        [DispId(-2147412972)]
        string overflowY { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412972), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413108)]
        string textTransform { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413108)] get; }
        [DispId(-2147412957)]
        string layoutFlow { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412957), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412954)]
        string wordWrap { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412954), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412953)]
        string textUnderlinePosition { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412953)] get; }
        [DispId(-2147412952)]
        bool hasLayout { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412952), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412932)]
        object scrollbarBaseColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412932)] get; }
        [DispId(-2147412931)]
        object scrollbarFaceColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412931)] get; }
        [DispId(-2147412930)]
        object scrollbar3dLightColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412930)] get; }
        [DispId(-2147412929)]
        object scrollbarShadowColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412929)] get; }
        [DispId(-2147412928)]
        object scrollbarHighlightColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412928)] get; }
        [DispId(-2147412927)]
        object scrollbarDarkShadowColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412927)] get; }
        [DispId(-2147412926)]
        object scrollbarArrowColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412926)] get; }
        [DispId(-2147412916)]
        object scrollbarTrackColor { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412916), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412920)]
        string writingMode { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412920), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412959)]
        object zoom { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412959), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413030)]
        string filter { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413030), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412909)]
        string textAlignLast { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412909), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412908)]
        object textKashidaSpace { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412908)] get; }
        [DispId(-2147412904)]
        bool isBlock { [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x455), DispId(-2147412904)] get; }
        [DispId(-2147412903)]
        string textOverflow { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412903), TypeLibFunc((short) 20)] get; }
        [DispId(-2147412901)]
        object minHeight { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412901)] get; }
        [DispId(-2147413065)]
        object wordSpacing { [return: MarshalAs(UnmanagedType.Struct)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413065), TypeLibFunc((short) 20)] get; }
        [DispId(-2147413036)]
        string whiteSpace { [return: MarshalAs(UnmanagedType.BStr)] [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413036), TypeLibFunc((short) 20)] get; }
    }
}

