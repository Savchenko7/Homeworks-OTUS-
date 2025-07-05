

//реализация интерфейса для генерации отчётов
public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<(int Total, int Completed, int Active, DateTime GeneratedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var todos = await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
        return (
            Total: todos.Count,
            Completed: todos.Count(t => t.State == ToDoItemState.Completed),
            Active: todos.Count(t => t.State == ToDoItemState.Active),
            GeneratedAt: DateTime.UtcNow
        );
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
    {
        return await _toDoRepository.FindAsync(userId, predicate, cancellationToken);
    }
}