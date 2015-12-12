// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    public enum ContextAvailability
    {
        NotAvailable = 0,
        Available = 1,
        Active = 2,
    }

    public enum FontProperties
    {
        NotAvailable = 0,
        NotSet = 1,
        Set = 2,
    }

    public enum FontPropertiesVerticalPositioning
    {
        NotAvailable = 0,
        NotSet = 1,
        Superscript = 2,
        Subscript = 3,
    }

    public enum FontPropertiesUnderline
    {
        NotAvailable = 0,
        NotSet = 1,
        Set = 2,
    }

    public enum ControlDock
    {
        Top = 1,
        Bottom = 3,
    }

    public enum SwatchColorType
    {
        NoColor = 0,
        Automatic = 1, // automatic swatch
        RGB = 2,       // Solid color swatch
    }

    [Flags]
    public enum CommandInvalidationFlags
    {
        AllCommands = 0,
        State = 0x00000001, // UI_PKEY_Enabled
        Value = 0x00000002, // Value property
        Property = 0x00000004, // Any property
        AllProperties = 0x00000008 // All properties
    }

    public enum CollectionChangeType
    {
        Insert = 0,
        Remove = 1,
        Replace = 2,
        Reset = 3,
    }

    public enum CommandExecutionVerb
    {
        Execute = 0,
        Preview = 1,
        CancelPreview = 2
    }

    public enum CommandTypeID
    {
        UI_COMMANDTYPE_UNKNOWN = 0,
        UI_COMMANDTYPE_GROUP = 1,
        UI_COMMANDTYPE_ACTION = 2,
        UI_COMMANDTYPE_ANCHOR = 3,
        UI_COMMANDTYPE_CONTEXT = 4,
        UI_COMMANDTYPE_COLLECTION = 5,
        UI_COMMANDTYPE_COMMANDCOLLECTION = 6,
        UI_COMMANDTYPE_DECIMAL = 7,
        UI_COMMANDTYPE_BOOLEAN = 8,
        UI_COMMANDTYPE_FONT = 9,
        UI_COMMANDTYPE_RECENTITEMS = 10,
        UI_COMMANDTYPE_COLORANCHOR = 11,
        UI_COMMANDTYPE_COLORCOLLECTION = 12,
    }

    public enum ViewVerb
    {
        Create,
        Destroy,
        Size,
        Error,
    }

    public enum ImageCreationOptions
    {
        Transfer, // IUIImage now owns HBITMAP; caller must NOT free it.
        Copy, // IUIImage creates a copy of HBITMAP; caller must free it.
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("c205bb48-5b1c-4219-a106-15bd0a5f24e2")
    ]
    public interface IUISimplePropertySet
    {
        // Retrieves the stored value of a given property
        [PreserveSig]
        Int32 GetValue([In] ref PropertyKey key, [Out] out PropVariant value);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("803982ab-370a-4f7e-a9e7-8784036a6e26")
    ]
    public interface IUIRibbon
    {
        // Returns the Ribbon height
        [PreserveSig]
        Int32 GetHeight([Out] out UInt32 cy);

        // Load QAT from a stream
        [PreserveSig]
        Int32 LoadSettingsFromStream([In] System.Runtime.InteropServices.ComTypes.IStream pStream);

        // Save QAT to a stream
        [PreserveSig]
        Int32 SaveSettingsToStream([In] System.Runtime.InteropServices.ComTypes.IStream pStream);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("F4F0385D-6872-43a8-AD09-4C339CB3F5C5")
    ]
    public interface IUIFramework
    {
        // Connects the framework and the application
        [PreserveSig]
        Int32 Initialize(IntPtr frameWnd, IUIApplication application);

        // Releases all framework objects
        [PreserveSig]
        Int32 Destroy();

        // Loads and instantiates the views and commands specified in markup
        [PreserveSig]
        Int32 LoadUI(IntPtr instance, [MarshalAs(UnmanagedType.LPWStr)] string resourceName);

        // Retrieves a pointer to a view object
        [PreserveSig]
        Int32 GetView(UInt32 viewId, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface, IidParameterIndex = 1)] out object ppv);

        // Retrieves the current value of a property
        [PreserveSig]
        Int32 GetUICommandProperty(UInt32 commandId, [In] ref PropertyKey key, [Out] out PropVariant value);

        // Immediately sets the value of a property
        [PreserveSig]
        Int32 SetUICommandProperty(UInt32 commandId, [In] ref PropertyKey key, [In] ref PropVariant value);

        // Asks the framework to retrieve the new value of a property at the next update cycle
        [PreserveSig]
        Int32 InvalidateUICommand(UInt32 commandId, CommandInvalidationFlags flags, IntPtr keyPtr);

        // Flush all the pending UI command updates
        [PreserveSig]
        Int32 FlushPendingInvalidations();

        // Asks the framework to switch to the list of modes specified and adjust visibility of controls accordingly
        [PreserveSig]
        Int32 SetModes(Int32 iModes);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("EEA11F37-7C46-437c-8E55-B52122B29293")
    ]
    public interface IContextualUI
    {
        // Sets the desired anchor point where ContextualUI should be displayed.
        // Typically this is the mouse location at the time of right click.
        // x and y are in virtual screen coordinates
        [PreserveSig]
        Int32 ShowAtLocation(Int32 x, Int32 y);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("DF4F45BF-6F9D-4dd7-9D68-D8F9CD18C4DB")
    ]
    public interface IUICollection
    {
        // Retrieves the count of the collection
        [PreserveSig]
        Int32 GetCount([Out] out UInt32 count);

        // Retrieves an item
        [PreserveSig]
        Int32 GetItem(UInt32 index, [Out, MarshalAs(UnmanagedType.IUnknown)] out object item);

        // Adds an item to the end
        [PreserveSig]
        Int32 Add([In, MarshalAs(UnmanagedType.IUnknown)] object item);

        // Inserts an item
        [PreserveSig]
        Int32 Insert(UInt32 index, [In, MarshalAs(UnmanagedType.IUnknown)] object item);

        // Removes an item at the specified position
        [PreserveSig]
        Int32 RemoveAt(UInt32 index);

        // Replaces an item at the specified position
        [PreserveSig]
        Int32 Replace(UInt32 indexReplaced, [In, MarshalAs(UnmanagedType.IUnknown)] object itemReplaceWith);

        // Clear the collection
        [PreserveSig]
        Int32 Clear();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("6502AE91-A14D-44b5-BBD0-62AACC581D52")
    ]
    public interface IUICollectionChangedEvent
    {
        [PreserveSig]
        Int32 OnChanged(CollectionChangeType action,
                          UInt32 oldIndex,
                          [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object oldItem,
                          UInt32 newIndex,
                          [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object newItem);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("75ae0a2d-dc03-4c9f-8883-069660d0beb6")
    ]
    public interface IUICommandHandler
    {
        // User action callback, with transient execution parameters
        [PreserveSig]
        Int32 Execute(UInt32 commandId, // the command that has been executed
                        CommandExecutionVerb verb, // the mode of execution
                        [In, Optional] PropertyKeyRef key, // the property that has changed
                        [In, Optional] PropVariantRef currentValue, // the new value of the property that has changed
                        [In, Optional] IUISimplePropertySet commandExecutionProperties); // additional data for this execution

        // Informs of the current value of a property, and queries for the new one
        [PreserveSig]
        Int32 UpdateProperty(UInt32 commandId,
                               [In] ref PropertyKey key,
                               [In, Optional] PropVariantRef currentValue,
                               [Out] out PropVariant newValue);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("D428903C-729A-491d-910D-682A08FF2522")
    ]
    public interface IUIApplication
    {
        // A view has changed
        [PreserveSig]
        Int32 OnViewChanged(UInt32 viewId,
                              CommandTypeID typeID,
                              [In, MarshalAs(UnmanagedType.IUnknown)] object view,
                              ViewVerb verb,
                              Int32 uReasonCode);

        // Command creation callback
        [PreserveSig]
        Int32 OnCreateUICommand(UInt32 commandId,
                                  CommandTypeID typeID,
                                  [Out] out IUICommandHandler commandHandler);

        // Command destroy callback
        [PreserveSig]
        Int32 OnDestroyUICommand(UInt32 commandId,
                                   CommandTypeID typeID,
                                   [In, Optional] IUICommandHandler commandHandler);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("23c8c838-4de6-436b-ab01-5554bb7c30dd")
    ]
    public interface IUIImage
    {
        [PreserveSig]
        Int32 GetBitmap([Out] out IntPtr bitmap);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("18aba7f3-4c1c-4ba2-bf6c-f5c3326fa816")
    ]
    public interface IUIImageFromBitmap
    {
        IUIImage CreateImage(IntPtr bitmap, ImageCreationOptions options);
    }

    [
        ComImport,
        Guid("926749fa-2615-4987-8845-c33e65f2b957")
    ]
    public class UIRibbonFrameworkClass
    {
    }

    [
        ComImport,
        Guid("0F7434B6-59B6-4250-999E-D168D6AE4293")]
    public class UIRibbonImageFromBitmapFactory
    {
    }
}
