// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class PostTitleEditingElementBehavior : TitledRegionElementBehavior
    {
        private bool defaultedText = false;
        public PostTitleEditingElementBehavior(IHtmlEditorComponentContext editorContext, IHTMLElement prevEditableRegion, IHTMLElement nextEditableRegion)
            : base(editorContext, prevEditableRegion, nextEditableRegion)
        {
        }

        private string DefaultText
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TitleDefaultText), EditingTargetName);
            }
        }

        public bool DefaultedText
        {
            get
            {
                return defaultedText;
            }
        }

        internal string EditingTargetName
        {
            get { return _editingTargetName; }
            set { _editingTargetName = value; }
        }
        private string _editingTargetName = Res.Get(StringId.PostLower);

        protected override void OnElementAttached()
        {
            base.OnElementAttached();

            if (PostEditorSettings.AutomationMode)
            {
                //Test automation requires the element to be explicitly named in the accessibility tree, but
                //setting a title causes an annoying tooltip and focus rectanlge, so we only show it in automation mode
                HTMLElement2.tabIndex = 0;
                HTMLElement.title = " Post Title";
            }

            if (string.IsNullOrEmpty(HTMLElement.innerText))
            {
                //set the default text for this editable region
                using (IUndoUnit undo = EditorContext.CreateInvisibleUndoUnit())
                {
                    HTMLElement.innerText = DefaultText;
                    defaultedText = true;
                    RegionBorderVisible = !Selected && defaultedText;
                    undo.Commit();
                }
            }

            //fix bug 403230: set the title style to block element so that it takes up the entire line.
            (HTMLElement as IHTMLElement2).runtimeStyle.display = "block";
        }

        public override string GetEditedHtml(bool useXhtml, bool doCleanup)
        {
            if (defaultedText)
            {
                return String.Empty;
            }
            else if (HTMLElement.innerText == null)
            {
                return String.Empty;
            }
            else
            {
                return HTMLElement.innerText.Trim();
            }
        }

        public event EventHandler TitleChanged;
        protected virtual void OnTitleChanged(EventArgs evt)
        {
            if (TitleChanged != null)
            {
                TitleChanged(null, evt);
            }

            defaultedText = false;
        }

        /// <summary>
        /// Checks to see if the title has been edited.
        /// </summary>
        private void CheckForTitleEdits()
        {
            if (HTMLElement.innerText != DefaultText && defaultedText)
            {
                OnTitleChanged(EventArgs.Empty);
            }
        }

        public void CleanBeforeInsert()
        {
            using (IUndoUnit undo = EditorContext.CreateInvisibleUndoUnit())
            {
                //Fire event if title has changed.
                CheckForTitleEdits();

                // If the title contains the default text..
                if (defaultedText)
                {
                    // Clear it before the insert.
                    HTMLElement.innerText = null;
                    defaultedText = false;
                }

                undo.Commit();
            }
        }

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();

            //Show the region border if the text is defaulted or empty
            //If the region is selected, clear any defaulted text
            using (IUndoUnit undo = EditorContext.CreateInvisibleUndoUnit())
            {
                CheckForTitleEdits();
                if (Selected)
                {
                    if (defaultedText)
                    {
                        HTMLElement.innerText = null;
                        RegionBorderVisible = true;
                        defaultedText = false;
                    }
                    OnEditableRegionFocusChanged(null, new EditableRegionFocusChangedEventArgs(false));
                }
                else
                {
                    if (HTMLElement.innerText == null)
                    {
                        HTMLElement.innerText = DefaultText;
                        defaultedText = true;
                        RegionBorderVisible = true;
                    }
                    RegionBorderVisible = defaultedText;
                }
                undo.Commit();
            }
        }

        protected override void OnCommandKey(object sender, KeyEventArgs e)
        {
            base.OnCommandKey(sender, e);
            bool addDamage = false;
            if (e.KeyCode == Keys.Enter)
            {
                LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                SelectNextRegion();
                e.Handled = true;
                addDamage = true;
            }
            else if (e.KeyCode == Keys.Tab && !e.Shift)
            {
                LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                SelectNextRegion();
                //Cancel the event so that it doesn't trigger the blockquote command
                e.Handled = true;
                addDamage = true;
            }

            if (addDamage)
            {
                // WinLive 240926
                // If we are moving away from this title, add our text for spell checking to be sure any changes are spell checked.
                // The Enter/Tab keys are eaten up after this (e.Handled = true) and so WordRangeDamager.OnKeyDown will not get a
                // chance to see these keys to update the damaged area.
                EditorContext.DamageServices.AddDamage(EditorContext.MarkupServices.CreateMarkupRange(HTMLElement, false));
            }

            //If the command associated with the shortcut is disabled, eat the command key
            //This prevents commands like paste from getting through to the standard handler
            //when they are disabled.
            Command command = EditorContext.CommandManager.FindCommandWithShortcut(e.KeyData);
            if (command != null && !command.Enabled)
            {
                LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(object o, HtmlEventArgs e)
        {
            base.OnKeyDown(o, e);

            if (HtmlEditorSettings.AggressivelyInvalidate)
                Invalidate();
        }

        protected override void OnKeyUp(object o, HtmlEventArgs e)
        {
            base.OnKeyUp(o, e);

            //fire a title changed event
            this.OnTitleChanged(EventArgs.Empty);
        }
    }

}
