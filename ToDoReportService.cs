// Core/Services/ToDoReportService.cs
public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _repository;

    public ToDoReportService(IToDoRepository repository)
    {
        _repository = repository;
    }

    public (int Total, int Completed, int Active, DateTime GeneratedAt) GetUserStats(Guid userId)
    {
        var todos = _repository.GetAllByUserId(userId);
        return (
            Total: todos.Count,
            Completed: todos.Count(t => t.State == ToDoItemState.Completed),
            Active: todos.Count(t => t.State == ToDoItemState.Active),
            GeneratedAt: DateTime.UtcNow
        );
    }
}