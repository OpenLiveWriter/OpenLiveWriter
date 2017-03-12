// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    public enum ImageDecoratorInvocationSource { Unknown, InitialInsert, Resize, Reset, ImagePropertiesEditor, Command, TiltPreview };

    public enum ImageEmbedType { Embedded, Linked };

    /// <summary>
    /// Provides runtime context information and callbacks related to the image that a decorator is modifying.
    /// </summary>
    public interface ImageDecoratorContext
    {
        /// <summary>
        /// The raw image that is being decorated.
        /// </summary>
        Bitmap Image { get; set; }

        /// <summary>
        /// The size of the border margins applied to the image.
        /// </summary>
        ImageBorderMargin BorderMargin { get; set; }

        /// <summary>
        /// The settings for the decorator.
        /// </summary>
        IProperties Settings { get; }

        /// <summary>
        /// The options for the current blog client
        /// </summary>
        IEditorOptions EditorOptions { get; }

        /// <summary>
        /// Returns the manner in which the current image is embedded into the post document.
        /// </summary>
        ImageEmbedType ImageEmbedType { get; }

        /// <summary>
        /// The img HTML element associated with the image being decorated.
        /// </summary>
        IHTMLElement ImgElement { get; }

        /// <summary>
        /// Returns a hint about the reason the image decoration was triggered.
        /// </summary>
        ImageDecoratorInvocationSource InvocationSource { get; }

        /// <summary>
        /// The rotation of the image with respect to the original source image.
        /// </summary>
        RotateFlipType ImageRotation { get; }

        /// <summary>
        /// The URI of the original source image.
        /// </summary>
        Uri SourceImageUri { get; }

        float? EnforcedAspectRatio { get; }
    }

    /// <summary>
    /// An ImageBorderMargin serves two purposes. It notes the current
    /// amount of margin that has actually been applied to an image, and
    /// it also keeps track of the calculations that were used to come
    /// up with that amount of margin. The calculations are necessary to
    /// determine how much border margin would be applied to the same image
    /// at other sizes.
    /// </summary>
    public class ImageBorderMargin
    {
        private readonly int _width;
        private readonly int _height;

        private readonly List<BorderCalculation> _calculations;

        public ImageBorderMargin(int width, int height, BorderCalculation calculation)
        {
            _width = width;
            _height = height;
            _calculations = new List<BorderCalculation>();
            _calculations.Add(calculation);
        }

        public ImageBorderMargin(ImageBorderMargin existingBorder, int width, int height, BorderCalculation calculation)
        {
            _width = width + existingBorder.Width;
            _height = height + existingBorder.Height;
            _calculations = new List<BorderCalculation>(existingBorder._calculations);
            _calculations.Add(calculation);
        }

        #region Property names
        private const string WIDTH = "Width";
        private const string HEIGHT = "Height";
        private const string COUNT = "CalcCount";
        private const string CALC = "Calc";
        private const string WIDTH_ADD = "WidthAdd";
        private const string HEIGHT_ADD = "HeightAdd";
        private const string WIDTH_FACTOR = "WidthFactor";
        private const string HEIGHT_FACTOR = "HeightFactor";
        #endregion

        public ImageBorderMargin(IProperties properties)
        {
            _width = properties.GetInt(WIDTH, 0);
            _height = properties.GetInt(HEIGHT, 0);
            int calcCount = properties.GetInt(COUNT, 0);
            _calculations = new List<BorderCalculation>(calcCount);
            for (int i = 0; i < calcCount; i++)
            {
                string prefix = CALC + i.ToString(CultureInfo.InvariantCulture);
                _calculations.Add(new BorderCalculation(
                    properties.GetInt(prefix + WIDTH_ADD, 0),
                    properties.GetInt(prefix + HEIGHT_ADD, 0),
                    properties.GetFloat(prefix + WIDTH_FACTOR, 1f),
                    properties.GetFloat(prefix + HEIGHT_FACTOR, 1f)));
            }
        }

        public void Save(IProperties properties)
        {
            properties.RemoveAll();
            properties.SetInt(WIDTH, _width);
            properties.SetInt(HEIGHT, _height);
            properties.SetInt(COUNT, _calculations.Count);
            for (int i = 0; i < _calculations.Count; i++)
            {
                BorderCalculation calc = _calculations[i];
                string prefix = CALC + i.ToString(CultureInfo.InvariantCulture);
                properties.SetInt(prefix + WIDTH_ADD, calc.WidthAdd);
                properties.SetInt(prefix + HEIGHT_ADD, calc.HeightAdd);
                properties.SetFloat(prefix + WIDTH_FACTOR, calc.WidthFactor);
                properties.SetFloat(prefix + HEIGHT_FACTOR, calc.HeightFactor);
            }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Create a blank ImageBorderMargin.
        /// </summary>
        public static ImageBorderMargin Empty
        {
            get { return new ImageBorderMargin(0, 0, new BorderCalculation(0, 0, 1f, 1f)); }
        }

        public Size CalculateImageSize(Size imageSize)
        {
            foreach (BorderCalculation calc in _calculations)
                imageSize = calc.ForwardCalculation(imageSize);
            return imageSize;
        }

        public Size ReverseCalculateImageSize(Size imageSize)
        {
            for (int i = _calculations.Count - 1; i >= 0; i--)
            {
                imageSize = _calculations[i].ReverseCalculation(imageSize);
            }
            return imageSize;
        }
    }
    public class BorderCalculation
    {
        private readonly int _widthAdd = 0;
        private readonly int _heightAdd = 0;
        private readonly float _widthFactor = 1f;
        private readonly float _heightFactor = 1f;

        public BorderCalculation(int widthAdd, int heightAdd, float widthFactor, float heightFactor)
        {
            _widthAdd = widthAdd;
            _heightAdd = heightAdd;
            _widthFactor = widthFactor;
            _heightFactor = heightFactor;
        }

        public BorderCalculation(int widthAdd, int heightAdd)
        {
            _widthAdd = widthAdd;
            _heightAdd = heightAdd;
        }

        public BorderCalculation(float widthFactor, float heightFactor)
        {
            _widthFactor = widthFactor;
            _heightFactor = heightFactor;
        }

        internal int WidthAdd
        {
            get { return _widthAdd; }
        }

        internal int HeightAdd
        {
            get { return _heightAdd; }
        }

        internal float WidthFactor
        {
            get { return _widthFactor; }
        }

        internal float HeightFactor
        {
            get { return _heightFactor; }
        }

        public Size ForwardCalculation(Size size)
        {
            size.Width += _widthAdd;
            size.Width = (int)(Math.Round(size.Width * _widthFactor));

            size.Height += _heightAdd;
            size.Height = (int)(Math.Round(size.Height * _heightFactor));

            return size;
        }

        public Size ReverseCalculation(Size size)
        {
            size.Width = (int)(Math.Round(size.Width / _widthFactor));
            size.Width -= _widthAdd;

            size.Height = (int)(Math.Round(size.Height / _heightFactor));
            size.Height -= _heightAdd;

            return size;
        }
    }
}

