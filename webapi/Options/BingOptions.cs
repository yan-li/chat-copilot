// Copyright (c) Microsoft. All rights reserved.

using DocumentFormat.OpenXml.Wordprocessing;

namespace CopilotChat.WebApi.Options;

/// <summary>
/// Configuration settings for bing.
/// </summary>
public class BingOptions
{
    public const string PropertyName = "Bing";

    /// <summary>
    /// Key to access the content safety service.
    /// </summary>
    [RequiredOnPropertyValue(nameof(Enabled), true)]
    public string ApiKey { get; set; } = string.Empty;
}
