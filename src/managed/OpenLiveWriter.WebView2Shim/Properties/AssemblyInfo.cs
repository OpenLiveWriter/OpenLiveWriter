// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.CompilerServices;

// Expose internals (e.g. WebView2SourceEditorControl.FormatHtmlForDisplay) to the test project.
// Note: a source-file attribute is used because GenerateAssemblyInfo is false repo-wide,
// which makes the MSBuild AssemblyAttribute item a no-op.
[assembly: InternalsVisibleTo("OpenLiveWriter.Tests")]
