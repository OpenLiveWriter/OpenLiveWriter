// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    public class DynamicCommandMenu : IDisposable
    {

        public DynamicCommandMenu(IDynamicCommandMenuContext context)
        {
            // save reference to command list context
            _context = context;

            // initialize commands
            InitializeCommands();
        }

        /// <summary>
        /// Get the underlying command identifiers managed by this dynamic command menu
        /// (this would allow us to embed these commands within the scope of a context-menu definition)
        /// </summary>
        public string[] CommandIdentifiers
        {
            get
            {
                ArrayList commands = new ArrayList(_commands.Count + 1);
                foreach (Command command in _commands)
                    commands.Add(command.Identifier);
                if (commandMore != null)
                    commands.Add(commandMore.Identifier);
                return (string[])commands.ToArray(typeof(string));
            }
        }

        // <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            foreach (Command command in _commands)
                Context.CommandManager.Remove(command);

            if (components != null)
            {
                components.Dispose();
            }
        }

        private void InitializeCommands()
        {
            Context.CommandManager.BeginUpdate();

            // add commands
            for (int i = 0; i < Context.Options.MaxCommandsShownOnMenu; i++)
            {
                // create the command
                Command command = new Command(this.components);

                // First command added should have a BeforeShowInMenu so that we can
                // dynamically update the contents of the menu
                if (i == 0)
                    command.BeforeShowInMenu += new EventHandler(commands_BeforeShowInMenu);

                // provide the command with a unique identifier
                command.Identifier = Guid.NewGuid().ToString();

                string separator = i == 0 && Context.Options.SeparatorBegin ? "-" : "";

                // define menu paths
                string menuMergeText = (Context.Options.MenuMergeOffset + i).ToString(CultureInfo.InvariantCulture);
                command.MenuText = (Context.Options.UseNumericMnemonics ? "&" + (i + 1).ToString(CultureInfo.InvariantCulture) + " {0}" : "{0}");
                command.MainMenuPath = Context.Options.MainMenuBasePath + "/" + separator + command.MenuText + "@" + menuMergeText;

                // generic execute handler for all window menu commands
                command.Execute += new EventHandler(command_Execute);

                // add the command to our internal list
                _commands.Add(command);

                // add the command to the system command manaager
                Context.CommandManager.Add(command);
            }

            // add 'more' command if appropriate
            if (Context.Options.MoreCommandsMenuCaption != null)
            {
                commandMore = new Command(this.components);
                commandMore.Identifier = Guid.NewGuid().ToString();
                commandMore.VisibleOnContextMenu = true;
                commandMore.VisibleOnMainMenu = true;
                commandMore.MenuText = Context.Options.MoreCommandsMenuCaption;
                commandMore.MainMenuPath = Context.Options.MainMenuBasePath + "/" + Context.Options.MoreCommandsMenuCaption + "@" + (Context.Options.MenuMergeOffset + Context.Options.MaxCommandsShownOnMenu).ToString(CultureInfo.InvariantCulture);
                commandMore.Execute += new EventHandler(commandMore_Execute);
                Context.CommandManager.Add(commandMore);
            }

            Context.CommandManager.EndUpdate();
        }

        /// <summary>
        /// BeforeShowInMenu to dynamically update the contents of the window menu
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="ea">event args</param>
        private void commands_BeforeShowInMenu(object sender, EventArgs ea)
        {
            // obtain the list of command objects
            IMenuCommandObject[] menuCommandObjects = Context.GetMenuCommandObjects();

            //	Adjust the commands
            for (int i = 0; i < _commands.Count; i++)
            {
                //	Get the command.
                Command command = _commands[i] as Command;

                //	If the command is beyond the set of command objects files turn it off.  Otherwise,
                //	turn it on and update it.
                if (i >= menuCommandObjects.Length)
                {
                    //  Turn the command off
                    command.VisibleOnContextMenu = false;
                    command.VisibleOnMainMenu = false;
                    command.Enabled = false;

                    //	Update the command.
                    command.Tag = String.Empty;
                    command.MenuFormatArgs = new object[] { String.Empty };
                }
                else
                {
                    //	Turn the command on
                    command.VisibleOnContextMenu = true;
                    command.VisibleOnMainMenu = true;
                    command.Enabled = menuCommandObjects[i].Enabled;
                    command.Latched = menuCommandObjects[i].Latched;

                    //	Update the command.
                    command.CommandBarButtonBitmapEnabled = menuCommandObjects[i].Image;
                    command.MenuFormatArgs = new object[] { menuCommandObjects[i].Caption };
                    command.Tag = menuCommandObjects[i];
                }
            }

            // show or hide the 'more' command as necessary
            if (commandMore != null)
            {
                bool showCommandMore = Context.Options.MoreCommandsMenuCaption != null && (menuCommandObjects.Length > _commands.Count);
                commandMore.VisibleOnContextMenu = showCommandMore;
                commandMore.VisibleOnMainMenu = showCommandMore;
                commandMore.Enabled = showCommandMore;
            }
        }

        private void command_Execute(object sender, EventArgs ea)
        {
            // notify context
            Context.CommandExecuted((sender as Command).Tag as IMenuCommandObject);
        }

        private void commandMore_Execute(object sender, EventArgs ea)
        {
            using (DynamicCommandMenuOverflowForm form = new DynamicCommandMenuOverflowForm(Context.GetMenuCommandObjects()))
            {
                // configure the title bar
                form.Text = Context.Options.MoreCommandsDialogTitle;

                // show the form
                using (new WaitCursor())
                {
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                        Context.CommandExecuted(form.SelectedObject);
                }
            }
        }

        private IDynamicCommandMenuContext Context
        {
            get { return _context; }
        }
        private IDynamicCommandMenuContext _context;

        private ArrayList _commands = new ArrayList();

        private Command commandMore;

        private Container components = new Container();

    }

    public interface IDynamicCommandMenuContext
    {
        /// <summary>
        /// Get the options for the menu
        /// </summary>
        DynamicCommandMenuOptions Options { get; }

        /// <summary>
        /// Command manager to add commands to
        /// </summary>
        CommandManager CommandManager { get; }

        /// <summary>
        /// Command objects to show on the menu
        /// </summary>
        /// <returns></returns>
        IMenuCommandObject[] GetMenuCommandObjects();

        /// <summary>
        /// Notification that the user executed a command
        /// </summary>
        /// <param name="commandObject"></param>
        void CommandExecuted(IMenuCommandObject menuCommandObject);
    }

    public interface IMenuCommandObject
    {
        Bitmap Image { get; }
        string Caption { get; }
        bool Enabled { get; }
        bool Latched { get; }
        string CaptionNoMnemonic { get; }
    }

    public class DynamicCommandMenuOptions
    {
        public DynamicCommandMenuOptions(string mainMenuBasePath, int menuMergeOffset)
            : this(mainMenuBasePath, menuMergeOffset, null, null)
        {
        }

        public DynamicCommandMenuOptions(
            string mainMenuBasePath, int menuMergeOffset,
            string moreCommandsMenuCaption, string moreCommandsDialogTitle)
        {
            // copy passed in values
            MainMenuBasePath = mainMenuBasePath;
            MenuMergeOffset = menuMergeOffset;
            MoreCommandsMenuCaption = moreCommandsMenuCaption;
            MoreCommandsDialogTitle = moreCommandsDialogTitle;
            SeparatorBegin = true;

            // default other options
            MaxCommandsShownOnMenu = 9;
            UseNumericMnemonics = true;
            SeparatorBegin = false;
        }

        /// <summary>
        /// Base path for main menu
        /// </summary>
        public readonly string MainMenuBasePath;

        /// <summary>
        /// Offset for menu items to be merged into the main menu
        /// </summary>
        public readonly int MenuMergeOffset;

        /// <summary>
        /// Menu caption to be used if more commands are available than are displayable on the menu
        /// (you can display up to 9 on the menu). If null then no 'More' option is provided.
        /// </summary>
        public readonly string MoreCommandsMenuCaption;

        /// <summary>
        /// Dialog title to be used when showing the more dialog
        /// </summary>
        public readonly string MoreCommandsDialogTitle;

        /// <summary>
        /// Maximum number of commands to show on the menu (must be from 1 to 9)
        /// </summary>
        public int MaxCommandsShownOnMenu
        {
            get
            {
                return _maxCommandsShownOnMenu;
            }
            set
            {
                if (value < 1 || (UseNumericMnemonics && (value > 9)))
                    throw new ArgumentException("Invalid value for MaxCommandsShownOnMenu");
                else
                    _maxCommandsShownOnMenu = value;
            }
        }
        private int _maxCommandsShownOnMenu;

        /// <summary>
        /// Should we add numeric Mnemonics to the commands
        /// </summary>
        public bool UseNumericMnemonics;

        /// <summary>
        /// Use a separator before the first command
        /// </summary>
        public bool SeparatorBegin;
    }

}

