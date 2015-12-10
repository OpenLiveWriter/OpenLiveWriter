// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Dispatch IDs for HTML element events
    /// </summary>
    public struct DISPID_HTMLELEMENTEVENTS2
    {
        // constants for base id's
        private const int DISPID_XOBJ_BASE = unchecked((int)0x80010000);
        private const int DISPID_NORMAL_FIRST = 1000;

        // event constants
        public const int ONHELP = DISPID_XOBJ_BASE + 10;
        public const int ONCLICK = -600;
        public const int ONDBLCLICK = -601;
        public const int ONKEYPRESS = -603;
        public const int ONKEYDOWN = -602;
        public const int ONKEYUP = -604;
        public const int ONMOUSEOUT = DISPID_XOBJ_BASE + 9;
        public const int ONMOUSEOVER = DISPID_XOBJ_BASE + 8;
        public const int ONMOUSEMOVE = -606;
        public const int ONMOUSEDOWN = -605;
        public const int ONMOUSEUP = -607;
        public const int ONSELECTSTART = DISPID_XOBJ_BASE + 12;
        public const int ONFILTERCHANGE = DISPID_XOBJ_BASE + 17;
        public const int ONDRAGSTART = DISPID_XOBJ_BASE + 11;
        public const int ONBEFOREUPDATE = DISPID_XOBJ_BASE + 4;
        public const int ONAFTERUPDATE = DISPID_XOBJ_BASE + 5;
        public const int ONERRORUPDATE = DISPID_XOBJ_BASE + 13;
        public const int ONROWEXIT = DISPID_XOBJ_BASE + 6;
        public const int ONROWENTER = DISPID_XOBJ_BASE + 7;
        public const int ONDATASETCHANGED = DISPID_XOBJ_BASE + 14;
        public const int ONDATAAVAILABLE = DISPID_XOBJ_BASE + 15;
        public const int ONDATASETCOMPLETE = DISPID_XOBJ_BASE + 16;
        public const int ONLOSECAPTURE = DISPID_XOBJ_BASE + 18;
        public const int ONPROPERTYCHANGE = DISPID_XOBJ_BASE + 19;
        public const int ONSCROLL = DISPID_NORMAL_FIRST + 14;
        public const int ONFOCUS = DISPID_XOBJ_BASE + 1;
        public const int ONBLUR = DISPID_XOBJ_BASE + 0;
        public const int ONRESIZE = DISPID_NORMAL_FIRST + 16;
        public const int ONDRAG = DISPID_XOBJ_BASE + 20;
        public const int ONDRAGEND = DISPID_XOBJ_BASE + 21;
        public const int ONDRAGENTER = DISPID_XOBJ_BASE + 22;
        public const int ONDRAGOVER = DISPID_XOBJ_BASE + 23;
        public const int ONDRAGLEAVE = DISPID_XOBJ_BASE + 24;
        public const int ONDROP = DISPID_XOBJ_BASE + 25;
        public const int ONBEFORECUT = DISPID_XOBJ_BASE + 29;
        public const int ONCUT = DISPID_XOBJ_BASE + 26;
        public const int ONBEFORECOPY = DISPID_XOBJ_BASE + 30;
        public const int ONCOPY = DISPID_XOBJ_BASE + 27;
        public const int ONBEFOREPASTE = DISPID_XOBJ_BASE + 31;
        public const int ONPASTE = DISPID_XOBJ_BASE + 28;
        public const int ONCONTEXTMENU = DISPID_NORMAL_FIRST + 23;
        public const int ONROWSDELETE = DISPID_XOBJ_BASE + 32;
        public const int ONROWSINSERTED = DISPID_XOBJ_BASE + 33;
        public const int ONCELLCHANGE = DISPID_XOBJ_BASE + 34;
        public const int ONREADYSTATECHANGE = -609;
        public const int ONLAYOUTCOMPLETE = DISPID_NORMAL_FIRST + 30;
        public const int ONPAGE = DISPID_NORMAL_FIRST + 31;
        public const int ONMOUSEENTER = DISPID_NORMAL_FIRST + 42;
        public const int ONMOUSELEAVE = DISPID_NORMAL_FIRST + 43;
        public const int ONACTIVATE = DISPID_NORMAL_FIRST + 44;
        public const int ONDEACTIVATE = DISPID_NORMAL_FIRST + 45;
        public const int ONBEFOREDEACTIVATE = DISPID_NORMAL_FIRST + 34;
        public const int ONBEFOREACTIVATE = DISPID_NORMAL_FIRST + 47;
        public const int ONFOCUSIN = DISPID_NORMAL_FIRST + 48;
        public const int ONFOCUSOUT = DISPID_NORMAL_FIRST + 49;
        public const int ONMOVE = DISPID_NORMAL_FIRST + 35;
        public const int ONCONTROLSELECT = DISPID_NORMAL_FIRST + 36;
        public const int ONMOVESTART = DISPID_NORMAL_FIRST + 38;
        public const int ONMOVEEND = DISPID_NORMAL_FIRST + 39;
        public const int ONRESIZESTART = DISPID_NORMAL_FIRST + 40;
        public const int ONRESIZEEND = DISPID_NORMAL_FIRST + 41;
        public const int ONMOUSEWHEEL = DISPID_NORMAL_FIRST + 33;

    }
}
