// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public delegate void RestoreBoolDelegate(bool value);

    /// <summary>
    /// Helper function to temporarily change a bool value.
    /// For example,
    /// using (new RestoreBool(ref myBool, myTempValue, b => { myBool = b; })
    /// {
    ///     // Do whatever work here that requires the temp value.
    /// }
    /// </summary>
    public class RestoreBool : IDisposable
    {
        private bool _originalValue;
        private RestoreBoolDelegate _restore;
        public RestoreBool(ref bool value, bool tempValue, RestoreBoolDelegate restore)
        {
            _originalValue = value;
            value = tempValue;
            _restore = restore;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _restore(_originalValue);
        }

        #endregion
    }
}
