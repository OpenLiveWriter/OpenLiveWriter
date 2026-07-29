// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpenLiveWriter.Interop.Windows.TaskDialog;

namespace OpenLiveWriter.UnitTest.Interop
{
    /// <summary>
    /// Layout regression tests for the TaskDialogIndirect interop structs.
    /// comctl32 v6.10 (Windows 11 24H2) rejects the naturally-aligned 64-bit
    /// layout (176 bytes) with E_INVALIDARG; it requires the packed SDK layout
    /// (160 bytes), so these structs must stay Pack = 1. The structs are
    /// internal, so they are inspected via reflection.
    /// </summary>
    [TestFixture]
    public class TaskDialogNativeTest
    {
        [Test]
        public void TaskDialogConfigUsesPackedLayout()
        {
            if (IntPtr.Size != 8)
                Assert.Ignore("layout assertions are for 64-bit");

            Type configType = typeof(TaskDialog).Assembly.GetType(
                "OpenLiveWriter.Interop.Windows.TaskDialog.TASKDIALOGCONFIG", true);
            Assert.AreEqual(160, Marshal.SizeOf(configType));
            Assert.AreEqual(140, (int)Marshal.OffsetOf(configType, "pfCallback"));
            Assert.AreEqual(156, (int)Marshal.OffsetOf(configType, "cxWidth"));
        }

        [Test]
        public void TaskDialogButtonUsesPackedLayout()
        {
            if (IntPtr.Size != 8)
                Assert.Ignore("layout assertions are for 64-bit");

            Type buttonType = typeof(TaskDialog).Assembly.GetType(
                "OpenLiveWriter.Interop.Windows.TaskDialog.TASKDIALOG_BUTTON", true);
            Assert.AreEqual(12, Marshal.SizeOf(buttonType));
            Assert.AreEqual(4, (int)Marshal.OffsetOf(buttonType, "pszButtonText"));
        }
    }
}
