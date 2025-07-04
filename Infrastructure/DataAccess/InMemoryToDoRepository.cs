
//реализация интерфейса IToDoRepository для работы с задачами
public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _todos = new List<ToDoItem>();

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _todos.Where(t => t.User.UserId == userId).ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _todos.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public ToDoItem? Get(Guid id)
    {
        return _todos.FirstOrDefault(t => t.Id == id);
    }

    public void Add(ToDoItem item)
    {
        _todos.Add(item);
    }

    public void Update(ToDoItem item)
    {
        var existingItem = _todos.FirstOrDefault(t => t.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Name = item.Name;
            existingItem.State = item.State;
            existingItem.StateChangedAt = item.StateChangedAt;
        }
    }

    public void Delete(Guid id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
    }

    public bool ExistsByName(Guid userId, string name)
    {
        return _todos.Any(t => t.User.UserId == userId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public int CountActive(Guid userId)
    {
        return _todos.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _todos.Where(t => t.User.UserId == userId && predicate(t)).ToList();
    }
}