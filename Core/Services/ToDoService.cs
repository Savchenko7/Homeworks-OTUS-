﻿//реализация интерфейса для работы с задачами.

using Homeworks__OTUS_.Core.Exceptions;

public class ToDoService : IToDoService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken cancellationToken)
    {
        // Ограничение на максимальную длину названия задачи
        if (name.Length > 50)
        {
            throw new TaskLengthLimitException(50);
        }

        // Проверка уникальности задачи по названию
        if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, cancellationToken))
        {
            throw new DuplicateTaskException(name);
        }

        // Ограничение на количество задач
        const int MAX_TASK_COUNT = 10;
        if ((await _toDoRepository.CountActiveAsync(user.UserId, cancellationToken)) >= MAX_TASK_COUNT)
        {
            throw new TaskCountLimitException(MAX_TASK_COUNT);
        }

        var todo = new ToDoItem
        {
            Id = Guid.NewGuid(),
            User = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            State = ToDoItemState.Active
        };
        await _toDoRepository.AddAsync(todo, cancellationToken);
        return todo;
    }

    public async Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken)
    {
        var todo = await _toDoRepository.GetAsync(id, cancellationToken);
        if (todo != null)
        {
            todo.State = ToDoItemState.Completed;
            await _toDoRepository.UpdateAsync(todo, cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _toDoRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, string namePrefix, CancellationToken cancellationToken)
    {
        return await _toDoRepository.FindAsync(userId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), cancellationToken);
    }
}