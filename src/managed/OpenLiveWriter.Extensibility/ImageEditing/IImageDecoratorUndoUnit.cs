// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    public interface IImageDecoratorUndoUnit : IDisposable
    {
        void Commit();
    }
}
