public class TaskCountLimitException : Exception
{
    public TaskCountLimitException(int limit)
        : base($"Достигнут максимальный лимит задач ({limit}).") { }
}