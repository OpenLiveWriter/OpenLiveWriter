// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    public interface ISimpleTextEditorCommandSource
    {
        bool HasFocus { get; }

        bool CanUndo { get; }
        void Undo();

        bool CanRedo { get; }
        void Redo();

        bool CanCut { get; }
        void Cut();

        bool CanCopy { get; }
        void Copy();

        bool CanPaste { get; }
        void Paste();

        bool CanClear { get; }
        void Clear();

        void SelectAll();
        void InsertEuroSymbol();

        bool ReadOnly { get; }

        event EventHandler CommandStateChanged;
        event EventHandler AggressiveCommandStateChanged;
    }

    public class SimpleTextEditorCommandHelper
    {
        /// <summary>
        /// Call this method to ensure that the passed control
        /// gets to handle cut, copy, paste, undo, redo, and del
        /// natively instead of through the SimpleTextEditorCommand
        /// system.
        /// </summary>
        public static IDisposable UseNativeBehaviors(CommandManager commandManager, params Control[] controls)
        {
            return new NativeBehaviors(commandManager, controls);
        }

        public class NativeBehaviors : IDisposable
        {
            private readonly Control[] Controls;
            private readonly CommandManager CommandManager;

            public NativeBehaviors(CommandManager commandManager, params Control[] controls)
            {
                Controls = controls;
                CommandManager = commandManager;
                foreach (Control c in Controls)
                {
                    c.GotFocus += new EventHandler(c_GotFocus);
                    c.LostFocus += new EventHandler(c_LostFocus);
                }
            }

            /// <summary>
            /// Call this method to ensure that the passed control
            /// does NOT get to handle cut, copy, paste, undo, redo,
            /// and del natively, but gets passed through the
            /// SimpleTextEditorCommand system instead.
            /// </summary>
            public void Dispose()
            {
                foreach (Control c in Controls)
                {
                    c.GotFocus -= new EventHandler(c_GotFocus);
                    c.LostFocus -= new EventHandler(c_LostFocus);
                }
            }

            private void c_GotFocus(object sender, EventArgs e)
            {
                CommandManager.IgnoreShortcut(Shortcut.CtrlZ);
                CommandManager.IgnoreShortcut(Shortcut.CtrlY);
                CommandManager.IgnoreShortcut(Shortcut.CtrlX);
                CommandManager.IgnoreShortcut(Shortcut.CtrlC);
                CommandManager.IgnoreShortcut(Shortcut.CtrlV);
                CommandManager.IgnoreShortcut(Shortcut.Del);
            }

            private void c_LostFocus(object sender, EventArgs e)
            {
                CommandManager.UnignoreShortcut(Shortcut.CtrlZ);
                CommandManager.UnignoreShortcut(Shortcut.CtrlY);
                CommandManager.UnignoreShortcut(Shortcut.CtrlX);
                CommandManager.UnignoreShortcut(Shortcut.CtrlC);
                CommandManager.UnignoreShortcut(Shortcut.CtrlV);
                CommandManager.UnignoreShortcut(Shortcut.Del);
            }

        }

    }
}
