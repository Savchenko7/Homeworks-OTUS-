public class TaskLengthLimitException : Exception
{
    public TaskLengthLimitException(int maxLength)
        : base($"Длина названия задачи превышает допустимый максимум ({maxLength}) символов.") { }
}