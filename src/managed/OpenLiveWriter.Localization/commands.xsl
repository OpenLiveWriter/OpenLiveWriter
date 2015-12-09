<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output version="1.0" method="xml" indent="yes"/>

  <!-- The identity transform -->
  <xsl:template match="/ | @* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- NOTE: In order to prevent pseudo-localization you need to add {Locked=1025,1041} to a string's comment. -->
  <xsl:template match="//Command">
    <xsl:copy>
      <xsl:if test="@DebugOnly and @MenuText">
        <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="MenuText">{Locked} Do not translate.  This is a debug only command.</xsl:attribute>
      </xsl:if>
      <xsl:if test="@DebugOnly and @Text">
        <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="Text">{Locked} Do not translate.  This is a debug only command.</xsl:attribute>
      </xsl:if>
      
      
      <xsl:if test="@Shortcut">
        <xsl:choose>
          <xsl:when test="@DebugOnly">
            <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="Shortcut">{Locked=1025,1041} Do not translate.  This is a debug only command.</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="Shortcut">{Locked=1025,1041}{ValidStrings="None", "Ins", "Del", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "ShiftIns", "ShiftDel", "ShiftF1", "ShiftF2", "ShiftF3", "ShiftF4", "ShiftF5", "ShiftF6", "ShiftF7", "ShiftF8", "ShiftF9", "ShiftF10", "ShiftF11", "ShiftF12", "CtrlIns", "CtrlDel", "Ctrl0", "Ctrl1", "Ctrl2", "Ctrl3", "Ctrl4", "Ctrl5", "Ctrl6", "Ctrl7", "Ctrl8", "Ctrl9", "CtrlA", "CtrlB", "CtrlC", "CtrlD", "CtrlE", "CtrlF", "CtrlG", "CtrlH", "CtrlI", "CtrlJ", "CtrlK", "CtrlL", "CtrlM", "CtrlN", "CtrlO", "CtrlP", "CtrlQ", "CtrlR", "CtrlS", "CtrlT", "CtrlU", "CtrlV", "CtrlW", "CtrlX", "CtrlY", "CtrlZ", "CtrlF1", "CtrlF2", "CtrlF3", "CtrlF4", "CtrlF5", "CtrlF6", "CtrlF7", "CtrlF8", "CtrlF9", "CtrlF10", "CtrlF11", "CtrlF12", "CtrlShift0", "CtrlShift1", "CtrlShift2", "CtrlShift3", "CtrlShift4", "CtrlShift5", "CtrlShift6", "CtrlShift7", "CtrlShift8", "CtrlShift9", "CtrlShiftA", "CtrlShiftB", "CtrlShiftC", "CtrlShiftD", "CtrlShiftE", "CtrlShiftF", "CtrlShiftG", "CtrlShiftH", "CtrlShiftI", "CtrlShiftJ", "CtrlShiftK", "CtrlShiftL", "CtrlShiftM", "CtrlShiftN", "CtrlShiftO", "CtrlShiftP", "CtrlShiftQ", "CtrlShiftR", "CtrlShiftS", "CtrlShiftT", "CtrlShiftU", "CtrlShiftV", "CtrlShiftW", "CtrlShiftX", "CtrlShiftY", "CtrlShiftZ", "CtrlShiftF1", "CtrlShiftF2", "CtrlShiftF3", "CtrlShiftF4", "CtrlShiftF5", "CtrlShiftF6", "CtrlShiftF7", "CtrlShiftF8", "CtrlShiftF9", "CtrlShiftF10", "CtrlShiftF11", "CtrlShiftF12", "AltBksp", "AltLeftArrow", "AltUpArrow", "AltRightArrow", "AltDownArrow", "Alt0", "Alt1", "Alt2", "Alt3", "Alt4", "Alt5", "Alt6", "Alt7", "Alt8", "Alt9", "AltF1", "AltF2", "AltF3", "AltF4", "AltF5", "AltF6", "AltF7", "AltF8", "AltF9", "AltF10", "AltF11", "AltF12"}</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>  
      </xsl:if>

      <!-- AdvancedShortcuts are currently marked as non-localizable, but there isn't any reason why they should not be.
           However, given the very short loc cycle here at the end of Wave 4 we'll not start localizing them now.
           Consider doing so in the future.
      -->
      <!--<xsl:if test="@AdvancedShortcut">
        <xsl:choose>
          <xsl:when test="@DebugOnly">
            <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="AdvancedShortcut">{Locked=1025,1041} Do not translate.  This is for testing only.</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>            
            <xsl:attribute namespace="http://OpenLiveWriter.spaces.live.com/#comment" name="AdvancedShortcut">{Locked=1025,1041}{RegEx="'^(None|LButton|RButton|Cancel|MButton|XButton1|XButton2|Back|Tab|LineFeed|Clear|Return|Return|ShiftKey|ControlKey|Menu|Pause|Capital|Capital|KanaMode|KanaMode|KanaMode|JunjaMode|FinalMode|HanjaMode|HanjaMode|Escape|IMEConvert|IMENonconvert|IMEAceept|IMEAceept|IMEModeChange|Space|PageUp|PageUp|Next|Next|End|Home|Left|Up|Right|Down|Select|Print|Execute|PrintScreen|PrintScreen|Insert|Delete|Help|D0|D1|D2|D3|D4|D5|D6|D7|D8|D9|A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|Q|R|S|T|U|V|W|X|Y|Z|LWin|RWin|Apps|Sleep|NumPad0|NumPad1|NumPad2|NumPad3|NumPad4|NumPad5|NumPad6|NumPad7|NumPad8|NumPad9|Multiply|Add|Separator|Subtract|Decimal|Divide|F1|F2|F3|F4|F5|F6|F7|F8|F9|F10|F11|F12|F13|F14|F15|F16|F17|F18|F19|F20|F21|F22|F23|F24|NumLock|Scroll|LShiftKey|RShiftKey|LControlKey|RControlKey|LMenu|RMenu|BrowserBack|BrowserForward|BrowserRefresh|BrowserStop|BrowserSearch|BrowserFavorites|BrowserHome|VolumeMute|VolumeDown|VolumeUp|MediaNextTrack|MediaPreviousTrack|MediaStop|MediaPlayPause|LaunchMail|SelectMedia|LaunchApplication1|LaunchApplication2|Oem1|Oem1|Oemplus|Oemcomma|OemMinus|OemPeriod|OemQuestion|OemQuestion|Oemtilde|Oemtilde|OemOpenBrackets|OemOpenBrackets|Oem5|Oem5|Oem6|Oem6|Oem7|Oem7|Oem8|OemBackslash|OemBackslash|ProcessKey|Packet|Attn|Crsel|Exsel|EraseEof|Play|Zoom|NoName|Pa1|OemClear|KeyCode|Shift|Control|Alt|Modifiers)+(,(None|LButton|RButton|Cancel|MButton|XButton1|XButton2|Back|Tab|LineFeed|Clear|Return|Return|ShiftKey|ControlKey|Menu|Pause|Capital|Capital|KanaMode|KanaMode|KanaMode|JunjaMode|FinalMode|HanjaMode|HanjaMode|Escape|IMEConvert|IMENonconvert|IMEAceept|IMEAceept|IMEModeChange|Space|PageUp|PageUp|Next|Next|End|Home|Left|Up|Right|Down|Select|Print|Execute|PrintScreen|PrintScreen|Insert|Delete|Help|D0|D1|D2|D3|D4|D5|D6|D7|D8|D9|A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|Q|R|S|T|U|V|W|X|Y|Z|LWin|RWin|Apps|Sleep|NumPad0|NumPad1|NumPad2|NumPad3|NumPad4|NumPad5|NumPad6|NumPad7|NumPad8|NumPad9|Multiply|Add|Separator|Subtract|Decimal|Divide|F1|F2|F3|F4|F5|F6|F7|F8|F9|F10|F11|F12|F13|F14|F15|F16|F17|F18|F19|F20|F21|F22|F23|F24|NumLock|Scroll|LShiftKey|RShiftKey|LControlKey|RControlKey|LMenu|RMenu|BrowserBack|BrowserForward|BrowserRefresh|BrowserStop|BrowserSearch|BrowserFavorites|BrowserHome|VolumeMute|VolumeDown|VolumeUp|MediaNextTrack|MediaPreviousTrack|MediaStop|MediaPlayPause|LaunchMail|SelectMedia|LaunchApplication1|LaunchApplication2|Oem1|Oem1|Oemplus|Oemcomma|OemMinus|OemPeriod|OemQuestion|OemQuestion|Oemtilde|Oemtilde|OemOpenBrackets|OemOpenBrackets|Oem5|Oem5|Oem6|Oem6|Oem7|Oem7|Oem8|OemBackslash|OemBackslash|ProcessKey|Packet|Attn|Crsel|Exsel|EraseEof|Play|Zoom|NoName|Pa1|OemClear|KeyCode|Shift|Control|Alt|Modifiers))*$'"}</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>-->
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>    
</xsl:stylesheet>