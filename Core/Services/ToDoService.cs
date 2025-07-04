//реализация интерфейса для работы с задачами.

using Homeworks__OTUS_.Core.Exceptions;

public class ToDoService : IToDoService
{
    private readonly IToDoRepository _toDoRepository;
   
    public ToDoService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }
    private readonly List<ToDoItem> _todos = new List<ToDoItem>();

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoRepository.GetAllByUserId(userId); // Получаем ВСЕ задачи пользователя
         }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _toDoRepository.GetActiveByUserId(userId); // Получаем АКТИВНЫЕ задачи пользователя
           }

    public ToDoItem Add(ToDoUser user, string name)
    {
        const int MAX_TASKS_PER_USER = 10; // Максимальное количество задач
        const int MAX_NAME_LENGTH = 50; // Максимальная длина названия задачи
        // Проверка на достижение лимита задач
        if (_toDoRepository.CountActive(user.UserId) >= MAX_TASKS_PER_USER)
        {
            throw new TaskCountLimitException(MAX_TASKS_PER_USER);
        }
        // Проверка на длину названия задачи
        if (name.Length > MAX_NAME_LENGTH)
        {
            throw new TaskLengthLimitException(MAX_NAME_LENGTH);
        }
        // Проверяем, есть ли уже задача с таким названием у пользователя
        if (_toDoRepository.ExistsByName(user.UserId, name))
        {
            throw new DuplicateTaskException(name); // Исключение, если задача уже существует
        }
        var todo = new ToDoItem
        {
            Id = Guid.NewGuid(),
            User = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            State = ToDoItemState.Active
        };
        _toDoRepository.Add(todo); // Сохраняем задачу в репозитории
        return todo;
    }

    public void MarkCompleted(Guid id)
    {
        var todo = _toDoRepository.Get(id); 
        if (todo != null)
        {
            todo.State = ToDoItemState.Completed;
            todo.StateChangedAt = DateTime.UtcNow;
            _toDoRepository.Update(todo); // ОБНОВЛЯЕМ задачу в репозитории
        }
    }

    public void Delete(Guid id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
        _toDoRepository.Delete(id); // Удаляем задачу из репозитория
    }
    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        return _toDoRepository.Find(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
    }
}