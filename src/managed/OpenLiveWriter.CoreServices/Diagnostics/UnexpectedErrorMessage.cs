// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// DisplayMessage component.
    /// </summary>
    public class UnexpectedErrorMessage : Component
    {

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

        /// <summary>
        /// Initializes a new instance of the message class.
        /// </summary>
        /// <param name="container"></param>
        public UnexpectedErrorMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the message class.
        /// </summary>
        public UnexpectedErrorMessage()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();
        }

        /// <summary>
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
        /// Show with just an exception
        /// </summary>
        /// <param name="rootCause">root cause</param>
        public static void Show(Exception rootCause)
        {
            Show(null, rootCause);
        }

        /// <summary>
        /// Show with owner and exception
        /// </summary>
        /// <param name="owner">owner</param>
        /// <param name="rootCause">root cause</param>
        public static void Show(IWin32Window owner, Exception rootCause)
        {
            Show(typeof(UnhandledExceptionErrorMessage), owner, rootCause);
        }

        /// <summary>
        /// Static version of Show that allows you to specify the Type of the
        /// UnexpectedErrorMessage and have the method automatically create
        /// and dispose the instance
        /// </summary>
        /// <param name="errorType">type of error (must be derived from UnexpectedErrorMessage)</param>
        /// <param name="owner">window owner for showing message</param>
        /// <param name="rootCause">root cause (can be null)</param>
        /// <param name="args">format arguments (optional)</param>
        public static void Show(Type errorType, IWin32Window owner, Exception rootCause, params object[] args)
        {
            // verify calling semantics
            if (errorType.IsSubclassOf(typeof(UnexpectedErrorMessage)))
            {
                // create instance of error type
                using (UnexpectedErrorMessage errorMessage =
                            Activator.CreateInstance(errorType) as UnexpectedErrorMessage)
                {
                    errorMessage.ShowMessage(owner, rootCause, args);
                }
            }
            else
            {
                Debug.Fail("Type passed to ShowErrorMessage (" + errorType.Name + ") is not a subclass of UnexpectedErrorMessage");
            }
        }

        public static void Show(IWin32Window owner, Exception rootCause, string title)
        {
            // create instance of error type
            using (UnexpectedErrorMessage errorMessage = new UnexpectedErrorMessage())
            {
                errorMessage.Title = title;
                errorMessage.ShowMessage(owner, rootCause, new string[0]);
            }
        }

        /// <summary>
        /// Shows the message -- includes root cause information that can optionally
        /// be made available for logging and/or for developers or motivated/curious
        /// end users.
        /// </summary>
        public virtual void ShowMessage(IWin32Window owner, Exception rootCause, params object[] args)
        {
            string dialogTitle = Title;
            string dialogMessage = FormatText(args);
            bool isUnexpected = true;

            //check for special error message type
            ExceptionMessage dynamicMessage;
            if (DynamicExceptionMessageRegistry.Instance.GetMessage(out dynamicMessage, rootCause))
            {
                if (dynamicMessage == null)
                    return;
                else
                {
                    dialogTitle = dynamicMessage.GetTitle(Title);
                    dialogMessage = dynamicMessage.GetMessage(args);
                    isUnexpected = dynamicMessage.Unexpected;
                }
            }

            Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}\r\nException Details:\r\n{1}\r\n{2}", title, dialogMessage, rootCause.ToString()), ErrText.FailText);

            try
            {
                using (Form errorDialog = new UnexpectedErrorDialog(dialogTitle, dialogMessage, rootCause))
                {
                    if (owner != null)
                        errorDialog.ShowDialog(owner);
                    else
                        errorDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Failure while attempting to show unexpected error", ex.Message);
            }
        }

        /// <summary>
        /// Format the title message
        /// </summary>
        /// <returns>title message</returns>
        private string FormatTitle()
        {
            if (title == null || title.Length == 0)
                return "Error";
            else
                return title;
        }

        /// <summary>
        /// Format the main error message text
        /// </summary>
        /// <param name="args">optional substitution parameters</param>
        /// <returns>formatted error text</returns>
        private string FormatText(params object[] args)
        {
            // format text (substitute arguments)
            string formattedText;
            if (args != null && args.Length != 0)
                formattedText = String.Format(CultureInfo.CurrentCulture, Text, args);
            else
                formattedText = Text;

            // return the formatted text
            return formattedText;

        }
    }

    public class ErrText
    {
        /// <summary>
        /// Fail text.
        /// </summary>
        public const string FailText = "Fail";
    }
}
