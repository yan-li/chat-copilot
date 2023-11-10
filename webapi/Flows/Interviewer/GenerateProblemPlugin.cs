// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Orchestration;

namespace CopilotChat.WebApi.Flows.Interviewer;
public sealed class GenerateProblemPlugin
{
    private const string Goal = "Generate coding problem prompt of finding maximum subarray in a list";
    private const string SystemPrompt =
        "I am a question generating bot. I will describe the coding problem " +
        "of finding maximum subarray in a list for the user to solve.";

    private readonly IChatCompletion _chat;

    private int MaxTokens { get; set; } = 256;

    private readonly AIRequestSettings _chatRequestSettings;

    public GenerateProblemPlugin(IKernel kernel)
    {
        this._chat = kernel.GetService<IChatCompletion>();
        this._chatRequestSettings = new OpenAIRequestSettings
        {
            MaxTokens = this.MaxTokens,
            StopSequences = new List<string>() { "Observation:" },
            Temperature = 0
        };
    }

    [SKFunction]
    [Description("This function is used to generate a coding problem")]
    [SKName("GenerateProblem")]
    public async Task<string> GenerateProblemAsync(
        [SKName("problem_statement")][Description("The coding problem prompt")] string problem,
        SKContext context)
    {
        var chat = this._chat.CreateNewChat(SystemPrompt);
        chat.AddUserMessage(Goal);

        ChatHistory? chatHistory = context.GetChatHistory();
        if (chatHistory?.Any() ?? false)
        {
            chat.Messages.AddRange(chatHistory);
        }

        if (!string.IsNullOrEmpty(problem))
        {
            context.Variables["problem_statement"] = problem;
            context.PromptInput();

            return "Hello! Thanks for joining the coding interview. " +
               "Here's the problem for you to solve: \n" + problem;
        }

        return "Hello! Thanks for joining the coding interview. " +
               "Here's the problem for you to solve: \n" +
               await this._chat.GenerateMessageAsync(chat, this._chatRequestSettings).ConfigureAwait(false);
    }
}
