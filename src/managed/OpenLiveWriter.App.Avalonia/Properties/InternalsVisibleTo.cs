// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.CompilerServices;

// Exposes internal test seams (link-building/escaping, source formatter, link
// dialog validation) to the automated editor test suite. Declared in source
// because this project sets GenerateAssemblyInfo=false, which disables the
// MSBuild <InternalsVisibleTo> item.
[assembly: InternalsVisibleTo("OpenLiveWriter.EditorTests.Automated")]
