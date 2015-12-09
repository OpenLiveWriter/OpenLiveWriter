// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public abstract class TemperatureDecorator : IImageDecorator
    {
        private readonly float _temperature;

        protected TemperatureDecorator(float temperature)
        {
            _temperature = temperature;
        }

        public void Decorate(ImageDecoratorContext context)
        {
            context.Image = Warm(context.Image, _temperature);
        }

        protected Bitmap Warm(Bitmap bitmap, float val)
        {
            ColorMatrix cm = new ColorMatrix(new float[][]{
                                                               new float[] {1, 0, 0, 0, 0},
                                                               new float[] {0, 1, 0, 0, 0},
                                                               new float[] {0, 0, 1, 0, 0},
                                                               new float[] {0, 0, 0, 1, 0},
                                                               new float[] {val, 0, -val, 0, 1}
                                                           });
            return ImageHelper.ApplyColorMatrix(bitmap, cm);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }
    }

    public class CoolestDecorator : TemperatureDecorator
    {
        public readonly static string Id = "Coolest";

        public CoolestDecorator()
            : base(-0.2f)
        {
        }
    }

    public class CoolDecorator : TemperatureDecorator
    {
        public readonly static string Id = "Cool";

        public CoolDecorator()
            : base(-0.1f)
        {
        }
    }

    public class WarmDecorator : TemperatureDecorator
    {
        public readonly static string Id = "Warm";

        public WarmDecorator()
            : base(0.1f)
        {
        }
    }

    public class WarmestDecorator : TemperatureDecorator
    {
        public readonly static string Id = "Warmest";

        public WarmestDecorator()
            : base(0.2f)
        {
        }
    }
}
