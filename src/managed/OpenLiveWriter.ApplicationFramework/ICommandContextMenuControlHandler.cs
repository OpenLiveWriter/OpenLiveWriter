// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface implemented to create and respond to user execute request
    /// for context menu controls
    /// </summary>
    public interface ICommandContextMenuControlHandler
    {
        Control CreateControl();
        string CaptionText { get; }
        string ButtonText { get; }
        object GetUserInput();
        void Execute(object userInput);
    }
}
