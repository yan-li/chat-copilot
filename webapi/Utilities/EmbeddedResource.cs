﻿// Copyright (c) Microsoft. All rights reserved.

namespace CopilotChat.WebApi.Utilities;

using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

internal static class EmbeddedResource
{
    private static readonly string? @namespace = typeof(EmbeddedResource).Namespace;
    private static readonly Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly;

    internal static string ReadFile(string name, bool useNamespace = true)
    {
        if (assembly == null)
        {
            throw new Exception($"[{@namespace}] {name} assembly not found");
        }

        var resourceName = useNamespace ? $"{@namespace}.{name}" : name;
        using Stream? resource = assembly.GetManifestResourceStream(resourceName);
        if (resource == null)
        {
            throw new Exception($"[{@namespace}] {name} resource not found");
        }

        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    internal static IList<string> ReadFileTypes(string fileType)
    {
        if (assembly == null)
        {
            throw new Exception($"[{@namespace}] assembly not found");
        }

        var resourceNames = assembly.GetManifestResourceNames();
        var resources = resourceNames.Where(x => x.EndsWith(fileType));
        var fileContents = new List<string>();
        foreach (var resourceName in resources)
        {
            var content = ReadFile(resourceName, false);
            fileContents.Add(content);
        }

        return fileContents;
    }
}
