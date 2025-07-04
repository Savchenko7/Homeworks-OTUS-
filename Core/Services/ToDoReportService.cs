

//реализация интерфейса для генерации отчётов
public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }
    // Реализуем метод Find
    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _toDoRepository.Find(userId, predicate);
    }

    public (int Total, int Completed, int Active, DateTime GeneratedAt) GetUserStats(Guid userId)
    {
        var todos = _toDoRepository.GetAllByUserId(userId);
        return (
            Total: todos.Count,
            Completed: todos.Count(t => t.State == ToDoItemState.Completed),
            Active: todos.Count(t => t.State == ToDoItemState.Active),
            GeneratedAt: DateTime.UtcNow
        );
    }
}