public class ToDoService : IToDoService
{
    private readonly IToDoRepository _toDoRepository;
    private readonly int _maxTasksPerUser;
    private readonly int _maxTaskNameLength;

    public ToDoService(IToDoRepository toDoRepository, int maxTasksPerUser, int maxTaskNameLength)
    {
        _toDoRepository = toDoRepository;
        _maxTasksPerUser = maxTasksPerUser;
        _maxTaskNameLength = maxTaskNameLength;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _toDoRepository.GetAllByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ToDoItem> AddAsync(ToDoUser user, string name, DateTime deadline, CancellationToken cancellationToken)
    {
        if (name.Length > _maxTaskNameLength)
            throw new TaskLengthLimitException(_maxTaskNameLength);

        if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, cancellationToken))
            throw new DuplicateTaskException(name);

        if ((await _toDoRepository.CountActiveAsync(user.UserId, cancellationToken)) >= _maxTasksPerUser)
            throw new TaskCountLimitException(_maxTasksPerUser);

        var todo = new ToDoItem
        {
            Id = Guid.NewGuid(),
            ToDoUser = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            State = ToDoItemState.Active,
            Deadline = deadline // Задание срока выполнения
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