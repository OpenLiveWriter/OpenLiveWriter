// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    public class CommandResourceLoader
    {
        private static readonly ResourcedPropertyLoader _resourcedPropertyLoader = new ResourcedPropertyLoader(
            typeof(Command),
            new ResourceManager("OpenLiveWriter.Localization.Properties", typeof(CommandId).Assembly),
            new ResourceManager("OpenLiveWriter.Localization.PropertiesNonLoc", typeof(CommandId).Assembly)
            );
        private static readonly IDictionary _commandMainMenuPaths;

        static CommandResourceLoader()
        {
            using (new QuickTimer("Parse main menu structure"))
                _commandMainMenuPaths = new MainMenuParser(_resourcedPropertyLoader.LocalizedResources, _resourcedPropertyLoader.NonLocalizedResources).Parse();
        }

        public static void ApplyResources(Command command)
        {
            _resourcedPropertyLoader.ApplyResources(command, command.Identifier);
        }

        public static Bitmap MissingLarge
        {
            get { return LoadCommandBitmap("Missing", "LargeImage"); }
        }

        public static Bitmap MissingSmall
        {
            get { return LoadCommandBitmap("Missing", "SmallImage"); }
        }

        public static Bitmap LoadCommandBitmap(string commandId, string bitmapType)
        {
            return ResourceHelper.LoadBitmap(commandId + "_" + bitmapType);
        }

        /// <summary>
        /// Parses the structure of the main menu into a hashtable whose keys are
        /// command identifier strings and values are localized MainMenuPath strings.
        ///
        /// The structure is stored as a string in PropertiesNonLoc.resx under
        /// the name "MainMenuStructure". Here's an example of the format:
        ///
        /// (File@0 NewPost@0 NewPage@1 OpenPost@2 -SavePost@3 PostAsDraft@4 PostAsDraftAndEditOnline@5 -DeleteDraft@6 -PostAndPublish@7 -Close@8) (Edit@1 Undo@0 Redo@1 -Cut@2 Copy@3 Paste@4 PasteSpecial@5 -Clear@6 -SelectAll@7 Find@8) (View@2 ViewNormal@0 ViewWebLayout@1 ViewPreview@2 ViewCode@3 -UpdateWeblogStyle@4 -ShowMenu@5 ViewSidebar@6 PostProperties@7) (Insert@3 InsertLink@0 InsertPicture@1 -InsertTable2@2) (Format@4 Font@0 FontColor@1 (-Align@2 AlignLeft@0 AlignCenter@1 AlignRight@2) -Numbers@3 Bullets@4 Blockquote@5 -InsertExtendedEntry@6) (Table@5 InsertTable@0 -TableProperties@1 RowProperties@2 ColumnProperties@3 CellProperties@4 -InsertRowAbove@5 InsertRowBelow@6 MoveRowUp@7 MoveRowDown@8 -InsertColumnLeft@9 InsertColumnRight@10 MoveColumnLeft@11 MoveColumnRight@12 -DeleteTable@13 DeleteRow@14 DeleteColumn@15 -ClearCell@16) (Tools@6 CheckSpelling@0 -Preferences@1 -DiagnosticsConsole@2 ForceWatson@3 BlogClientOptions@4 ShowDisplayMessageTestForm@5) (Weblog@7 ViewWeblog@0 ViewWeblogAdmin@1 -ConfigureWeblog@2 -AddWeblog@9999) (Help@8 Help@0 -SendFeedback@1 -About@2)
        ///
        /// Each menu or sub-menu is surrounded by parentheses. The first
        /// item after the open-parenthesis is the title of that menu;
        /// the remaining items are the commands or (sub-menus) of the menu.
        ///
        /// The localized text for each menu/sub-menu title is in Properties.resx
        /// under the name "MainMenu.[menu]", for example "MainMenu.File". The localized
        /// text for each command is under the name "Command.[command].MenuText", for
        /// example "Command.NewPost.MenuText".
        /// </summary>
        private class MainMenuParser
        {
            private readonly ResourceManager loc;
            private readonly ResourceManager nonLoc;

            public MainMenuParser(ResourceManager loc, ResourceManager nonLoc)
            {
                this.loc = loc;
                this.nonLoc = nonLoc;
            }

            /// <summary>
            /// Returns a hashtable whose keys are command ID strings and
            /// whose values are localized MainMenuPath strings.
            /// </summary>
            public Hashtable Parse()
            {
                string data = nonLoc.GetString("MainMenuStructure");
                if (data == null)
                    data = "";
                Hashtable results = new Hashtable();
                Parse(new StringReader(data), results, "");
                return results;
            }

            /// <summary>
            /// Parse the body of a menu
            /// </summary>
            private void Parse(StringReader data, Hashtable results, string prefix)
            {
                int c;
                while (-1 != (c = data.Peek()))
                {
                    if (c == '(')
                    {
                        // sub-menu begins
                        data.Read(); // eat the (
                        string menuId;
                        string menuPath = ReadNextEntry(true, prefix, data, out menuId);
                        Parse(data, results, menuPath);
                    }
                    else if (c == ')')
                    {
                        // this menu ends
                        data.Read();
                        if (data.Peek() == ' ')
                            data.Read();
                        return;
                    }
                    else
                    {
                        string commandId;
                        string commandMenuPath = ReadNextEntry(false, prefix, data, out commandId);
                        results.Add(commandId, commandMenuPath);
                    }
                }
            }

            private string ReadNextEntry(bool isMenu, string prefix, StringReader data, out string id)
            {
                StringBuilder chunk = new StringBuilder();
                bool hasSeparator = false;
                id = null;
                int c;
                while (-1 != (c = data.Peek()))
                {
                    if (c == ')')
                    {
                        break;
                    }

                    data.Read();

                    if (c == ' ')
                    {
                        break;
                    }

                    if (c == '@')
                    {
                        id = chunk.ToString();
                        chunk = new StringBuilder();
                        continue;
                    }

                    if (c == '-' && id == null && chunk.Length == 0)
                    {
                        hasSeparator = true;
                        continue;
                    }

                    chunk.Append((char)c);
                }

                if (id == null)
                    throw new ArgumentException("Malformed menu structure");

                int position = int.Parse(chunk.ToString(), CultureInfo.InvariantCulture);

                string locString = loc.GetString(string.Format(CultureInfo.InvariantCulture, isMenu ? "MainMenu.{0}" : "Command.{0}.MenuText", id));

                if (Res.DebugMode)
                {
                    locString = string.Format(CultureInfo.InvariantCulture, isMenu ? "MainMenu.{0}" : "Command.{0}.MenuText", id);
                }

                if (locString == null)
                {
                    throw new ArgumentException("No loc string for " + id);
                }

                return
                    (prefix.Length == 0 ? "" : prefix + "/")
                    + (hasSeparator ? "-" : "")
                    + locString
                    + "@"
                    + position.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
