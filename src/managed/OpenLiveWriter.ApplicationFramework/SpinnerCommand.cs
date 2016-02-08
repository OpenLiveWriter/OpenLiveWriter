// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public interface IRepresentativeString
    {
        string RepresentativeString { get; }
    }

    public class SpinnerCommand : Command, IRepresentativeString
    {
        private decimal _minValue;
        private decimal _maxValue;
        private decimal _value;
        private decimal _increment;
        private uint _decimalPlaces;
        private string _representativeString;
        private string _formatString;

        public SpinnerCommand(CommandId commandId, decimal minValue, decimal maxValue, decimal initialValue, decimal increment, uint decimalPlaces, string representativeString, string formatString)
            : base(commandId)
        {
            Debug.Assert(initialValue >= minValue && initialValue <= maxValue, "Initial value is outside of allowed range.");
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._value = initialValue;
            this._increment = increment;
            this._decimalPlaces = decimalPlaces;
            this._representativeString = representativeString;
            this._formatString = formatString;

            UpdateInvalidationState(PropertyKeys.MinValue, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.MaxValue, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.DecimalValue, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.Increment, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.DecimalPlaces, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.RepresentativeString, InvalidationState.Pending);
            UpdateInvalidationState(PropertyKeys.FormatString, InvalidationState.Pending);
        }

        public decimal MinValue
        {
            get { return _minValue; }
        }

        public decimal MaxValue
        {
            get { return _maxValue; }
        }

        public decimal Value
        {
            get { return _value; }
            set
            {
                if (this._value != value)
                {
                    this._value = value;
                    UpdateInvalidationState(PropertyKeys.DecimalValue, InvalidationState.Pending);
                    OnStateChanged(EventArgs.Empty);
                }
            }
        }

        public decimal Increment
        {
            get { return _increment; }
        }

        public uint DecimalPlaces
        {
            get { return _decimalPlaces; }
        }

        public string RepresentativeString
        {
            get { return _representativeString; }
        }

        public string FormatString
        {
            get { return _formatString; }
        }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.DecimalValue)
            {
                value.SetDecimal(Value);
            }
            else if (key == PropertyKeys.MinValue)
            {
                value.SetDecimal(MinValue);
            }
            else if (key == PropertyKeys.MaxValue)
            {
                value.SetDecimal(MaxValue);
            }
            else if (key == PropertyKeys.Increment)
            {
                value.SetDecimal(Increment);
            }
            else if (key == PropertyKeys.DecimalPlaces)
            {
                value.SetUInt(DecimalPlaces);
            }
            else if (key == PropertyKeys.FormatString)
            {
                value.SetString(FormatString);
            }
            else
                base.GetPropVariant(key, currentValue, ref value);
        }

        public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            decimal spinnerValue = Convert.ToDecimal(currentValue.PropVariant.Value, CultureInfo.InvariantCulture);
            PerformExecuteWithArgs(verb, new ExecuteEventHandlerArgs(CommandId.ToString(), spinnerValue));
            return HRESULT.S_OK;
        }
    }
}
