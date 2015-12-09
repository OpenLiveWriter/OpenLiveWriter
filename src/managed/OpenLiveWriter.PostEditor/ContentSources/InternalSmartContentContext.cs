// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    /// <summary>
    /// Base implementation of IInternalSmartContentContext
    /// </summary>
    public class InternalSmartContentContext : IInternalSmartContentContext
    {
        private SmartContentEditor _smartContentEditor;
        private IInternalSmartContentContextSource _contextSource;
        public InternalSmartContentContext(IInternalSmartContentContextSource contextSource, string contentSourceId)
        {
            _contextSource = contextSource;
            _smartContentEditor = contextSource.GetSmartContentEditor(contentSourceId);
        }

        public SmartContentEditor SmartContentEditor
        {
            get { return _smartContentEditor; }
        }

        public Size BodySize
        {
            get { return _contextSource.BodySize; }
        }
    }

    /// <summary>
    /// Context used by our internal smart content sources
    /// </summary>
    public interface IInternalSmartContentContext
    {
        Size BodySize { get; }
        SmartContentEditor SmartContentEditor { get; }
    }
}
