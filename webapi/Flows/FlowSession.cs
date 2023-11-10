// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Experimental.Orchestration;

namespace CopilotChat.WebApi.Flows;

public class FlowSession
{
    public Flow Flow { get; set; }

    public string SessionId { get; set; }

    public bool IsCompleted { get; set; }
}
