// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.ContentSources
{
    // Allows us to pass IInternalSmartContentContext through functions that accept ISmartContent.
    public class InternalSmartContent : SmartContent
    {
        private IInternalSmartContentContext _smartContentContext;
        public InternalSmartContent(IExtensionData extensionData, IInternalSmartContentContextSource smartContentContextSource, string contentSourceId)
            : base(extensionData)
        {
            _smartContentContext = new InternalSmartContentContext(smartContentContextSource, contentSourceId);
        }

        public IInternalSmartContentContext InternalSmartContentContext { get { return _smartContentContext; } }
    }
}
