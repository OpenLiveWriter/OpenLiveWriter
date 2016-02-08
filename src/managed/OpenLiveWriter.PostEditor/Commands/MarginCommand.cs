// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class MarginCommand : Command
    {
        private const decimal MinMargin = 0;//(decimal)0.00001;
        private const decimal MaxMargin = 999;
        private const int Increment = 1;
        private const uint DecimalPlaces = 0;

        private string RepresentativeString;
        private string FormatString;

        private SpinnerCommand commandLeftMargin;
        private SpinnerCommand commandTopMargin;
        private SpinnerCommand commandRightMargin;
        private SpinnerCommand commandBottomMargin;
        private Padding marginValue;

        public MarginCommand(CommandManager commandManager)
            : base(CommandId.MarginsGroup)
        {
            RepresentativeString = Res.Get(StringId.SpinnerPixelRepresentativeString);
            FormatString = Res.Get(StringId.SpinnerPixelFormatString);

            // @RIBBON TODO: Have a way to initialize this.
            marginValue = new Padding(Convert.ToInt32(MinMargin));

            commandManager.BeginUpdate();

            commandLeftMargin = new SpinnerCommand(CommandId.AdjustLeftMargin, MinMargin, MaxMargin, MinMargin, Increment, DecimalPlaces, RepresentativeString, FormatString);
            commandManager.Add(commandLeftMargin, commandMargin_ExecuteWithArgs);

            commandTopMargin = new SpinnerCommand(CommandId.AdjustTopMargin, MinMargin, MaxMargin, MinMargin, Increment, DecimalPlaces, RepresentativeString, FormatString);
            commandManager.Add(commandTopMargin, commandMargin_ExecuteWithArgs);

            commandRightMargin = new SpinnerCommand(CommandId.AdjustRightMargin, MinMargin, MaxMargin, MinMargin, Increment, DecimalPlaces, RepresentativeString, FormatString);
            commandManager.Add(commandRightMargin, commandMargin_ExecuteWithArgs);

            commandBottomMargin = new SpinnerCommand(CommandId.AdjustBottomMargin, MinMargin, MaxMargin, MinMargin, Increment, DecimalPlaces, RepresentativeString, FormatString);
            commandManager.Add(commandBottomMargin, commandMargin_ExecuteWithArgs);

            commandManager.EndUpdate();
        }

        public bool IsZero()
        {
            return (marginValue.Left == 0 && marginValue.Top == 0 && marginValue.Right == 0 && marginValue.Bottom == 0);
        }

        public event EventHandler MarginChanged;

        public int Left { get { return marginValue.Left; } }
        public int Top { get { return marginValue.Top; } }
        public int Right { get { return marginValue.Right; } }
        public int Bottom { get { return marginValue.Bottom; } }

        public Padding Value
        {
            get { return marginValue; }
            set
            {
                marginValue = value;

                commandLeftMargin.Value = value.Left;
                commandTopMargin.Value = value.Top;
                commandRightMargin.Value = value.Right;
                commandBottomMargin.Value = value.Bottom;

                FireMarginChanged();
            }
        }

        void commandMargin_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            Command command = (Command)sender;

            int value = Convert.ToInt32(args.GetDecimal(command.CommandId.ToString()));
            Debug.WriteLine(command.LabelTitle + ": " + value);

            if (command.CommandId == commandLeftMargin.CommandId)
            {
                marginValue.Left = value;
                commandLeftMargin.Value = value;
            }
            else if (command.CommandId == commandRightMargin.CommandId)
            {
                marginValue.Right = value;
                commandRightMargin.Value = value;
            }
            else if (command.CommandId == commandBottomMargin.CommandId)
            {
                marginValue.Bottom = value;
                commandBottomMargin.Value = value;
            }
            else if (command.CommandId == commandTopMargin.CommandId)
            {
                marginValue.Top = value;
                commandTopMargin.Value = value;
            }

            FireMarginChanged();
        }

        private void FireMarginChanged()
        {
            if (MarginChanged != null)
                MarginChanged(this, EventArgs.Empty);
        }

        public void SetMargin(Padding? margin)
        {
            if (margin.HasValue)
                Value = margin.Value;
        }

        public override bool Enabled
        {
            set
            {
                if (Enabled != value)
                {
                    commandLeftMargin.Enabled = value;
                    commandTopMargin.Enabled = value;
                    commandRightMargin.Enabled = value;
                    commandBottomMargin.Enabled = value;
                }

                base.Enabled = value;
            }
        }
    }
}
