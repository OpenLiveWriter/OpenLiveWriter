// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// Fails if the given assembly was compiled as a Debug build.
//
// The Velopack package is what users install, so the release pipeline must never
// pack Debug binaries. The configuration is threaded through several layers
// (workflow env -> msbuild -> the installer post-build script) and a Debug build
// packaged by mistake is not obvious from the artifacts, so assert it straight
// from the assembly metadata instead of trusting the plumbing.
//
// The C# compiler emits [Debuggable] on every assembly. Debug builds set
// DebuggingModes.DisableOptimizations (0x100); Release builds do not.
//
// Usage: dotnet run scripts/AssertReleaseBuild.cs -- <path-to-assembly>

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

if (args.Length != 1)
{
    Console.Error.WriteLine("usage: AssertReleaseBuild.cs <path-to-assembly>");
    return 2;
}

var path = args[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"assert-release-build: '{path}' does not exist - the build produced no assembly to check.");
    return 1;
}

var full = Path.GetFullPath(path);
bool sawDebuggable = false;
bool optimizationsDisabled = false;

using (var stream = File.OpenRead(full))
using (var pe = new PEReader(stream))
{
    var reader = pe.GetMetadataReader();
    foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
    {
        var attribute = reader.GetCustomAttribute(handle);
        if (attribute.Constructor.Kind != HandleKind.MemberReference)
            continue;

        var memberRef = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
        if (memberRef.Parent.Kind != HandleKind.TypeReference)
            continue;

        var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
        if (reader.GetString(typeRef.Name) != "DebuggableAttribute")
            continue;

        sawDebuggable = true;

        // Blob layout: 2-byte prolog, then the fixed constructor arguments.
        var blob = reader.GetBlobBytes(attribute.Value);
        if (blob.Length >= 6)
        {
            // DebuggableAttribute(DebuggingModes)
            optimizationsDisabled = (BitConverter.ToInt32(blob, 2) & 0x100) != 0;
        }
        else if (blob.Length >= 4)
        {
            // Legacy DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled)
            optimizationsDisabled = blob[3] != 0;
        }
    }
}

if (!sawDebuggable)
{
    Console.WriteLine($"assert-release-build: no DebuggableAttribute on {full}; treating as optimized.");
    return 0;
}

if (optimizationsDisabled)
{
    Console.Error.WriteLine($"assert-release-build: '{full}' is a Debug build (DebuggableAttribute disables optimizations). Refusing to package it.");
    return 1;
}

Console.WriteLine($"assert-release-build: {full} is a Release build.");
return 0;
