//интерфейс для генерации отчётов по задачам.

public interface IToDoReportService
    {
        Task<(int Total, int Completed, int Active, DateTime GeneratedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
    }