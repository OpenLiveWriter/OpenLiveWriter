// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.PostEditor.Tables
{
    public sealed class TableEditingSettings
    {
        public static bool TableEditingEnabled
        {
            get
            {
                return true;
                //return SettingsKey.GetBoolean("Enabled", false);
            }
        }

        public static int DefaultRows
        {
            get { return SettingsKey.GetInt32(DEFAULT_ROWS, 2); }
            set { SettingsKey.SetInt32(DEFAULT_ROWS, value); }
        }
        private const string DEFAULT_ROWS = "DefaultRows";

        public static int DefaultColumns
        {
            get { return SettingsKey.GetInt32(DEFAULT_COLUMNS, 2); }
            set { SettingsKey.SetInt32(DEFAULT_COLUMNS, value); }
        }
        private const string DEFAULT_COLUMNS = "DefaultColumns";

        public static string DefaultCellPadding
        {
            get { return SettingsKey.GetString(DEFAULT_CELL_PADDING, "2"); }
            set { SettingsKey.SetString(DEFAULT_CELL_PADDING, value); }
        }
        private const string DEFAULT_CELL_PADDING = "DefaultCellPadding";

        public static string DefaultCellSpacing
        {
            get { return SettingsKey.GetString(DEFAULT_CELL_SPACING, "0"); }
            set { SettingsKey.SetString(DEFAULT_CELL_SPACING, value); }
        }
        private const string DEFAULT_CELL_SPACING = "DefaultCellSpacing";

        public static string DefaultBorderSize
        {
            get { return SettingsKey.GetString(DEFAULT_BORDER_SIZE, "0"); }
            set { SettingsKey.SetString(DEFAULT_BORDER_SIZE, value); }
        }
        private const string DEFAULT_BORDER_SIZE = "DefaultBorderSize";

        public static int DefaultWidth
        {
            get { return SettingsKey.GetInt32(DEFAULT_WIDTH, 400); }
            set { SettingsKey.SetInt32(DEFAULT_WIDTH, value); }
        }
        private const string DEFAULT_WIDTH = "DefaultWidth";

        internal static SettingsPersisterHelper SettingsKey = PostEditorSettings.SettingsKey.GetSubSettings("TableEditing");
    }
}
