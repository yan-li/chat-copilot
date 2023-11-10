// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using CopilotChat.WebApi.Utilities;
using Microsoft.Graph;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace CopilotChat.WebApi.Flows.FlowRouter;

/// <summary>
/// In memory implementation
/// TODO: define the interface
/// TODO: concurrency support
/// </summary>
public class FlowRouter
{
    private readonly IKernel _kernel;

    private readonly ISKFunction _getIntentFunction;

    private static class Constants
    {
        public const string QAFlow = "QA";
        public const string InterviewerFlow = "Interviewer";
    }

    private readonly List<(string, string)> _fewShotExamples = new List<(string, string)>()
    {
        ("I want to do an interview", Constants.InterviewerFlow),
        ("Let's start the phone screen", Constants.InterviewerFlow),
        ("What's the longest river in the world", Constants.QAFlow),
    };

    private readonly Dictionary<string, string> _descriptions = new Dictionary<string, string>()
    {
        { Constants.QAFlow, "This flow is used to answer questions" },
        { Constants.InterviewerFlow, "This flow is used to conduct an interview" },
    };

    // <chatId, <sessionId, FlowStatus>>
    private readonly Dictionary<string, Dictionary<string, FlowSession>> _flows = new();

    public FlowRouter(IKernel kernel)
    {
        this._kernel = kernel;
        this._getIntentFunction = this.InitializeIntentFunction();
    }

    public FlowSession? GetInProgressFlowSession(string chatId)
    {
        return this.GetFlows(chatId).FirstOrDefault(flow => !flow.Value.IsCompleted).Value;
    }

    public FlowSession StartFlow(string chatId, Flow flow)
    {
        FlowSession session = new()
        {
            Flow = flow,
            SessionId = Guid.NewGuid().ToString(),
            IsCompleted = false
        };

        var sessions = this.GetFlows(chatId);
        sessions.Add(session.SessionId, session);

        return session;
    }

    public void CompleteFlow(string chatId, string sessionId)
    {
        var sessions = this.GetFlows(chatId);
        var session = sessions[sessionId];

        session.IsCompleted = true;
    }

    public async Task<string> GetFlowIntentAsync(string message, CancellationToken cancellationToken)
    {
        //return await Task.FromResult("QA");

        var delimiter = "``#``";

        List<string> examples = new ();
        foreach (var example in this._fewShotExamples)
        {
            examples.Add($"USER: {delimiter}{example.Item1}{delimiter}\nINTENT: {example.Item2}");
        }

        var context = this._kernel.CreateNewContext();
        context.Variables["input"] = $"{delimiter}{message}{delimiter}";
        context.Variables["delimiter"] = delimiter;
        context.Variables["descriptions"] = JsonSerializer.Serialize(this._descriptions);
        context.Variables["examples"] = string.Join("\n", examples);
        context.Variables["defaultAnswer"] = Constants.QAFlow;

        var result = await this._getIntentFunction.InvokeAsync(context, cancellationToken: cancellationToken).ConfigureAwait(false);

        return result.GetValue<string>()!;
    }

    private Dictionary<string, FlowSession> GetFlows(string chatId)
    {
        if (this._flows.TryGetValue(chatId, out var result))
        {
            return result;
        }

        result = new Dictionary<string, FlowSession>();
        this._flows.Add(chatId, result);
        return result;
    }

    private ISKFunction InitializeIntentFunction()
    {
        string promptConfigString = EmbeddedResource.ReadFile("Flows.FlowRouter.GetIntent.config.json");
        if (string.IsNullOrWhiteSpace(promptConfigString))
        {
            throw new InvalidOperationException("Flows.FlowRouter.GetIntent.config.json Prompt config is empty");
        }

        var promptTemplate = EmbeddedResource.ReadFile("Flows.FlowRouter.GetIntent.skprompt.txt");
        if (string.IsNullOrWhiteSpace(promptTemplate))
        {
            throw new InvalidOperationException("Flows.FlowRouter.GetIntent.skprompt.txt Prompt template is empty");
        }

        var promptConfig = PromptTemplateConfig.FromJson(promptConfigString);
        var template = new PromptTemplate(promptTemplate, promptConfig, this._kernel.PromptTemplateEngine);

        return this._kernel.RegisterSemanticFunction("GetIntent", promptConfig, template);
    }
}
