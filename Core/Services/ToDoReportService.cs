public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<(int Total, int Completed, int Active, DateTime GeneratedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var allItems = await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);

        return (
            Total: allItems.Count,                             // Всего задач (без удалённых)
            Completed: allItems.Count(t => t.State == ToDoItemState.Completed), // Количество завершенных задач
            Active: allItems.Count(t => t.State == ToDoItemState.Active),      // Количество активных задач
            GeneratedAt: DateTime.UtcNow                      // Дата генерации отчета
        );
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
    {
        return await _toDoRepository.FindAsync(userId, predicate, cancellationToken);
    }
}
