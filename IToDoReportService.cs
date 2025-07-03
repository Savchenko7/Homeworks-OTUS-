// Core/Services/IToDoReportService.cs
public interface IToDoReportService
{
    (int Total, int Completed, int Active, DateTime GeneratedAt) GetUserStats(Guid userId);
}