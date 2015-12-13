// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Generic interface for interacting with Mshtml commands
    /// </summary>
    public interface IMshtmlCommand
    {
        /// <summary>
        /// Is the command enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Is the command latched
        /// </summary>
        bool Latched { get; }

        /// <summary>
        /// Execute the command with no input parameters
        /// </summary>
        void Execute();

        /// <summary>
        /// Execute the command with no input parameters
        /// </summary>
        void Execute(OLECMDEXECOPT execOption);

        /// <summary>
        /// Execute the command with an input parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        void Execute(object input);

        /// <summary>
        /// Execute the command with an input parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        void Execute(OLECMDEXECOPT execOption, object input);

        /// <summary>
        /// Execute the command with an input and output parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        /// <param name="output">output parameter</param>
        void Execute(object input, ref object output);

        /// <summary>
        /// Execute the command with an input and output parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        /// <param name="output">output parameter</param>
        void Execute(OLECMDEXECOPT execOption, object input, ref object output);

        /// <summary>
        /// Get the value of the command
        /// </summary>
        /// <returns></returns>
        object GetValue();
    }

    /// <summary>
    /// Core command set exposed by MSHTML
    /// </summary>
    public class MshtmlCoreCommandSet : Dictionary<uint, IMshtmlCommand>
    {
        /// <summary>
        /// Initialize with a command target
        /// </summary>
        /// <param name="target">command target</param>
        public MshtmlCoreCommandSet(IOleCommandTargetWithExecParams target)
        {
            // keep a copy of the command target
            commandTarget = target;

            // add all of the standard commands
            AddCommand(IDM._1D_ELEMENT);
            AddCommand(IDM._2D_ELEMENT);
            AddCommand(IDM._2D_POSITION);
            AddCommand(IDM.ABSOLUTE_POSITION);
            AddCommand(IDM.ADDFAVORITES);
            AddCommand(IDM.ADDTOGLYPHTABLE);
            AddCommand(IDM.ATOMICSELECTION);
            AddCommand(IDM.AUTOURLDETECT_MODE);
            AddCommand(IDM.BACKCOLOR);
            AddCommand(IDM.BLOCKFMT);
            AddCommand(IDM.BOLD);
            AddCommand(IDM.BOOKMARK);
            AddCommand(IDM.BROWSEMODE);
            AddCommand(IDM.BUTTON);
            AddCommand(IDM.CHECKBOX);
            AddCommand(IDM.CLEARAUTHENTICATIONCACHE);
            AddCommand(IDM.CLEARSELECTION);
            AddCommand(IDM.COMPOSESETTINGS, true);
            AddCommand(IDM.COPY);
            AddCommand(IDM.CSSEDITING_LEVEL);
            AddCommand(IDM.CUT);
            AddCommand(IDM.DELETE);
            AddCommand(IDM.DISABLE_EDITFOCUS_UI);
            AddCommand(IDM.DROPDOWNBOX);
            AddCommand(IDM.EMPTYGLYPHTABLE);
            AddCommand(IDM.FIND);
            AddCommand(IDM.FONT);
            AddCommand(IDM.FONTNAME);
            AddCommand(IDM.FONTSIZE);
            AddCommand(IDM.FORECOLOR);
            AddCommand(IDM.GETBLOCKFMTS);
            AddCommand(IDM.GETFRAMEZONE);
            AddCommand(IDM.HORIZONTALLINE);
            AddCommand(IDM.HTMLEDITMODE);
            AddCommand(IDM.HYPERLINK);
            AddCommand(IDM.IFRAME);
            AddCommand(IDM.IMAGE);
            AddCommand(IDM.IME_ENABLE_RECONVERSION, true);
            AddCommand(IDM.INDENT);
            AddCommand(IDM.INSFIELDSET);
            AddCommand(IDM.INSINPUTBUTTON);
            AddCommand(IDM.INSINPUTHIDDEN);
            AddCommand(IDM.INSINPUTIMAGE);
            AddCommand(IDM.INSINPUTPASSWORD);
            AddCommand(IDM.INSINPUTRESET);
            AddCommand(IDM.INSINPUTSUBMIT);
            AddCommand(IDM.INSINPUTUPLOAD);
            AddCommand(IDM.ITALIC);
            AddCommand(IDM.JUSTIFYNONE);
            AddCommand(IDM.JUSTIFYCENTER);
            AddCommand(IDM.JUSTIFYLEFT);
            AddCommand(IDM.JUSTIFYRIGHT);
            AddCommand(IDM.JUSTIFYFULL);
            AddCommand(IDM.KEEPSELECTION);
            AddCommand(IDM.LISTBOX);
            AddCommand(IDM.LIVERESIZE);
            AddCommand(IDM.MARQUEE);
            AddCommand(IDM.MULTIPLESELECTION);
            AddCommand(IDM.NOFIXUPURLSONPASTE);
            AddCommand(IDM.ORDERLIST);
            AddCommand(IDM.OUTDENT);
            AddCommand(IDM.OVERWRITE);
            AddCommand(IDM.OVERRIDE_CURSOR);
            AddCommand(IDM.PAGESETUP);
            AddCommand(IDM.PARAGRAPH);
            AddCommand(IDM.PASTE);
            AddCommand(IDM.PRINT);
            AddCommand(IDM.PRINTPREVIEW);
            AddCommand(IDM.RADIOBUTTON);
            AddCommand(IDM.REDO);
            AddCommand(IDM.REFRESH);
            AddCommand(IDM.REMOVEFORMAT);
            AddCommand(IDM.RESPECTVISIBILITY_INDESIGN);
            AddCommand(IDM.SAVE);
            AddCommand(IDM.SAVEAS);
            AddCommand(IDM.SELECTALL);
            AddCommand(IDM.SHOWALLTAGS);
            AddCommand(IDM.SHOWHIDE_CODE);
            AddCommand(IDM.SHOWZEROBORDERATDESIGNTIME);
            AddCommand(IDM.STRIKETHROUGH);
            AddCommand(IDM.TEXTAREA);
            AddCommand(IDM.UNBOOKMARK);
            AddCommand(IDM.UNDERLINE);
            AddCommand(IDM.UNDO);
            AddCommand(IDM.UNLINK);
            AddCommand(IDM.UNORDERLIST);
            AddCommand(IDM.VIEWSOURCE);
        }

        /// <summary>
        /// Helper method to add a command to the standard command set
        /// </summary>
        /// <param name="cmdID"></param>
        private void AddCommand(uint cmdID)
        {
            AddCommand(cmdID, false);
        }

        /// <summary>
        /// Helper method to add a command to the standard command set
        /// </summary>
        /// <param name="cmdID"></param>
        private void AddCommand(uint cmdID, bool useNullOutputParam)
        {
            Add(cmdID, new MshtmlCommandFromCoreSet(cmdID, commandTarget, useNullOutputParam));
        }

        /// <summary>
        /// Command target that we execute our commands against
        /// </summary>
        private IOleCommandTargetWithExecParams commandTarget;
    }

    /// <summary>
    /// Class which implements access to an MSHTML command that is defined as
    /// part of the core command set
    /// </summary>
    internal class MshtmlCommandFromCoreSet : IMshtmlCommand
    {
        /// <summary>
        /// Initialize based on a command id and the document to which
        /// to send commands to
        /// </summary>
        /// <param name="cmdID">mshtml command id (IDM_)</param>
        /// <param name="target">command target</param>
        public MshtmlCommandFromCoreSet(uint cmdID, IOleCommandTargetWithExecParams target, bool useNullOutputParam)
        {
            commandID = cmdID;
            commandTarget = target;
            getValueCommandTarget = (IOleCommandTargetGetCommandValue)target;
            UseNullOutputParam = useNullOutputParam;
        }

        private readonly bool UseNullOutputParam;

        /// <summary>
        /// Check whether the command is supported by the document
        /// </summary>
        public bool Supported
        {
            get
            {
                if ((GetCommandStatus() & OLECMDF.SUPPORTED) > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Check whether the command is currently enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                if ((GetCommandStatus() & OLECMDF.ENABLED) > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Check whether the command is currently latched
        /// </summary>
        public bool Latched
        {
            get
            {
                if ((GetCommandStatus() & OLECMDF.LATCHED) > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Execute the command without parameters
        /// </summary>
        public virtual void Execute()
        {
            Execute(null);
        }

        /// <summary>
        /// Execute the command without parameters
        /// </summary>
        public virtual void Execute(OLECMDEXECOPT execOption)
        {
            Execute(execOption, null);
        }

        /// <summary>
        /// Execute the command with an input parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        public virtual void Execute(object input)
        {
            object output = null;
            Execute(input, ref output);
        }

        /// <summary>
        /// Execute the command with an input parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        public virtual void Execute(OLECMDEXECOPT execOption, object input)
        {
            object output = null;
            Execute(execOption, input, ref output);
        }

        /// <summary>
        /// Execute the command with an input and an output parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        /// <param name="output">output parameter</param>
        public virtual void Execute(object input, ref object output)
        {
            Execute(OLECMDEXECOPT.DODEFAULT, input, ref output);
        }

        /// <summary>
        /// Execute the command with an input and an output parameter
        /// </summary>
        /// <param name="input">input parameter</param>
        /// <param name="output">output parameter</param>
        public virtual void Execute(OLECMDEXECOPT execOption, object input, ref object output)
        {
            if (UseNullOutputParam)
            {
                ((IOleCommandTargetNullOutputParam)commandTarget).Exec(CGID.MSHTML, commandID, execOption, ref input, IntPtr.Zero);
            }
            else
            {
                commandTarget.Exec(CGID.MSHTML, commandID, execOption, ref input, ref output);
            }

        }

        /// <summary>
        /// Get the value of the command
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            object output = new object();
            getValueCommandTarget.Exec(
                CGID.MSHTML, commandID, OLECMDEXECOPT.DODEFAULT,
                IntPtr.Zero, ref output);
            return output;
        }

        /// <summary>
        /// Helper method to query for the current status (latched, enabled, etc.)
        /// of the command
        /// </summary>
        /// <returns>flags representing current status</returns>
        private OLECMDF GetCommandStatus()
        {
            // query for the command status
            OLECMD cmd = new OLECMD();
            cmd.cmdID = commandID;
            try
            {
                commandTarget.QueryStatus(
                    CGID.MSHTML, 1, ref cmd, IntPtr.Zero);
            }
            catch
            {
                // this can throw exceptions at unexpected times such as managing
                // command-state when a flash-player is in the editor
            }

            // return the command flags
            return cmd.cmdf;
        }

        // underlying command-id
        protected uint commandID;

        // underlying command target references
        protected IOleCommandTargetWithExecParams commandTarget;
        protected IOleCommandTargetGetCommandValue getValueCommandTarget;

    }

    /// <summary>
    /// Constant representing the Guid of the MSHTML core command set
    /// </summary>
    public class CGID
    {
        public static Guid MSHTML = new Guid("DE4BA900-59CA-11CF-9592-444553540000");
    }

    /// <summary>
    /// Underlying MSHTML command ids. Note that this all of the command id's
    /// listed in the MsHtmlcid.h header file are listed below. Most however
    /// are commented out as they are not documented as part of the official
    /// MSHTML Command Identifiers list. It is likely that many of them will
    /// work correctly -- as we experiement with them and verify their correct
    /// usage we should comment them out and add a brief documentation note
    /// on how to use them correctly.
    /// </summary>
    public struct IDM
    {
        //		public const uint UNKNOWN =                0 ;
        //		public const uint ALIGNBOTTOM =            1 ;
        //		public const uint ALIGNHORIZONTALCENTERS = 2 ;
        //		public const uint ALIGNLEFT =              3 ;
        //		public const uint ALIGNRIGHT =             4 ;
        //		public const uint ALIGNTOGRID =            5 ;
        //		public const uint ALIGNTOP =               6 ;
        //		public const uint ALIGNVERTICALCENTERS =   7 ;
        //		public const uint ARRANGEBOTTOM =          8 ;
        //		public const uint ARRANGERIGHT =           9 ;
        //		public const uint BRINGFORWARD =           10 ;
        //		public const uint BRINGTOFRONT =           11 ;
        //		public const uint CENTERHORIZONTALLY =     12 ;
        //		public const uint CENTERVERTICALLY =       13 ;
        //		public const uint CODE =                   14 ;
        public const uint DELETE = 17;
        public const uint FONTNAME = 18;
        public const uint FONTSIZE = 19;
        //		public const uint GROUP =                  20 ;
        //		public const uint HORIZSPACECONCATENATE =  21 ;
        //		public const uint HORIZSPACEDECREASE =     22 ;
        //		public const uint HORIZSPACEINCREASE =     23 ;
        //		public const uint HORIZSPACEMAKEEQUAL =    24 ;
        //		public const uint INSERTOBJECT =           25 ;
        //		public const uint MULTILEVELREDO =         30 ;
        //		public const uint SENDBACKWARD =           32 ;
        //		public const uint SENDTOBACK =             33 ;
        //		public const uint SHOWTABLE =              34 ;
        //		public const uint SIZETOCONTROL =          35 ;
        //		public const uint SIZETOCONTROLHEIGHT =    36 ;
        //		public const uint SIZETOCONTROLWIDTH =     37 ;
        //		public const uint SIZETOFIT =              38 ;
        //		public const uint SIZETOGRID =             39 ;
        //		public const uint SNAPTOGRID =             40 ;
        //		public const uint TABORDER =               41 ;
        //		public const uint TOOLBOX =                42 ;
        //		public const uint MULTILEVELUNDO =         44 ;
        //		public const uint UNGROUP =                45 ;
        //		public const uint VERTSPACECONCATENATE =   46 ;
        //		public const uint VERTSPACEDECREASE =      47 ;
        //		public const uint VERTSPACEINCREASE =      48 ;
        //		public const uint VERTSPACEMAKEEQUAL =     49 ;
        public const uint JUSTIFYFULL = 50;
        public const uint BACKCOLOR = 51;
        public const uint BOLD = 52;
        //		public const uint BORDERCOLOR =            53 ;
        //		public const uint FLAT =                   54 ;
        public const uint FORECOLOR = 55;
        public const uint ITALIC = 56;
        public const uint JUSTIFYCENTER = 57;
        public const uint JUSTIFYGENERAL = 58;
        public const uint JUSTIFYLEFT = 59;
        public const uint JUSTIFYRIGHT = 60;
        //		public const uint RAISED =                 61 ;
        //		public const uint SUNKEN =                 62 ;
        public const uint UNDERLINE = 63;
        //		public const uint CHISELED =               64 ;
        //		public const uint ETCHED =                 65 ;
        //		public const uint SHADOWED =               66 ;
        public const uint FIND = 67;
        //		public const uint SHOWGRID =               69 ;
        //		public const uint OBJECTVERBLIST0 =        72 ;
        //		public const uint OBJECTVERBLIST1 =        73 ;
        //		public const uint OBJECTVERBLIST2 =        74 ;
        //		public const uint OBJECTVERBLIST3 =        75 ;
        //		public const uint OBJECTVERBLIST4 =        76 ;
        //		public const uint OBJECTVERBLIST5 =        77 ;
        //		public const uint OBJECTVERBLIST6 =        78 ;
        //		public const uint OBJECTVERBLIST7 =        79 ;
        //		public const uint OBJECTVERBLIST8 =        80 ;
        //		public const uint OBJECTVERBLIST9 =        81 ;
        //		public const uint OBJECTVERBLISTLAST = IDM.OBJECTVERBLIST9 ;
        //		public const uint CONVERTOBJECT =          82 ;
        //		public const uint CUSTOMCONTROL =          83 ;
        //		public const uint CUSTOMIZEITEM =          84 ;
        //		public const uint RENAME =                 85 ;
        //		public const uint IMPORT =                 86 ;
        //		public const uint NEWPAGE =                87 ;
        //		public const uint MOVE =                   88 ;
        //		public const uint CANCEL =                 89 ;
        public const uint FONT = 90;
        public const uint STRIKETHROUGH = 91;
        //		public const uint DELETEWORD =             92 ;
        //		public const uint EXECPRINT =              93 ;
        public const uint JUSTIFYNONE = 94;
        //		public const uint TRISTATEBOLD =           95 ;
        //		public const uint TRISTATEITALIC =         96 ;
        //		public const uint TRISTATEUNDERLINE =      97 ;

        //		public const uint FOLLOW_ANCHOR =          2008 ;

        public const uint INSINPUTIMAGE = 2114;
        public const uint INSINPUTBUTTON = 2115;
        public const uint INSINPUTRESET = 2116;
        public const uint INSINPUTSUBMIT = 2117;
        public const uint INSINPUTUPLOAD = 2118;
        public const uint INSFIELDSET = 2119;

        //		public const uint PASTEINSERT =            2120 ;
        //      public const uint REPLACE =                2121 ;
        //      public const uint EDITSOURCE =             2122 ;
        public const uint BOOKMARK = 2123;
        public const uint HYPERLINK = 2124;
        public const uint UNLINK = 2125;
        public const uint BROWSEMODE = 2126;
        //		public const uint EDITMODE =               2127 ;
        public const uint UNBOOKMARK = 2128;

        //		public const uint TOOLBARS =               2130 ;
        //		public const uint STATUSBAR =              2131 ;
        //		public const uint FORMATMARK =             2132 ;
        //		public const uint TEXTONLY =               2133 ;
        //		public const uint OPTIONS =                2135 ;
        //		public const uint FOLLOWLINKC =            2136 ;
        //		public const uint FOLLOWLINKN =            2137 ;
        public const uint VIEWSOURCE = 2139;
        //		public const uint ZOOMPOPUP =              2140 ;

        // IDM_BASELINEFONT1, IDM_BASELINEFONT2, IDM_BASELINEFONT3, IDM_BASELINEFONT4,
        // and IDM_BASELINEFONT5 should be consecutive integers;
        //
        //		public const uint BASELINEFONT1 =          2141 ;
        //		public const uint BASELINEFONT2 =          2142 ;
        //		public const uint BASELINEFONT3 =          2143 ;
        //		public const uint BASELINEFONT4 =          2144 ;
        //		public const uint BASELINEFONT5 =          2145 ;

        public const uint HORIZONTALLINE = 2150;
        //		public const uint LINEBREAKNORMAL =        2151 ;
        //		public const uint LINEBREAKLEFT =          2152 ;
        //		public const uint LINEBREAKRIGHT =         2153 ;
        //		public const uint LINEBREAKBOTH =          2154 ;
        //		public const uint NONBREAK =               2155 ;
        //		public const uint SPECIALCHAR =            2156 ;
        //		public const uint HTMLSOURCE =             2157 ;
        public const uint IFRAME = 2158;
        //		public const uint HTMLCONTAIN =            2159 ;
        public const uint TEXTBOX = 2161;
        public const uint TEXTAREA = 2162;
        public const uint CHECKBOX = 2163;
        public const uint RADIOBUTTON = 2164;
        public const uint DROPDOWNBOX = 2165;
        public const uint LISTBOX = 2166;
        public const uint BUTTON = 2167;
        public const uint IMAGE = 2168;
        //		public const uint OBJECT =                 2169 ;
        //		public const uint _1D =                     2170 ;
        //		public const uint IMAGEMAP =               2171 ;
        //		public const uint FILE =                   2172 ;
        //		public const uint COMMENT =                2173 ;
        //		public const uint SCRIPT =                 2174 ;
        //		public const uint JAVAAPPLET =             2175 ;
        //		public const uint PLUGIN =                 2176 ;
        //		public const uint PAGEBREAK =              2177 ;
        //		public const uint HTMLAREA =               2178 ;

        public const uint PARAGRAPH = 2180;
        //		public const uint FORM =                   2181 ;
        public const uint MARQUEE = 2182;
        //		public const uint LIST =                   2183 ;
        public const uint ORDERLIST = 2184;
        public const uint UNORDERLIST = 2185;
        public const uint INDENT = 2186;
        public const uint OUTDENT = 2187;
        //		public const uint PREFORMATTED =           2188 ;
        //		public const uint ADDRESS =                2189 ;
        //		public const uint BLINK =                  2190 ;
        //		public const uint DIV =                    2191 ;

        //		public const uint TABLEINSERT =            2200 ;
        //		public const uint RCINSERT =               2201 ;
        //		public const uint CELLINSERT =             2202 ;
        //		public const uint CAPTIONINSERT =          2203 ;
        //		public const uint CELLMERGE =              2204 ;
        //		public const uint CELLSPLIT =              2205 ;
        //		public const uint CELLSELECT =             2206 ;
        //		public const uint ROWSELECT =              2207 ;
        //		public const uint COLUMNSELECT =           2208 ;
        //		public const uint TABLESELECT =            2209 ;
        //		public const uint TABLEPROPERTIES =        2210 ;
        //		public const uint CELLPROPERTIES =         2211 ;
        //		public const uint ROWINSERT =              2212 ;
        //		public const uint COLUMNINSERT =           2213 ;

        //		public const uint HELP_CONTENT =           2220 ;
        //		public const uint HELP_ABOUT =             2221 ;
        //		public const uint HELP_README =            2222 ;

        public const uint REMOVEFORMAT = 2230;
        //		public const uint PAGEINFO =               2231 ;
        //		public const uint TELETYPE =               2232 ;
        public const uint GETBLOCKFMTS = 2233;
        public const uint BLOCKFMT = 2234;
        public const uint SHOWHIDE_CODE = 2235;
        //		public const uint TABLE =                  2236 ;

        //		public const uint COPYFORMAT =             2237 ;
        //		public const uint PASTEFORMAT =            2238 ;
        //		public const uint GOTO =                   2239 ;

        //		public const uint CHANGEFONT =             2240 ;
        //		public const uint CHANGEFONTSIZE =         2241 ;
        //		public const uint CHANGECASE =             2246 ;
        //		public const uint SHOWSPECIALCHAR =        2249 ;

        //		public const uint SUBSCRIPT =              2247 ;
        //		public const uint SUPERSCRIPT =            2248 ;

        //		public const uint CENTERALIGNPARA =        2250 ;
        //		public const uint LEFTALIGNPARA =          2251 ;
        //		public const uint RIGHTALIGNPARA =         2252 ;
        //		public const uint REMOVEPARAFORMAT =       2253 ;
        //		public const uint APPLYNORMAL =            2254 ;
        //		public const uint APPLYHEADING1 =          2255 ;
        //		public const uint APPLYHEADING2 =          2256 ;
        //		public const uint APPLYHEADING3 =          2257 ;

        //		public const uint DOCPROPERTIES =          2260 ;
        public const uint ADDFAVORITES = 2261;
        //		public const uint COPYSHORTCUT =           2262 ;
        //		public const uint SAVEBACKGROUND =         2263 ;
        //		public const uint SETWALLPAPER =           2264 ;
        //		public const uint COPYBACKGROUND =         2265 ;
        //		public const uint CREATESHORTCUT =         2266 ;
        //		public const uint PAGE =                   2267 ;
        //		public const uint SAVETARGET =             2268 ;
        //		public const uint SHOWPICTURE =            2269 ;
        //		public const uint SAVEPICTURE =            2270 ;
        //		public const uint DYNSRCPLAY =             2271 ;
        //		public const uint DYNSRCSTOP =             2272 ;
        //		public const uint PRINTTARGET =            2273 ;
        //		public const uint IMGARTPLAY =             2274 ;
        //		public const uint IMGARTSTOP =             2275 ;
        //		public const uint IMGARTREWIND =           2276 ;
        //		public const uint PRINTQUERYJOBSPENDING =  2277 ;
        //		public const uint SETDESKTOPITEM =         2278 ;

        //		public const uint CONTEXTMENU =            2280 ;
        //		public const uint GOBACKWARD =             2282 ;
        //		public const uint GOFORWARD =              2283 ;
        //		public const uint PRESTOP =                2284 ;
        //		public const uint MP_MYPICS =              2287 ;
        //		public const uint MP_EMAILPICTURE =        2288 ;
        //		public const uint MP_PRINTPICTURE =        2289 ;

        //		public const uint CREATELINK =             2290 ;
        //		public const uint COPYCONTENT =            2291 ;

        //		public const uint LANGUAGE =               2292 ;

        //		public const uint GETPRINTTEMPLATE =       2295 ;
        //		public const uint SETPRINTTEMPLATE =       2296 ;
        //		public const uint TEMPLATE_PAGESETUP =     2298 ;

        public const uint REFRESH = 2300;
        //		public const uint STOPDOWNLOAD =           2301 ;

        //		public const uint ENABLE_INTERACTION =     2302 ;

        //		public const uint LAUNCHDEBUGGER =         2310 ;
        //		public const uint BREAKATNEXT =            2311 ;

        public const uint INSINPUTHIDDEN = 2312;
        public const uint INSINPUTPASSWORD = 2313;

        public const uint OVERWRITE = 2314;

        //		public const uint PARSECOMPLETE =          2315 ;

        public const uint HTMLEDITMODE = 2316;

        public const uint REGISTRYREFRESH = 2317;
        public const uint COMPOSESETTINGS = 2318;

        public const uint SHOWALLTAGS = 2327;
        //		public const uint SHOWALIGNEDSITETAGS =    2321 ;
        //		public const uint SHOWSCRIPTTAGS =         2322 ;
        //		public const uint SHOWSTYLETAGS =          2323 ;
        //		public const uint SHOWCOMMENTTAGS =        2324 ;
        //		public const uint SHOWAREATAGS =           2325 ;
        //		public const uint SHOWUNKNOWNTAGS =        2326 ;
        //		public const uint SHOWMISCTAGS =           2320 ;
        public const uint SHOWZEROBORDERATDESIGNTIME = 2328;

        //		public const uint AUTODETECT =             2329 ;

        //		public const uint SCRIPTDEBUGGER =         2330 ;

        //		public const uint GETBYTESDOWNLOADED =     2331 ;

        //		public const uint NOACTIVATENORMALOLECONTROLS =       2332 ;
        //		public const uint NOACTIVATEDESIGNTIMECONTROLS =      2333 ;
        //		public const uint NOACTIVATEJAVAAPPLETS =             2334 ;
        public const uint NOFIXUPURLSONPASTE = 2335;

        public const uint EMPTYGLYPHTABLE = 2336;
        public const uint ADDTOGLYPHTABLE = 2337;
        //		public const uint REMOVEFROMGLYPHTABLE =   2338 ;
        //		public const uint REPLACEGLYPHCONTENTS =   2339 ;

        //		public const uint SHOWWBRTAGS =            2340 ;

        //		public const uint PERSISTSTREAMSYNC =      2341 ;
        //		public const uint SETDIRTY =               2342 ;

        //		public const uint RUNURLSCRIPT =           2343 ;

        //		public const uint ZOOMRATIO =              2344 ;
        //		public const uint GETZOOMNUMERATOR =       2345 ;
        //		public const uint GETZOOMDENOMINATOR =     2346 ;

        // COMMANDS FOR COMPLEX TEXT
        //		public const uint DIRLTR =                 2350 ;
        //		public const uint DIRRTL =                 2351 ;
        //		public const uint BLOCKDIRLTR =            2352 ;
        //		public const uint BLOCKDIRRTL =            2353 ;
        //		public const uint INLINEDIRLTR =           2354 ;
        //		public const uint INLINEDIRRTL =           2355 ;

        // SHDOCVW
        //		public const uint ISTRUSTEDDLG =           2356 ;

        // MSHTMLED
        //		public const uint INSERTSPAN =             2357 ;
        //		public const uint LOCALIZEEDITOR =         2358 ;

        // XML MIMEVIEWER
        //		public const uint SAVEPRETRANSFORMSOURCE = 2370 ;
        //		public const uint VIEWPRETRANSFORMSOURCE = 2371 ;

        // Scrollbar context menu
        //		public const uint SCROLL_HERE =            2380 ;
        //		public const uint SCROLL_TOP =             2381 ;
        //		public const uint SCROLL_BOTTOM =          2382 ;
        //		public const uint SCROLL_PAGEUP =          2383 ;
        //		public const uint SCROLL_PAGEDOWN =        2384 ;
        //		public const uint SCROLL_UP =              2385 ;
        //		public const uint SCROLL_DOWN =            2386 ;
        //		public const uint SCROLL_LEFTEDGE =        2387 ;
        //		public const uint SCROLL_RIGHTEDGE =       2388 ;
        //		public const uint SCROLL_PAGELEFT =        2389 ;
        //		public const uint SCROLL_PAGERIGHT =       2390 ;
        //		public const uint SCROLL_LEFT =            2391 ;
        //		public const uint SCROLL_RIGHT =           2392 ;

        // IE 6 Form Editing Commands
        public const uint MULTIPLESELECTION = 2393;
        public const uint _2D_POSITION = 2394;
        public const uint _2D_ELEMENT = 2395;
        public const uint _1D_ELEMENT = 2396;
        public const uint ABSOLUTE_POSITION = 2397;
        public const uint LIVERESIZE = 2398;
        public const uint ATOMICSELECTION = 2399;

        // Auto URL detection mode
        public const uint AUTOURLDETECT_MODE = 2400;

        // Legacy IE50 compatible paste
        //		public const uint IE50_PASTE =             2401 ;

        // ie50 paste mode
        //		public const uint IE50_PASTE_MODE =        2402 ;

        //;begin_internal
        //		public const uint GETIPRINT =              2403 ;
        //;end_internal

        // for disabling selection handles
        public const uint DISABLE_EDITFOCUS_UI = 2404;

        // for visibility/display in design
        public const uint RESPECTVISIBILITY_INDESIGN = 2405;

        // set css mode
        public const uint CSSEDITING_LEVEL = 2406;

        // New outdent
        //		public const uint UI_OUTDENT =             2407 ;

        // Printing Status
        //		public const uint UPDATEPAGESTATUS =       2408 ;

        // IME Reconversion
        public const uint IME_ENABLE_RECONVERSION = 2409;

        public const uint KEEPSELECTION = 2410;

        //		public const uint UNLOADDOCUMENT =         2411 ;

        public const uint OVERRIDE_CURSOR = 2420;

        //		public const uint PEERHITTESTSAMEINEDIT =  2423 ;

        //		public const uint TRUSTAPPCACHE =          2425 ;

        //		public const uint BACKGROUNDIMAGECACHE =   2430 ;

        //		public const uint DEFAULTBLOCK =           6046 ;

        //		public const uint MIMECSET__FIRST__ =      3609 ;
        //		public const uint MIMECSET__LAST__ =       3699 ;

        //		public const uint MENUEXT_FIRST__ =        3700 ;
        //		public const uint MENUEXT_LAST__ =         3732 ;
        //		public const uint MENUEXT_COUNT =          3733 ;

        // Commands mapped from the standard set.  We should
        // consider deleting them from public header files.

        //		public const uint OPEN =                   2000 ;
        //		public const uint NEW =                    2001 ;
        public const uint SAVE = 70;
        public const uint SAVEAS = 71;
        //		public const uint SAVECOPYAS =             2002 ;
        public const uint PRINTPREVIEW = 2003;
        //		public const uint SHOWPRINT =              2010 ;
        //		public const uint SHOWPAGESETUP =          2011 ;
        public const uint PRINT = 27;
        public const uint PAGESETUP = 2004;
        //		public const uint SPELL =                  2005 ;
        //		public const uint PASTESPECIAL =           2006 ;
        public const uint CLEARSELECTION = 2007;
        //		public const uint PROPERTIES =             28 ;
        public const uint REDO = 29;
        public const uint UNDO = 43;
        public const uint SELECTALL = 31;
        //		public const uint ZOOMPERCENT =            50 ;
        //		public const uint GETZOOM =                68 ;
        //		public const uint STOP =                   2138 ;
        public const uint COPY = 15;
        public const uint CUT = 16;
        public const uint PASTE = 26;

        // IDMs for CGID_EditStateCommands group
        //		public const uint CONTEXT =                1 ;
        //		public const uint HWND =                   2 ;

        // Shdocvw Execs on CGID_DocHostCommandHandler
        //		public const uint NEW_TOPLEVELWINDOW =     7050 ;

        //
        // Undo persistence comands
        //
        //		public const uint PRESERVEUNDOALWAYS =     6049 ;
        //		public const uint PERSISTDEFAULTVALUES =   7100 ;

        /// <summary>
        /// Use a boolean set to true as input to protect meta tags being overwritten when parsing a string of HTML.
        /// </summary>
        public const uint PROTECTMETATAGS = 7101;

        public const uint GETFRAMEZONE = 6037;

        //;begin_internal
        // <New in IE6>
        //		public const uint FIRE_PRINTTEMPLATEUP =     15000 ;
        //		public const uint FIRE_PRINTTEMPLATEDOWN =   15001 ;
        //		public const uint SETPRINTHANDLES =          15002 ;
        public const uint CLEARAUTHENTICATIONCACHE = 15003;
        //;end_internal
    }
}
