// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// NOTE: These interfaces were referenced but never implemented in the original codebase.
// They are stub implementations to allow the .NET 10 migration to proceed.
// These should be properly implemented or the referencing code should be removed.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Stub interface for selection management. Never fully implemented.
    /// </summary>
    public interface ISelectionManager
    {
        void SelectObject(ISelectableObject selectableObject);
        void UnselectObject(ISelectableObject selectableObject);
    }

    /// <summary>
    /// Stub interface for selectable objects. Never fully implemented.
    /// </summary>
    public interface ISelectableObject
    {
        bool Selected { get; set; }
    }

    /// <summary>
    /// Interface for controls that provide a command bar definition.
    /// </summary>
    public interface ICommandBarProvider
    {
        CommandBarDefinition CommandBarDefinition { get; }
    }
}
