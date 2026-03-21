namespace RegionHR.Analytics.Domain;

/// <summary>
/// Beräknar nätverksanalys (ONA) från enkätsvar.
/// Bygger adjacensmatris, beräknar grad-centralitet och betweenness.
/// </summary>
public static class ONACalculationService
{
    public record ONAResult(List<NetworkNode> Nodes, List<NetworkEdge> Edges);

    /// <summary>
    /// Beräkna nätverksanalys från ONA-svar.
    /// </summary>
    public static ONAResult Berakna(Guid surveyId, IReadOnlyList<ONAResponse> responses)
    {
        if (responses.Count == 0)
            return new ONAResult([], []);

        // Build edges with aggregated strength per pair
        var edgeMap = new Dictionary<(Guid from, Guid to, int frageIndex), decimal>();
        foreach (var r in responses)
        {
            var key = (r.RespondentId, r.NomineradId, r.FrageIndex);
            edgeMap[key] = r.Varde;
        }

        var edges = edgeMap.Select(kvp =>
            NetworkEdge.Skapa(surveyId, kvp.Key.from, kvp.Key.to, kvp.Key.frageIndex, kvp.Value)
        ).ToList();

        // Collect all unique participants
        var allIds = responses.Select(r => r.RespondentId)
            .Union(responses.Select(r => r.NomineradId))
            .Distinct()
            .ToList();

        // Calculate in-degree and out-degree (unique connections)
        var outDegrees = new Dictionary<Guid, HashSet<Guid>>();
        var inDegrees = new Dictionary<Guid, HashSet<Guid>>();

        foreach (var id in allIds)
        {
            outDegrees[id] = new HashSet<Guid>();
            inDegrees[id] = new HashSet<Guid>();
        }

        foreach (var r in responses)
        {
            outDegrees[r.RespondentId].Add(r.NomineradId);
            inDegrees[r.NomineradId].Add(r.RespondentId);
        }

        // Simplified betweenness centrality (BFS shortest paths)
        var betweenness = BeraknaBetwenness(allIds, responses);

        // Classify roles
        var nodes = allIds.Select(id =>
        {
            var inDeg = inDegrees[id].Count;
            var outDeg = outDegrees[id].Count;
            var bc = betweenness.GetValueOrDefault(id, 0m);
            var roll = KlassificeraRoll(inDeg, outDeg, bc, allIds.Count);

            return NetworkNode.Skapa(surveyId, id, inDeg, outDeg, bc, null, roll);
        }).ToList();

        return new ONAResult(nodes, edges);
    }

    /// <summary>
    /// Simplified betweenness centrality via BFS.
    /// Counts for each node how many shortest paths between other pairs pass through it.
    /// </summary>
    private static Dictionary<Guid, decimal> BeraknaBetwenness(
        List<Guid> nodeIds,
        IReadOnlyList<ONAResponse> responses)
    {
        var betweenness = nodeIds.ToDictionary(id => id, _ => 0m);

        // Build adjacency list (directed)
        var adj = new Dictionary<Guid, HashSet<Guid>>();
        foreach (var id in nodeIds)
            adj[id] = new HashSet<Guid>();
        foreach (var r in responses)
            adj[r.RespondentId].Add(r.NomineradId);

        // For each source, do BFS and count paths
        foreach (var source in nodeIds)
        {
            var dist = new Dictionary<Guid, int>();
            var pathCount = new Dictionary<Guid, decimal>();
            var predecessors = new Dictionary<Guid, List<Guid>>();
            var stack = new Stack<Guid>();

            foreach (var id in nodeIds)
            {
                dist[id] = -1;
                pathCount[id] = 0;
                predecessors[id] = new List<Guid>();
            }

            dist[source] = 0;
            pathCount[source] = 1;
            var queue = new Queue<Guid>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                stack.Push(v);

                foreach (var w in adj[v])
                {
                    if (dist[w] < 0)
                    {
                        dist[w] = dist[v] + 1;
                        queue.Enqueue(w);
                    }
                    if (dist[w] == dist[v] + 1)
                    {
                        pathCount[w] += pathCount[v];
                        predecessors[w].Add(v);
                    }
                }
            }

            // Back-propagation of dependencies
            var dependency = nodeIds.ToDictionary(id => id, _ => 0m);

            while (stack.Count > 0)
            {
                var w = stack.Pop();
                foreach (var v in predecessors[w])
                {
                    var fraction = pathCount[w] > 0 ? (pathCount[v] / pathCount[w]) * (1 + dependency[w]) : 0;
                    dependency[v] += fraction;
                }
                if (w != source)
                {
                    betweenness[w] += dependency[w];
                }
            }
        }

        // Normalize
        var n = nodeIds.Count;
        if (n > 2)
        {
            var scale = 1.0m / ((n - 1) * (n - 2));
            foreach (var id in nodeIds)
                betweenness[id] = Math.Round(betweenness[id] * scale, 4);
        }

        return betweenness;
    }

    /// <summary>
    /// Classify network role based on degree distribution and betweenness centrality.
    /// </summary>
    private static string KlassificeraRoll(int inDegree, int outDegree, decimal betweenness, int totalNodes)
    {
        var totalDegree = inDegree + outDegree;

        // Isolated: very few connections
        if (totalDegree <= 1)
            return "Isolated";

        // Bottleneck: high betweenness but moderate degree
        if (betweenness > 0.15m && totalDegree < totalNodes / 2)
            return "Bottleneck";

        // Boundary Spanner: high betweenness and moderate-high degree
        if (betweenness > 0.1m)
            return "BoundarySpanner";

        // Influencer: high in-degree (many people seek them out)
        if (inDegree >= totalNodes / 3 && inDegree > outDegree)
            return "Influencer";

        // Value Creator: high out-degree (reaches out to many)
        return "ValueCreator";
    }
}
