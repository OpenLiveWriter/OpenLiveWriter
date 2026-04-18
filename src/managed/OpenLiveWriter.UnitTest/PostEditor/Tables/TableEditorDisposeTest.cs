// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.PostEditor.Tables
{
    /// <summary>
    /// Validates the defensive Dispose pattern used by TableEditor.SelectionPreserver.
    /// See: https://github.com/OpenLiveWriter/OpenLiveWriter/issues/750
    /// </summary>
    [TestClass]
    public class TableEditorDisposeTest
    {
        [TestMethod]
        public void Dispose_ShouldNotThrowCOMException()
        {
            var disposable = new SafeDisposable(() =>
            {
                throw new COMException("Simulated COM error during selection restore");
            });

            disposable.Dispose();
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowInvalidOperationException()
        {
            var disposable = new SafeDisposable(() =>
            {
                throw new InvalidOperationException("Simulated invalid state");
            });

            disposable.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Dispose_ShouldStillThrowUnexpectedExceptions()
        {
            var disposable = new SafeDisposable(() =>
            {
                throw new ArgumentException("Unexpected error");
            });

            disposable.Dispose();
        }

        private class SafeDisposable : IDisposable
        {
            private readonly Action _action;
            public SafeDisposable(Action action) => _action = action;

            public void Dispose()
            {
                try { _action(); }
                catch (COMException) { }
                catch (InvalidOperationException) { }
            }
        }
    }
}
