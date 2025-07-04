//выбрасывается, если достигнут предел количества задач

namespace Homeworks__OTUS_.Core.Exceptions
{
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int limit)
            : base($"Максимальное количество задач ограничено {limit}.")
        { }
    }
}

