// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Orchestration;

namespace CopilotChat.WebApi.Flows.QA;
public sealed class RephrasePlugin
{
    public RephrasePlugin()
    {
    }

    [SKFunction]
    [Description("This function is used to rephrase the answer from bing search")]
    [SKName("RephraseAnswer")]
    public async Task<string> RephraseAnswerAsync(
        [SKName("answer")][Description("The answer")] string answer,
        SKContext context)
    {
        // TODO: rephrase answer with LLM

        context.PromptInput();
        context.Variables["output"] = answer;
        return await Task.FromResult(answer);
    }
}
