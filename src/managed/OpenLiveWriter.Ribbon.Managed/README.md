# OpenLiveWriter.Ribbon.Managed

A fully managed C# WinForms ribbon implementation replacing the native C++ Windows Ribbon Framework.

## Overview

This project provides a custom WinForms ribbon control built from scratch in C#, eliminating the dependency on the native C++ `OpenLiveWriter.Ribbon.dll`.

## Architecture

### Core Components

- **`ManagedRibbonControl`** - Main entry point for integrating the ribbon into the application
- **`RibbonPanel`** - Container control that hosts tabs, groups, and the application menu
- **`RibbonTab`** - Represents a tab in the ribbon
- **`RibbonGroup`** - Groups related commands together within a tab

### Control Types

- **`RibbonButton`** - Standard, toggle, split, and dropdown button variants
- **`RibbonGallery`** - In-ribbon and dropdown gallery controls
- **`RibbonComboBox`** - Dropdown/editable combobox (e.g., font family/size)
- **`RibbonColorPicker`** - Color selection with standard palettes
- **`RibbonSpinner`** - Numeric up/down control

### Additional Features

- **`ApplicationMenu`** - Backstage menu with MRU (Most Recently Used) support
- **`QuickAccessToolbar`** - Customizable quick access buttons
- **`KeytipManager`** - Keyboard navigation with keytip badges

## Configuration

The ribbon structure is defined programmatically using the configuration classes:

```csharp
var config = DefaultRibbonConfiguration.Create();
// Or create custom configuration:
var custom = new RibbonConfiguration();
custom.Tabs.Add(new TabConfig { ... });
```

## Integration

To use the managed ribbon in `PostEditorMainControl`:

```csharp
// Replace InitializeRibbon() with:
private void InitializeManagedRibbon()
{
    var managedRibbon = new ManagedRibbonControl();
    managedRibbon.Initialize(CommandManager); // Pass existing CommandManager
    managedRibbon.BuildDefaultConfiguration();
    
    Controls.Add(managedRibbon);
    managedRibbon.BringToFront();
    
    // Set initial modes
    managedRibbon.SetTextDirection(RightToLeft == RightToLeft.Yes);
    managedRibbon.SetPluginsAvailable(hasPlugins);
}
```

## Application Modes

The ribbon supports different visibility modes:

- `Normal` - Standard editing mode
- `Preview` - Preview mode (shows Preview tab)
- `LTR` / `RTL` - Text direction modes
- `WithPlugins` / `WithoutPlugins` - Plugin gallery visibility
- `Debug` - Debug tab visibility

## Contextual Tabs

Contextual tabs appear based on content selection:

```csharp
// Show image tools when an image is selected
managedRibbon.ShowContextualTabGroup(RibbonContextualTabGroup.ImageTools);

// Hide when deselected
managedRibbon.HideContextualTabGroup(RibbonContextualTabGroup.ImageTools);
```

## Removing the C++ Dependency

After switching to the managed ribbon:

1. Remove `src/unmanaged/OpenLiveWriter.Ribbon/` from the solution
2. Remove the project dependency from `OpenLiveWriter.csproj`:
   ```xml
   <!-- Remove this: -->
   <ProjectDependencies>
     {195A60BF-7A4D-42E6-B5F4-FEBC679E19F0}
   </ProjectDependencies>
   ```
3. Remove the native DLL copy step from build scripts
4. Remove `OpenLiveWriter.Ribbon.dll` from installer manifests

## Customization

### Theming

Colors are managed through `RibbonColors`:

```csharp
// Use built-in dark theme
RibbonColors.Current = RibbonColors.CreateDarkTheme();

// Or customize individual colors
RibbonColors.Current.TabBackground = Color.FromArgb(30, 30, 30);
```

### Adding Commands

Register new commands through the CommandManagerBridge:

```csharp
managedRibbon.CommandBridge.RegisterCommand(CommandId.MyNewCommand);
```

## Known Limitations

- Some advanced gallery features (e.g., item categories) need expansion
- Keyboard navigation is basic compared to native ribbon
- High DPI scaling may need additional testing

## Migration Checklist

- [x] Core ribbon panel with tabs and groups
- [x] Button variants (standard, toggle, split, dropdown)
- [x] Gallery controls
- [x] ComboBox, ColorPicker, Spinner
- [x] Application menu with MRU
- [x] Quick Access Toolbar
- [x] Contextual tabs
- [x] Application modes
- [x] Basic keyboard navigation
- [ ] Full keyboard navigation parity
- [ ] Performance optimization
- [ ] Visual regression testing
