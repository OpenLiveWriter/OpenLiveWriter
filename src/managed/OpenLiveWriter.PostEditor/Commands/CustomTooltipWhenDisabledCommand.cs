// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    class CustomTooltipWhenDisabledCommand : OverridableCommand
    {
        private string _disabledTooltipDescription;
        public CustomTooltipWhenDisabledCommand(CommandId commandId, string disabledTooltipDescription) : base(commandId)
        {
            _disabledTooltipDescription = disabledTooltipDescription;
        }

        public override string TooltipDescription
        {
            get
            {
                return Enabled ? base.TooltipDescription : _disabledTooltipDescription;
            }
        }

        public override bool Enabled
        {
            set
            {
                if (base.Enabled != value)
                    UpdateInvalidationState(PropertyKeys.TooltipDescription, InvalidationState.Pending);

                base.Enabled = value;
            }
        }

        public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            PerformExecuteWithArgs(verb, new ExecuteEventHandlerArgs());
            return HRESULT.S_OK;
        }
    }
}
