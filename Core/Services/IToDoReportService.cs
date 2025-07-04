//интерфейс для генерации отчётов по задачам.
public interface IToDoReportService
{
    (int Total, int Completed, int Active, DateTime GeneratedAt) GetUserStats(Guid userId);
    IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate);
}