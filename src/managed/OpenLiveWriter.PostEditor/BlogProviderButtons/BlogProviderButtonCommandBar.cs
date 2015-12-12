// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing ;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Globalization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;

namespace OpenLiveWriter.PostEditor.BlogProviderButtons
{
    internal sealed class BlogProviderButtonCommandBarInfo
    {
        public const int MaximumProviderCommands = 4 ;
        public const string ProviderCommandFormat = "BlogProviderButtonsProviderCommand{0}" ;
    }

    // @RIBBON TODO: Remove obsolete code
    [Obsolete]
    internal class BlogProviderButtonCommandBarControl : TransparentCommandBarControl
    {
        public BlogProviderButtonCommandBarControl()
            : base(	new BlogProviderButtonCommandBarLightweightControl(), CommandBarDefinition.Create(_providerCommandIds.ToArray()))
        {
        }

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
            base.OnPaintBackground (pevent);

            Graphics g = pevent.Graphics;
            g.ResetClip();
            g.ResetTransform();

            if(!ColorizedResources.UseSystemColors)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(64, Color.White)))
                    g.FillRectangle(b, ClientRectangle);
            }
        }

        public Size DesiredSize
        {
            get
            {
                return _commandBar.DefaultVirtualSize ;
            }
        }

        public bool HasButtons
        {
            get
            {
                Command firstButtonCommand = ApplicationManager.CommandManager.Get(String.Format(CultureInfo.InvariantCulture, BlogProviderButtonCommandBarInfo.ProviderCommandFormat, 0)) ;
                return firstButtonCommand.On ;
            }
        }


        static BlogProviderButtonCommandBarControl()
        {
            for (int i=0; i<BlogProviderButtonCommandBarInfo.MaximumProviderCommands; i++ )
                _providerCommandIds.Add(String.Format(CultureInfo.InvariantCulture, BlogProviderButtonCommandBarInfo.ProviderCommandFormat, i)) ;
        }

        private static readonly ArrayList _providerCommandIds = new ArrayList();

        private class BlogProviderButtonCommandBarLightweightControl : ApplicationCommandBarLightweightControl
        {
            public BlogProviderButtonCommandBarLightweightControl()
            {
                CommandManager = ApplicationManager.CommandManager ;
            }

            public override int BottomLayoutMargin
            {
                get
                {
                    // reflection is so light that we count this as our
                    // bottom layout margin
                    return 0;
                }
            }

        }
    }
}
