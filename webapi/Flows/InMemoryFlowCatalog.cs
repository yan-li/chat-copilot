// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace CopilotChat.WebApi.Flows;

internal class InMemoryFlowCatalog : IFlowCatalog
{
    private readonly Dictionary<string, Flow> _flows = new Dictionary<string, Flow>();

    internal InMemoryFlowCatalog()
    {
    }

    internal InMemoryFlowCatalog(IReadOnlyList<Flow> flows)
    {
        // phase 1: register original flows
        foreach (var flow in flows)
        {
            this._flows.Add(flow.Name, flow);
        }

        // phase 2: build references
        foreach (var flow in flows)
        {
            flow.BuildReferenceAsync(this).Wait();
        }
    }

    public Task<IEnumerable<Flow>> GetFlowsAsync()
    {
        return Task.FromResult(this._flows.Select(_ => _.Value));
    }

    public Task<Flow?> GetFlowAsync(string flowName)
    {
        return Task.FromResult(this._flows.TryGetValue(flowName, out var flow) ? flow : null);
    }

    public Task<bool> RegisterFlowAsync(Flow flow)
    {
        this._flows.Add(flow.Name, flow);

        return Task.FromResult(true);
    }
}
