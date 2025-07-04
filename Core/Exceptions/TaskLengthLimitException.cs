//выбрасывается, если название задачи слишком длинное.

namespace Homeworks__OTUS_.Core.Exceptions
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int maxLength)
            : base($"Название задачи не может превышать {maxLength} символов.")
        { }
    }
}
