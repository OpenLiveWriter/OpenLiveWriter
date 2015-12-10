// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal delegate void ApplyDecoratorCallback();
    internal class ImageDecoratorEditorContextImpl : ImageDecoratorEditorContext
    {
        ApplyDecoratorCallback _applyDecoratorCallback;
        IProperties _settings;
        ImagePropertiesInfo _imageProperties;
        IUndoUnitFactory _undoHost;
        public ImageDecoratorEditorContextImpl(IProperties settings, ApplyDecoratorCallback applyDecoratorCallback, ImagePropertiesInfo imageProperties, IUndoUnitFactory undoHost, CommandManager commandManager)
        {
            _applyDecoratorCallback = applyDecoratorCallback;
            _settings = settings;
            _imageProperties = imageProperties;
            _undoHost = undoHost;
            _commandManager = commandManager;
        }
        public void ApplyDecorator()
        {
            _applyDecoratorCallback();
        }

        public IProperties Settings
        {
            get { return _settings; }
        }

        public Size SourceImageSize
        {
            get { return ImageInfo.ImageSourceSize; }
        }

        public Uri SourceImageUri
        {
            get
            {
                return ImageInfo.ImageSourceUri;
            }
        }

        private ImagePropertiesInfo ImageInfo
        {
            get
            {
                return _imageProperties;
            }
        }

        public IHTMLElement ImgElement
        {
            get { return ImageInfo.ImgElement; }
        }

        public float? EnforcedAspectRatio
        {
            get { return _imageProperties.EnforcedAspectRatio; }
        }

        public RotateFlipType ImageRotation
        {
            get
            {
                return ImageInfo.ImageRotation;
            }
            set
            {
                ImageInfo.ImageRotation = value;
            }
        }

        public IImageDecoratorUndoUnit CreateUndoUnit()
        {
            return new ImageDecoratorUndoUnitAdapter(_undoHost.CreateUndoUnit());
        }

        internal class ImageDecoratorUndoUnitAdapter : IUndoUnit, IImageDecoratorUndoUnit
        {
            private IUndoUnit _undo;
            public ImageDecoratorUndoUnitAdapter(IUndoUnit undo)
            {
                _undo = undo;
            }

            public void Commit()
            {
                _undo.Commit();
            }

            public void Dispose()
            {
                _undo.Dispose();
            }
        }

        private CommandManager _commandManager;
        public CommandManager CommandManager
        {
            get
            {
                return _commandManager;
            }
        }
    }
}
