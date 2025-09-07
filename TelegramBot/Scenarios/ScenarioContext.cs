public class ScenarioContext
{
    public long UserId { get; init; }
    public ScenarioType CurrentScenario { get; set; }
    public string? CurrentStep { get; set; }
    public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

    public ScenarioContext(long userId, ScenarioType currentScenario)
    {
        UserId = userId;
        CurrentScenario = currentScenario;
    }
}