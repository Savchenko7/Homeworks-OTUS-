public interface IToDoReportService
{
    Task<(int Total, int Completed, int Active, DateTime GeneratedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken);
    
}