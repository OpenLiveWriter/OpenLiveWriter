// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Use this attribute to mark forms that derive from ApplicationDialog
    /// that should save their location and/or size whenever they close.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PersistentWindowAttribute : Attribute
    {
        private string windowKey;
        private bool location = false;
        private bool size = true;

        public PersistentWindowAttribute(string windowKey)
        {
            this.windowKey = windowKey;
        }

        /// <summary>
        /// The registry key where this window will be saved.  Must be
        /// unique to each window type.
        /// </summary>
        public string WindowKey
        {
            get { return windowKey; }
            set { this.windowKey = value; }
        }

        /// <summary>
        /// True if Location should be saved/restored.
        /// </summary>
        public bool Location
        {
            get { return location; }
            set { this.location = value; }
        }

        /// <summary>
        /// True if Size should be saved/restored.
        /// </summary>
        public bool Size
        {
            get { return size; }
            set { this.size = value; }
        }

        private static PersistentWindowAttribute GetAttrib(object o)
        {
            object[] results = o.GetType().GetCustomAttributes(typeof(PersistentWindowAttribute), false);
            if (results == null || results.Length == 0)
                return null;

            Debug.Assert(results.Length == 1, "More than one PersistentWindowAttribute applied to " + o.GetType().FullName);

            return results[0] as PersistentWindowAttribute;
        }

        public static void MaybeRestore(Form form)
        {
            PersistentWindowAttribute pwa = GetAttrib(form);
            if (pwa == null)
                return;

            Rectangle bounds = LoadWindowBounds(pwa.WindowKey, form.Bounds);
            if (pwa.Size && pwa.Location)
                form.Bounds = bounds;
            else if (pwa.Size)
                form.Size = bounds.Size;
            else if (pwa.Location)
                form.Location = bounds.Location;
        }

        public static void MaybePersist(Form form)
        {
            PersistentWindowAttribute pwa = GetAttrib(form);
            if (pwa == null)
                return;

            SaveWindowBounds(pwa.WindowKey, form.Bounds);
        }

        static PersistentWindowAttribute()
        {
            windowSizesPersisterHelper = ApplicationEnvironment.UserSettingsRoot.GetSubSettings(@"WindowBounds");

        }
        private static SettingsPersisterHelper windowSizesPersisterHelper;

        public static Rectangle LoadWindowBounds(string key, Rectangle defaultBounds)
        {
            return windowSizesPersisterHelper.GetRectangle(key, defaultBounds);
        }

        public static void SaveWindowBounds(string key, Form form)
        {
            SaveWindowBounds(key, form.Bounds);
        }

        public static void SaveWindowBounds(string key, Rectangle bounds)
        {
            windowSizesPersisterHelper.SetRectangle(key, bounds);
        }

        public static void ApplyWindowSize(string key, Form form, Size defaultSize)
        {
            Rectangle bounds = windowSizesPersisterHelper.GetRectangle(key, new Rectangle(Point.Empty, defaultSize));
            form.Size = bounds.Size;
        }

        public static void ApplyWindowBounds(string key, Form form, Rectangle defaultBounds)
        {
            form.Bounds = windowSizesPersisterHelper.GetRectangle(key, defaultBounds);
        }

    }
}
