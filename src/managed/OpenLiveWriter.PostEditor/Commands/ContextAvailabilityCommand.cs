// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class ContextAvailabilityCommand : OverridableCommand
    {
        public ContextAvailabilityCommand(CommandId commandId)
            : base(commandId)
        { }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.ContextAvailable)
            {
                value.SetUInt((uint)ContextAvailability);
            }
            else
                base.GetPropVariant(key, currentValue, ref value);
        }

        private ContextAvailability _contextAvailability = ContextAvailability.NotAvailable;

        public ContextAvailability InnerContextAvailability
        {
            get
            {
                return _contextAvailability;
            }
        }
        public override ContextAvailability ContextAvailability
        {
            get
            {
                return (ContextAvailability)Convert.ToUInt32(GetOverride(ref PropertyKeys.ContextAvailable, (uint)_contextAvailability), CultureInfo.InvariantCulture);
            }
            set
            {
                if (ContextAvailability != value)
                {
                    _contextAvailability = value;
                    UpdateInvalidationState(PropertyKeys.ContextAvailable, InvalidationState.Pending);
                    OnStateChanged(EventArgs.Empty);
                }
            }
        }
    }
}
