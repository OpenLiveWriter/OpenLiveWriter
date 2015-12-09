// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public interface IOverridableCommand
    {
        ContextAvailability ContextAvailability { get; set; }

        bool Enabled { get; set; }

        int OverrideProperty(ref PropertyKey key, PropVariantRef overrideValue);
        int CancelOverride(ref PropertyKey key);
    }

    public class OverridableCommand : Command, IOverridableCommand
    {
        protected Dictionary<PropertyKey, PropVariant> _overrides = new Dictionary<PropertyKey, PropVariant>();
        public OverridableCommand(CommandId commandId)
            : base(commandId)
        {
        }

        public virtual int OverrideProperty(ref PropertyKey key, PropVariantRef overrideValue)
        {
            if (key == PropertyKeys.Enabled)
            {
                bool currentValue = Enabled;
                _overrides[key] = overrideValue.PropVariant;
                UpdateInvalidationStateAndNotifyIfDifferent(ref key, Convert.ToBoolean(overrideValue.PropVariant.Value, CultureInfo.InvariantCulture), () => currentValue);
                return HRESULT.S_OK;
            }
            else if (key == PropertyKeys.ContextAvailable)
            {
                uint currentValue = (uint)ContextAvailability;
                _overrides[key] = overrideValue.PropVariant;
                UpdateInvalidationStateAndNotifyIfDifferent(ref key, Convert.ToUInt32(overrideValue.PropVariant.Value, CultureInfo.InvariantCulture), () => currentValue);
                return HRESULT.S_OK;
            }

            return HRESULT.E_INVALIDARG;
        }

        protected int RemoveOverride(ref PropertyKey key, IComparableDelegate currentValueDelegate)
        {
            PropVariant overrideValue;
            if (_overrides.TryGetValue(key, out overrideValue))
            {
                _overrides.Remove(key);
                UpdateInvalidationStateAndNotifyIfDifferent(ref key, (IComparable)overrideValue.Value, currentValueDelegate);
                return HRESULT.S_OK;
            }

            return HRESULT.S_FALSE;
        }

        public virtual int CancelOverride(ref PropertyKey key)
        {
            if (key == PropertyKeys.Enabled)
            {
                return RemoveOverride(ref key, () => Enabled);
            }
            if (key == PropertyKeys.ContextAvailable)
            {
                return RemoveOverride(ref key, () => (uint)ContextAvailability);
            }

            return HRESULT.E_INVALIDARG;
        }

        protected object GetOverride(ref PropertyKey key, object defaultValue)
        {
            PropVariant propVariant;
            if (_overrides.TryGetValue(key, out propVariant))
                return propVariant.Value;

            return defaultValue;
        }

        protected delegate IComparable IComparableDelegate();

        private void UpdateInvalidationStateAndNotifyIfDifferent(ref PropertyKey key, IComparable overrideValue, IComparableDelegate currentValueDelegate)// where T : IComparable
        {
            // Only invalidate if we're actually making a change.
            // Unnecessary invalidations hurt perf as well as cause ribbon bugs.
            if (overrideValue.CompareTo(currentValueDelegate()) != 0)
            {
                UpdateInvalidationState(key, InvalidationState.Pending);
                OnStateChanged(EventArgs.Empty);
            }
        }

        #region Implementation of IOverridableCommand

        public virtual ContextAvailability ContextAvailability
        {
            get { throw new System.NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override bool Enabled
        {
            get
            {
                return Convert.ToBoolean(GetOverride(ref PropertyKeys.Enabled, enabled), CultureInfo.InvariantCulture);
            }

            set
            {
                if (enabled != value)
                {
                    bool currentValue = Enabled;
                    enabled = value;
                    UpdateInvalidationStateAndNotifyIfDifferent(ref PropertyKeys.Enabled, Enabled, () => currentValue);

                    // If changing in High Contrast Black mode, force a refresh of the image
                    if (CoreServices.ApplicationEnvironment.IsHighContrastBlack)
                    {
                        Invalidate(new PropertyKey[] { PropertyKeys.SmallHighContrastImage, PropertyKeys.LargeHighContrastImage });
                    }
                }
            }
        }

        #endregion
    }
}
