// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents an instance of a Taskbar button jump list.
    /// </summary>
    public class JumpList
    {
        /// <summary>
        /// Create a JumpList for the application's taskbar button.
        /// </summary>
        /// <param name="appid">Application Id for the individual window. This must be unique for each top-level window in order to have a individual JumpList.</param>
        /// <param name="windowHandle">Handle of the window associated with the new JumpList</param>
        /// <returns>A new JumpList that is associated with the specific window handle</returns>
        public static JumpList CreateJumpListForIndividualWindow(string appid, IntPtr windowHandle)
        {
            return new JumpList(appid, windowHandle);
        }

        // Best practice recommends defining a private object to lock on
        private static Object syncLock = new Object();

        // Native implementation of destination list
        private ICustomDestinationList customDestinationList;

        #region Properties

        private JumpListCustomCategoryCollection customCategoriesCollection;
        /// <summary>
        /// Adds a collection of custom categories to the Taskbar jump list.
        /// </summary>
        /// <param name="customCategories">The catagories to add to the jump list.</param>
        public void AddCustomCategories(params JumpListCustomCategory[] customCategories)
        {
            if (customCategoriesCollection == null)
            {
                // Make sure that we don't create multiple instances
                // of this object
                lock (syncLock)
                {
                    if (customCategoriesCollection == null)
                    {
                        customCategoriesCollection = new JumpListCustomCategoryCollection();
                    }
                }
            }

            foreach (JumpListCustomCategory category in customCategories)
                customCategoriesCollection.Add(category);
        }

        /// <summary>
        /// Gets or sets the type of known categories to display.
        /// </summary>
        public JumpListKnownCategoryType KnownCategoryToDisplay { get; set; }

        private int knownCategoryOrdinalPosition = 0;
        /// <summary>
        /// Gets or sets the value for the known category location relative to the
        /// custom category collection.
        /// </summary>
        public int KnownCategoryOrdinalPosition
        {
            get
            {
                return knownCategoryOrdinalPosition;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Negative numbers are not allowed for the ordinal position.");

                knownCategoryOrdinalPosition = value;
            }

        }

        /// <summary>
        /// Gets or sets the application ID to use for this jump list.
        /// </summary>
        private string ApplicationId { get; set; }

        #endregion

        /// <summary>
        /// Creates a new instance of the JumpList class with the specified
        /// appId. The JumpList is associated with the given window.
        /// </summary>
        /// <param name="appID">Application Id to use for this instance.</param>
        /// <param name="windowHandle">Window handle for the window that is associated with this JumpList</param>
        protected JumpList(string appID, IntPtr windowHandle)
        {
            // Throw exception if not running on Win7 or newer
            PlatformHelper.ThrowIfNotWin7OrHigher();

            // Native implementation of destination list
            customDestinationList = (ICustomDestinationList)new CDestinationList();

            // Set application user model ID
            if (!string.IsNullOrEmpty(appID))
            {
                ApplicationId = appID;

                // If the user hasn't yet set the application id for the whole process,
                // use the first JumpList's AppId for the whole process. This will ensure
                // we have the same JumpList for all the windows (unless user overrides and creates a new
                // JumpList for a specific child window)
                if (!TaskbarManager.Instance.ApplicationIdSetProcessWide)
                    TaskbarManager.Instance.ApplicationId = appID;

                if (windowHandle != IntPtr.Zero)
                    TaskbarManager.Instance.SetApplicationIdForSpecificWindow(windowHandle, appID);
            }
        }

        /// <summary>
        /// Commits the pending JumpList changes and refreshes the Taskbar.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Will throw if the type of the file being added to the JumpList is not registered with the application.</exception>
        /// <exception cref="System.UnauthorizedAccessException">Will throw if recent documents tracking is turned off by the user or via group policy.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Will throw if updating the JumpList fails for any other reason.</exception>
        public void Refresh()
        {
            // Let the taskbar know which specific jumplist we are updating
            if (!string.IsNullOrEmpty(ApplicationId))
                customDestinationList.SetAppID(ApplicationId);

            // Begins rendering on the taskbar destination list
            BeginList();

            // Even it fails, continue appending the custom categories
            try
            {
                // Add custom categories
                AppendCustomCategories();
            }
            finally
            {
                // End rendering of the taskbar destination list
                customDestinationList.CommitList();
            }
        }

        /// <summary>
        /// Returns true if a JumpList can be instantiated at this time.
        /// </summary>
        public static bool CanShowJumpList()
        {
            return PlatformHelper.RunningOnWin7OrHigher();
        }

        private void BeginList()
        {
            // Get list of removed items from native code
            object removedItems;
            uint maxSlotsInList = 10; // default

            // Native call to start adding items to the taskbar destination list
            int hr = customDestinationList.BeginList(
                out maxSlotsInList,
                ref Shell32.IObjectArray,
                out removedItems);

            if (!ComHelper.SUCCEEDED(hr))
                Marshal.ThrowExceptionForHR(hr);
        }

        private static void DumpFileRegistration(JumpListCustomCategory category)
        {
            try
            {
                Trace.WriteLine("Dumping file registration for category " + category.Name + " in response to jumplist error:");

                Dictionary<string, bool> fileTypes = new Dictionary<string, bool>();
                foreach (IJumpListItem link in category.JumpListItems)
                {
                    bool error = false;
                    string extension = Path.GetExtension(link.Path);
                    if (link is JumpListItem)
                    {
                        JumpListItem item = (JumpListItem)link;
                        Trace.WriteLine("JumpListItem: " + item.ParsingName + " with path: " + item.Path);

                        // Verify the file registration for this file type.
                        if (!fileTypes.ContainsKey(extension))
                        {
                            Trace.WriteLine("Dumping file registration for extension: " + extension);
                            RegistryHelper.DumpKey(Registry.LocalMachine, @"SOFTWARE\Classes\" + extension);

                            Trace.WriteLine("Dumping ProgId for extension: " + extension);
                            string progId = FileHelper.GetProgIDFromExtension(extension);
                            if (String.IsNullOrEmpty(progId))
                            {
                                error = true;
                                Trace.WriteLine("ERROR: ProgId missing for extension: " + extension);
                            }
                            else
                            {
                                RegistryHelper.DumpKey(Registry.LocalMachine, @"SOFTWARE\Classes\" + progId);

                                // Detect the UserAppModelId
                                string appUserModelID = RegistryHelper.GetAppUserModelID(progId);
                                if (String.IsNullOrEmpty(appUserModelID))
                                {
                                    error = true;
                                    Trace.WriteLine("ERROR: Missing AppUserModelID for " + progId);
                                }
                                else if (!appUserModelID.Equals(TaskbarManager.Instance.ApplicationId, StringComparison.Ordinal))
                                {
                                    error = true;
                                    Trace.WriteLine("ERROR: Incorrect AppUserModelID for " + progId + ": " + appUserModelID);
                                }
                            }
                        }
                        // else already verified/dumped
                    }
                    else
                    {
                        error = true;
                        Trace.WriteLine("ERROR: Adding something that is not a JumpListItem: " + link.Path);
                    }

                    if (!fileTypes.ContainsKey(extension))
                        fileTypes.Add(extension, error);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception thrown while trying to dump file registration: " + ex);
                throw;
            }
        }

        private void AppendCustomCategories()
        {
            // Initialize our current index in the custom categories list
            int currentIndex = 0;

            // Keep track whether we add the Known Categories to our list
            bool knownCategoriesAdded = false;

            if (customCategoriesCollection != null)
            {
                // Append each category to list
                foreach (JumpListCustomCategory category in customCategoriesCollection)
                {
                    // If our current index is same as the KnownCategory OrdinalPosition,
                    // append the Known Categories
                    if (!knownCategoriesAdded && currentIndex == KnownCategoryOrdinalPosition)
                    {
                        AppendKnownCategories();
                        knownCategoriesAdded = true;
                    }

                    // Don't process empty categories
                    if (category.JumpListItems.Count == 0)
                        continue;

                    IObjectCollection categoryContent =
                        (IObjectCollection)new CEnumerableObjectCollection();

                    // Add each link's shell representation to the object array
                    foreach (IJumpListItem link in category.JumpListItems)
                    {
                        if (link is JumpListItem)
                            categoryContent.AddObject(((JumpListItem)link).NativeShellItem);
                    }

                    // Add current category to destination list
                    int hr = customDestinationList.AppendCategory(
                        category.Name,
                        (IObjectArray)categoryContent);

                    if (!ComHelper.SUCCEEDED(hr))
                    {
                        if ((uint)hr == 0x80040F03)
                        {
                            DumpFileRegistration(category);
                            throw new InvalidOperationException("The file type is not registered with this application.");
                        }
                        else if ((uint)hr == 0x80070005 /*E_ACCESSDENIED*/)
                        {
                            // If the recent documents tracking is turned off by the user,
                            // custom categories or items to an existing category cannot be added.
                            // The recent documents tracking can be changed via:
                            //      1. Group Policy “Do not keep history of recently opened documents”.
                            //      2. Via the user setting “Store and display recently opened items in
                            //         the Start menu and the taskbar” in the Start menu property dialog.
                            //
                            throw new UnauthorizedAccessException("Custom categories cannot be added while recent documents tracking is turned off.");
                        }
                        else
                            Marshal.ThrowExceptionForHR(hr);
                    }

                    // Increase our current index
                    currentIndex++;
                }
            }

            // If the ordinal position was out of range, append the Known Categories
            // at the end
            if (!knownCategoriesAdded)
                AppendKnownCategories();
        }

        private void AppendKnownCategories()
        {
            if (KnownCategoryToDisplay == JumpListKnownCategoryType.Recent)
                customDestinationList.AppendKnownCategory(Shell32.KNOWNDESTCATEGORY.KDC_RECENT);
            else if (KnownCategoryToDisplay == JumpListKnownCategoryType.Frequent)
                customDestinationList.AppendKnownCategory(Shell32.KNOWNDESTCATEGORY.KDC_FREQUENT);
        }
    }
}
