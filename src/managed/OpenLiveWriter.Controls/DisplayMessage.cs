// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// DisplayMessage component.
    /// </summary>
    public sealed class DisplayMessage : Component
    {
        /// <summary>
        /// The display message type.
        /// </summary>
        private MessageBoxIcon type = MessageBoxIcon.Exclamation;

        private static ResourcedPropertyLoader propLoader = new ResourcedPropertyLoader(
            typeof(DisplayMessage),
            new ResourceManager("OpenLiveWriter.Localization.Properties", typeof(MessageId).Assembly),
            new ResourceManager("OpenLiveWriter.Localization.PropertiesNonLoc", typeof(MessageId).Assembly)
            );

        /// <summary>
        /// Gets or sets the display message type.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(MessageBoxIcon.Exclamation),
                Description("Specifies the display message type.")
        ]
        public MessageBoxIcon Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        /// <summary>
        /// The message box buttons to display for the message.
        /// </summary>
        private MessageBoxButtons buttons = MessageBoxButtons.OK;

        /// <summary>
        /// Gets or sets the message box buttons to display for the message.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(MessageBoxButtons.OK),
                Description("Specifies the message box buttons to display for the message.")
        ]
        public MessageBoxButtons Buttons
        {
            get
            {
                return buttons;
            }
            set
            {
                buttons = value;
            }
        }

        /// <summary>
        /// The default message box button to display for the message.
        /// </summary>
        private MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1;

        /// <summary>
        /// Gets or sets the default message box button to display for the message.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(MessageBoxDefaultButton.Button1),
                Description("Specifies the default message box button to display for the message.")
        ]
        public MessageBoxDefaultButton DefaultButton
        {
            get
            {
                return defaultButton;
            }
            set
            {
                defaultButton = value;
            }
        }

        /// <summary>
        /// The title of the message.
        /// </summary>
        private string title;

        /// <summary>
        /// Gets or sets the title to display for the message.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the the title to display for the message.  Note that this property is optional.")
        ]
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }

        /// <summary>
        /// The text of the message.
        /// </summary>
        private string text;

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the the text to display for the message.")
        ]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public DisplayMessage(MessageId messageId)
        {
            InitializeComponent();
            if (messageId != MessageId.None)
                propLoader.ApplyResources(this, messageId.ToString());
        }

        /// <summary>
        public DisplayMessage(String text, String title)
        {
            InitializeComponent();

            if (!String.IsNullOrEmpty(text))
                Text = text;

            if (!String.IsNullOrEmpty(title))
                Title = title;
        }

        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        /// <summary>
        /// Shows the DisplayMessage of the specified type.
        /// </summary>
        /// <param name="type">The DisplayMessage type.</param>
        /// <param name="args">Optional format arguments.</param>
        /// <returns>The DialogResult.</returns>
        public static DialogResult Show(MessageId messageId, params object[] args)
        {
            return Show(messageId, Win32WindowImpl.ForegroundWin32Window, args);
        }

        /// <summary>
        /// Shows the DisplayMessage of the specified type.
        /// </summary>
        /// <param name="type">The DisplayMessage type.</param>
        /// <param name="args">Optional format arguments.</param>
        /// <returns>The DialogResult.</returns>
        public static DialogResult Show(MessageId messageId, IWin32Window owner, params object[] args)
        {
            if (messageId == MessageId.None)
            {
                Trace.Fail("MessageId.None passed to DisplayMessage.Show");
                return DialogResult.None;
            }

            DisplayMessage message = new DisplayMessage(messageId);

            return message.Show(owner, args);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="args">Optional format arguments.</param>
        /// <returns>The DialogResult.</returns>
        public DialogResult Show(params object[] args)
        {
            return Show(Win32WindowImpl.ForegroundWin32Window, args);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="args">Optional format arguments.</param>
        /// <returns>The DialogResult.</returns>
        public DialogResult Show(IWin32Window owner, params object[] args)
        {
            //	Set the MessageBoxIcon to display.
            MessageBoxIcon messageBoxIcon = Type;

            //	Format the caption.
            string caption;
            if (Title != null && Title.Length != 0)
                caption = Title;
            else
                caption = ApplicationEnvironment.ProductNameQualified;

            //	Format the display text.
            string displayText;
            if (args != null && args.Length != 0)
                displayText = String.Format(CultureInfo.CurrentCulture, text, args);
            else
                displayText = text;

            //	Show the message,
            return MessageBox.Show(owner == null ? Win32WindowImpl.ForegroundWin32Window : owner,
                                    displayText,
                                    caption,
                                    Buttons,
                                    messageBoxIcon,
                                    DefaultButton,
                                    BidiHelper.RTLMBOptions);

        }
    }
}
