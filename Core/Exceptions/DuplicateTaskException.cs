public class DuplicateTaskException : Exception
{
    public DuplicateTaskException(string taskName)
        : base($"Задача с названием '{taskName}' уже существует.") { }
}