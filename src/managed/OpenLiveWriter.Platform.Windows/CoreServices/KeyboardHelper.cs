// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Specifies an "accelerator mnemonic" (which is a custom accelerator).
    /// </summary>
    public enum AcceleratorMnemonic
    {
        None = Keys.None,
        AltA = Keys.Alt | Keys.A,
        AltB = Keys.Alt | Keys.B,
        AltC = Keys.Alt | Keys.C,
        AltD = Keys.Alt | Keys.D,
        AltE = Keys.Alt | Keys.E,
        AltF = Keys.Alt | Keys.F,
        AltG = Keys.Alt | Keys.G,
        AltH = Keys.Alt | Keys.H,
        AltI = Keys.Alt | Keys.I,
        AltJ = Keys.Alt | Keys.J,
        AltK = Keys.Alt | Keys.K,
        AltL = Keys.Alt | Keys.L,
        AltM = Keys.Alt | Keys.M,
        AltN = Keys.Alt | Keys.N,
        AltO = Keys.Alt | Keys.O,
        AltP = Keys.Alt | Keys.P,
        AltQ = Keys.Alt | Keys.Q,
        AltR = Keys.Alt | Keys.R,
        AltS = Keys.Alt | Keys.S,
        AltT = Keys.Alt | Keys.T,
        AltU = Keys.Alt | Keys.U,
        AltV = Keys.Alt | Keys.V,
        AltW = Keys.Alt | Keys.W,
        AltX = Keys.Alt | Keys.X,
        AltY = Keys.Alt | Keys.Y,
        AltZ = Keys.Alt | Keys.Z,
        CtrlOEMPLUS = Keys.Control | Keys.Oemplus,
        CtrlShiftOEMPLUS = Keys.Control | Keys.Shift | Keys.Oemplus
    }

    /// <summary>
    /// KeyboardHelper helper class.
    /// </summary>
    public class KeyboardHelper
    {
        /// <summary>
        /// Translation table to map a Keys value to a Shortcut value.
        /// </summary>
        private static Hashtable keysToShortcutTable;

        /// <summary>
        /// Translation table to map a Keys value to a AcceleratorMnemonic value.
        /// </summary>
        private static Hashtable keysToAcceleratorMnemonicTable;

        /// <summary>
        /// Static initialization of the KeyboardHelper class.
        /// </summary>
        static KeyboardHelper()
        {
            //	Instantiate the keys to Shortcut table.
            keysToShortcutTable = new Hashtable();

            //	Instantiate the keys to AcceleratorMnemonic table.
            keysToAcceleratorMnemonicTable = new Hashtable();

            //	Sadly, this is best solution...
            keysToShortcutTable[Keys.Alt & Keys.Back] = Shortcut.AltBksp;

            keysToShortcutTable[Keys.Insert] = Shortcut.Ins;
            keysToShortcutTable[Keys.Shift | Keys.Insert] = Shortcut.ShiftIns;
            keysToShortcutTable[Keys.Control | Keys.Insert] = Shortcut.CtrlIns;

            keysToShortcutTable[Keys.Delete] = Shortcut.Del;
            keysToShortcutTable[Keys.Shift | Keys.Delete] = Shortcut.ShiftDel;
            keysToShortcutTable[Keys.Control | Keys.Delete] = Shortcut.CtrlDel;

            keysToShortcutTable[Keys.Alt | Keys.D0] = Shortcut.Alt0;
            keysToShortcutTable[Keys.Alt | Keys.D1] = Shortcut.Alt1;
            keysToShortcutTable[Keys.Alt | Keys.D2] = Shortcut.Alt2;
            keysToShortcutTable[Keys.Alt | Keys.D3] = Shortcut.Alt3;
            keysToShortcutTable[Keys.Alt | Keys.D4] = Shortcut.Alt4;
            keysToShortcutTable[Keys.Alt | Keys.D5] = Shortcut.Alt5;
            keysToShortcutTable[Keys.Alt | Keys.D6] = Shortcut.Alt6;
            keysToShortcutTable[Keys.Alt | Keys.D7] = Shortcut.Alt7;
            keysToShortcutTable[Keys.Alt | Keys.D8] = Shortcut.Alt8;
            keysToShortcutTable[Keys.Alt | Keys.D9] = Shortcut.Alt9;

            keysToShortcutTable[Keys.Control | Keys.D0] = Shortcut.Ctrl0;
            keysToShortcutTable[Keys.Control | Keys.D1] = Shortcut.Ctrl1;
            keysToShortcutTable[Keys.Control | Keys.D2] = Shortcut.Ctrl2;
            keysToShortcutTable[Keys.Control | Keys.D3] = Shortcut.Ctrl3;
            keysToShortcutTable[Keys.Control | Keys.D4] = Shortcut.Ctrl4;
            keysToShortcutTable[Keys.Control | Keys.D5] = Shortcut.Ctrl5;
            keysToShortcutTable[Keys.Control | Keys.D6] = Shortcut.Ctrl6;
            keysToShortcutTable[Keys.Control | Keys.D7] = Shortcut.Ctrl7;
            keysToShortcutTable[Keys.Control | Keys.D8] = Shortcut.Ctrl8;
            keysToShortcutTable[Keys.Control | Keys.D9] = Shortcut.Ctrl9;

            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D0] = Shortcut.CtrlShift0;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D1] = Shortcut.CtrlShift1;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D2] = Shortcut.CtrlShift2;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D3] = Shortcut.CtrlShift3;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D4] = Shortcut.CtrlShift4;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D5] = Shortcut.CtrlShift5;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D6] = Shortcut.CtrlShift6;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D7] = Shortcut.CtrlShift7;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D8] = Shortcut.CtrlShift8;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D9] = Shortcut.CtrlShift9;

            keysToShortcutTable[Keys.F1] = Shortcut.F1;
            keysToShortcutTable[Keys.F2] = Shortcut.F2;
            keysToShortcutTable[Keys.F3] = Shortcut.F3;
            keysToShortcutTable[Keys.F4] = Shortcut.F4;
            keysToShortcutTable[Keys.F5] = Shortcut.F5;
            keysToShortcutTable[Keys.F6] = Shortcut.F6;
            keysToShortcutTable[Keys.F7] = Shortcut.F7;
            keysToShortcutTable[Keys.F8] = Shortcut.F8;
            keysToShortcutTable[Keys.F9] = Shortcut.F9;
            keysToShortcutTable[Keys.F10] = Shortcut.F10;
            keysToShortcutTable[Keys.F11] = Shortcut.F11;
            keysToShortcutTable[Keys.F12] = Shortcut.F12;

            keysToShortcutTable[Keys.Alt | Keys.F1] = Shortcut.AltF1;
            keysToShortcutTable[Keys.Alt | Keys.F2] = Shortcut.AltF2;
            keysToShortcutTable[Keys.Alt | Keys.F3] = Shortcut.AltF3;
            keysToShortcutTable[Keys.Alt | Keys.F4] = Shortcut.AltF4;
            keysToShortcutTable[Keys.Alt | Keys.F5] = Shortcut.AltF5;
            keysToShortcutTable[Keys.Alt | Keys.F6] = Shortcut.AltF6;
            keysToShortcutTable[Keys.Alt | Keys.F7] = Shortcut.AltF7;
            keysToShortcutTable[Keys.Alt | Keys.F8] = Shortcut.AltF8;
            keysToShortcutTable[Keys.Alt | Keys.F9] = Shortcut.AltF9;
            keysToShortcutTable[Keys.Alt | Keys.F10] = Shortcut.AltF10;
            keysToShortcutTable[Keys.Alt | Keys.F11] = Shortcut.AltF11;
            keysToShortcutTable[Keys.Alt | Keys.F12] = Shortcut.AltF12;

            keysToShortcutTable[Keys.Shift | Keys.F1] = Shortcut.ShiftF1;
            keysToShortcutTable[Keys.Shift | Keys.F2] = Shortcut.ShiftF2;
            keysToShortcutTable[Keys.Shift | Keys.F3] = Shortcut.ShiftF3;
            keysToShortcutTable[Keys.Shift | Keys.F4] = Shortcut.ShiftF4;
            keysToShortcutTable[Keys.Shift | Keys.F5] = Shortcut.ShiftF5;
            keysToShortcutTable[Keys.Shift | Keys.F6] = Shortcut.ShiftF6;
            keysToShortcutTable[Keys.Shift | Keys.F7] = Shortcut.ShiftF7;
            keysToShortcutTable[Keys.Shift | Keys.F8] = Shortcut.ShiftF8;
            keysToShortcutTable[Keys.Shift | Keys.F9] = Shortcut.ShiftF9;
            keysToShortcutTable[Keys.Shift | Keys.F10] = Shortcut.ShiftF10;
            keysToShortcutTable[Keys.Shift | Keys.F11] = Shortcut.ShiftF11;
            keysToShortcutTable[Keys.Shift | Keys.F12] = Shortcut.ShiftF12;

            keysToShortcutTable[Keys.Control | Keys.F1] = Shortcut.CtrlF1;
            keysToShortcutTable[Keys.Control | Keys.F2] = Shortcut.CtrlF2;
            keysToShortcutTable[Keys.Control | Keys.F3] = Shortcut.CtrlF3;
            keysToShortcutTable[Keys.Control | Keys.F4] = Shortcut.CtrlF4;
            keysToShortcutTable[Keys.Control | Keys.F5] = Shortcut.CtrlF5;
            keysToShortcutTable[Keys.Control | Keys.F6] = Shortcut.CtrlF6;
            keysToShortcutTable[Keys.Control | Keys.F7] = Shortcut.CtrlF7;
            keysToShortcutTable[Keys.Control | Keys.F8] = Shortcut.CtrlF8;
            keysToShortcutTable[Keys.Control | Keys.F9] = Shortcut.CtrlF9;
            keysToShortcutTable[Keys.Control | Keys.F10] = Shortcut.CtrlF10;
            keysToShortcutTable[Keys.Control | Keys.F11] = Shortcut.CtrlF11;
            keysToShortcutTable[Keys.Control | Keys.F12] = Shortcut.CtrlF12;

            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F1] = Shortcut.CtrlShiftF1;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F2] = Shortcut.CtrlShiftF2;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F3] = Shortcut.CtrlShiftF3;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F4] = Shortcut.CtrlShiftF4;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F5] = Shortcut.CtrlShiftF5;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F6] = Shortcut.CtrlShiftF6;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F7] = Shortcut.CtrlShiftF7;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F8] = Shortcut.CtrlShiftF8;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F9] = Shortcut.CtrlShiftF9;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F10] = Shortcut.CtrlShiftF10;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F11] = Shortcut.CtrlShiftF11;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F12] = Shortcut.CtrlShiftF12;

            keysToShortcutTable[Keys.Control | Keys.A] = Shortcut.CtrlA;
            keysToShortcutTable[Keys.Control | Keys.B] = Shortcut.CtrlB;
            keysToShortcutTable[Keys.Control | Keys.C] = Shortcut.CtrlC;
            keysToShortcutTable[Keys.Control | Keys.D] = Shortcut.CtrlD;
            keysToShortcutTable[Keys.Control | Keys.E] = Shortcut.CtrlE;
            keysToShortcutTable[Keys.Control | Keys.F] = Shortcut.CtrlF;
            keysToShortcutTable[Keys.Control | Keys.G] = Shortcut.CtrlG;
            keysToShortcutTable[Keys.Control | Keys.H] = Shortcut.CtrlH;
            keysToShortcutTable[Keys.Control | Keys.I] = Shortcut.CtrlI;
            keysToShortcutTable[Keys.Control | Keys.J] = Shortcut.CtrlJ;
            keysToShortcutTable[Keys.Control | Keys.K] = Shortcut.CtrlK;
            keysToShortcutTable[Keys.Control | Keys.L] = Shortcut.CtrlL;
            keysToShortcutTable[Keys.Control | Keys.M] = Shortcut.CtrlM;
            keysToShortcutTable[Keys.Control | Keys.N] = Shortcut.CtrlN;
            keysToShortcutTable[Keys.Control | Keys.O] = Shortcut.CtrlO;
            keysToShortcutTable[Keys.Control | Keys.P] = Shortcut.CtrlP;
            keysToShortcutTable[Keys.Control | Keys.Q] = Shortcut.CtrlQ;
            keysToShortcutTable[Keys.Control | Keys.R] = Shortcut.CtrlR;
            keysToShortcutTable[Keys.Control | Keys.S] = Shortcut.CtrlS;
            keysToShortcutTable[Keys.Control | Keys.T] = Shortcut.CtrlT;
            keysToShortcutTable[Keys.Control | Keys.U] = Shortcut.CtrlU;
            keysToShortcutTable[Keys.Control | Keys.V] = Shortcut.CtrlV;
            keysToShortcutTable[Keys.Control | Keys.W] = Shortcut.CtrlW;
            keysToShortcutTable[Keys.Control | Keys.X] = Shortcut.CtrlX;
            keysToShortcutTable[Keys.Control | Keys.Y] = Shortcut.CtrlY;
            keysToShortcutTable[Keys.Control | Keys.Z] = Shortcut.CtrlZ;

            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.A] = Shortcut.CtrlShiftA;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.B] = Shortcut.CtrlShiftB;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.C] = Shortcut.CtrlShiftC;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.D] = Shortcut.CtrlShiftD;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.E] = Shortcut.CtrlShiftE;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.F] = Shortcut.CtrlShiftF;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.G] = Shortcut.CtrlShiftG;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.H] = Shortcut.CtrlShiftH;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.I] = Shortcut.CtrlShiftI;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.J] = Shortcut.CtrlShiftJ;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.K] = Shortcut.CtrlShiftK;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.L] = Shortcut.CtrlShiftL;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.M] = Shortcut.CtrlShiftM;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.N] = Shortcut.CtrlShiftN;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.O] = Shortcut.CtrlShiftO;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.P] = Shortcut.CtrlShiftP;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.Q] = Shortcut.CtrlShiftQ;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.R] = Shortcut.CtrlShiftR;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.S] = Shortcut.CtrlShiftS;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.T] = Shortcut.CtrlShiftT;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.U] = Shortcut.CtrlShiftU;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.V] = Shortcut.CtrlShiftV;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.W] = Shortcut.CtrlShiftW;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.X] = Shortcut.CtrlShiftX;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.Y] = Shortcut.CtrlShiftY;
            keysToShortcutTable[Keys.Control | Keys.Shift | Keys.Z] = Shortcut.CtrlShiftZ;

            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.A] = AcceleratorMnemonic.AltA;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.B] = AcceleratorMnemonic.AltB;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.C] = AcceleratorMnemonic.AltC;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.D] = AcceleratorMnemonic.AltD;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.E] = AcceleratorMnemonic.AltE;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.F] = AcceleratorMnemonic.AltF;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.G] = AcceleratorMnemonic.AltG;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.H] = AcceleratorMnemonic.AltH;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.I] = AcceleratorMnemonic.AltI;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.J] = AcceleratorMnemonic.AltJ;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.K] = AcceleratorMnemonic.AltK;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.L] = AcceleratorMnemonic.AltL;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.M] = AcceleratorMnemonic.AltM;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.N] = AcceleratorMnemonic.AltN;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.O] = AcceleratorMnemonic.AltO;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.P] = AcceleratorMnemonic.AltP;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.Q] = AcceleratorMnemonic.AltQ;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.R] = AcceleratorMnemonic.AltR;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.S] = AcceleratorMnemonic.AltS;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.T] = AcceleratorMnemonic.AltT;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.U] = AcceleratorMnemonic.AltU;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.V] = AcceleratorMnemonic.AltV;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.W] = AcceleratorMnemonic.AltW;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.X] = AcceleratorMnemonic.AltX;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.Y] = AcceleratorMnemonic.AltY;
            keysToAcceleratorMnemonicTable[Keys.Alt | Keys.Z] = AcceleratorMnemonic.AltZ;
            keysToAcceleratorMnemonicTable[Keys.Control | Keys.Oemplus] = AcceleratorMnemonic.CtrlOEMPLUS;
            keysToAcceleratorMnemonicTable[Keys.Control | Keys.Shift | Keys.Oemplus] = AcceleratorMnemonic.CtrlShiftOEMPLUS;
        }

        /// <summary>
        /// Initializes a new instance of the KeyboardHelper class.
        /// </summary>
        private KeyboardHelper()
        {
        }

        /// <summary>
        /// List of all known shortcuts
        /// </summary>
        public static ICollection Shortcuts
        {
            get
            {
                return keysToShortcutTable.Values;
            }
        }

        private static Regex regexCtrlShiftAlt = new Regex("(Ctrl|Shift|Alt)");

        /// <summary>
        /// Format a shortcut as a string.
        /// </summary>
        /// <param name="shortcut">Shortcut to format.</param>
        /// <returns>String format of shortcut.</returns>
        public static string FormatShortcutString(Shortcut shortcut)
        {
            if (shortcut == Shortcut.None)
                return null;
            else
            {
                return regexCtrlShiftAlt.Replace(shortcut.ToString(), new MatchEvaluator(ReplaceShortcutPart));
            }
        }

        private static string ReplaceShortcutPart(Match match)
        {
            switch (match.Value)
            {
                case "Ctrl":
                    return Res.Get(StringId.ShortcutCtrl) + Res.Get(StringId.ShortcutPlus);
                case "Shift":
                    return Res.Get(StringId.ShortcutShift) + Res.Get(StringId.ShortcutPlus);
                case "Alt":
                    return Res.Get(StringId.ShortcutAlt) + Res.Get(StringId.ShortcutPlus);
                default:
                    Debug.Fail("Unexpected match: " + match.Value);
                    return match.Value;
            }
        }

        /// <summary>
        /// Map the specified Shortcut value to a Keys value (note: this operation is a reverse
        /// lookup in a hashtable so may be slightly expensive)
        /// </summary>
        /// <param name="shortcut">Shortcut value to map</param>
        /// <returns>Keys that the Shortcut maps to</returns>
        public static Keys MapToKeys(Shortcut shortcut)
        {
            // search for the shortcut value
            IDictionaryEnumerator tableEnumerator = keysToShortcutTable.GetEnumerator();
            while (tableEnumerator.MoveNext())
            {
                if ((Shortcut)tableEnumerator.Value == shortcut)
                    return (Keys)tableEnumerator.Key;
            }

            // if we didn't find any value then return 'empty' Keys
            return Keys.None;
        }

        /// <summary>
        /// Maps the specified Keys value to a Shortcut.
        /// </summary>
        /// <param name="keyData">Keys value to map.</param>
        /// <returns>Shortcut that the Keys value maps to, or Shortcut.None if the Keys value is
        /// not a valid Shortcut.</returns>
        public static Shortcut MapToShortcut(Keys keyData)
        {
            Object value = keysToShortcutTable[keyData];
            if (value == null)
                return Shortcut.None;
            else
                return (Shortcut)value;
        }

        /// <summary>
        /// Maps the specified Keys value to a AcceleratorMnemonic.
        /// </summary>
        /// <param name="keyData">Keys value to map.</param>
        /// <returns>AcceleratorMnemonic that the Keys value maps to, or AcceleratorMnemonic.None if the Keys value is
        /// not a valid AcceleratorMnemonic.</returns>
        public static AcceleratorMnemonic MapToAcceleratorMnemonic(Keys keyData)
        {
            Object value = keysToAcceleratorMnemonicTable[keyData];
            if (value == null)
                return AcceleratorMnemonic.None;
            else
                return (AcceleratorMnemonic)value;
        }

        /// <summary>
        /// Get the current state of the modifier keys (shift, control, alt)
        /// </summary>
        /// <returns>Keys with flags set for currently pressed modifiers</returns>
        public static Keys GetModifierKeys()
        {
            // default to no keys
            Keys modifiers = 0;

            // check each key state
            if (User32.GetKeyState(VK.SHIFT) < 0)
                modifiers |= Keys.Shift;
            if (User32.GetKeyState(VK.CONTROL) < 0)
                modifiers |= Keys.Control;
            if (User32.GetKeyState(VK.MENU) < 0)
                modifiers |= Keys.Alt;

            // return the state
            return modifiers;
        }

        public static bool IsCtrlRightAlt(Keys keys)
        {
            return (((keys & Keys.Control) != 0) &&
                    ((keys & Keys.Alt) != 0) &&
                    (User32.GetKeyState(VK.RMENU) < 0));
        }
    }
}
