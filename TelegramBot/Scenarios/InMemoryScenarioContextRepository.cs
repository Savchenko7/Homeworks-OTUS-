public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    private readonly Dictionary<long, ScenarioContext> _contexts = new();

    public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        return Task.FromResult(_contexts.TryGetValue(userId, out var ctx) ? ctx : null);
    }

    public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
    {
        _contexts[userId] = context;
        return Task.CompletedTask;
    }

    public Task ResetContext(long userId, CancellationToken ct)
    {
        _contexts.Remove(userId);
        return Task.CompletedTask;
    }
}