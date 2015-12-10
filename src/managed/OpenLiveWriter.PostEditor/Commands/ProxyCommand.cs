// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    class ProxyCommand : Command
    {
        private Command _command;
        public ProxyCommand(CommandId commandId, Command command) : base(commandId)
        {
            _command = command;
        }

        public override bool Enabled
        {
            get
            {
                return _command.Enabled;
            }
            set
            {
                _command.Enabled = value;
            }
        }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            _command.GetPropVariant(key, currentValue, ref value);
        }

        public override void Invalidate()
        {
            _command.Invalidate();
        }

        public override string LabelTitle
        {
            get
            {
                return _command.LabelTitle;
            }
            set
            {
                _command.LabelTitle = value;
            }
        }

        public override System.Drawing.Bitmap LargeHighContrastImage
        {
            get
            {
                return _command.LargeHighContrastImage;
            }
            set
            {
                _command.LargeHighContrastImage = value;
            }
        }

        public override System.Drawing.Bitmap LargeImage
        {
            get
            {
                return _command.LargeImage;
            }
            set
            {
                _command.LargeImage = value;
            }
        }

        public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            return _command.PerformExecute(verb, key, currentValue, commandExecutionProperties);
        }

        protected override void PerformExecuteWithArgs(CommandExecutionVerb verb, ExecuteEventHandlerArgs args)
        {
            _command.PerformExecuteWithArgs(args);
        }

        public override System.Drawing.Bitmap SmallHighContrastImage
        {
            get
            {
                return _command.SmallHighContrastImage;
            }
            set
            {
                _command.SmallHighContrastImage = value;
            }
        }

        public override System.Drawing.Bitmap SmallImage
        {
            get
            {
                return _command.SmallImage;
            }
            set
            {
                _command.SmallImage = value;
            }
        }

        public override string TooltipDescription
        {
            get
            {
                return _command.TooltipDescription;
            }
            set
            {
                _command.TooltipDescription = value;
            }
        }

        public override int UpdateProperty(ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            return _command.UpdateProperty(ref key, currentValue, out newValue);
        }
    }
}
