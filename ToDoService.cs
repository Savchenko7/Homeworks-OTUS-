using System.Collections.Generic;
using System.Linq;

public class ToDoService : IToDoService
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

    public ToDoItem Add(ToDoUser user, string name)
    {
        var todo = new ToDoItem
        {
            Id = Guid.NewGuid(),
            User = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            State = ToDoItemState.Active
        };
        _todos.Add(todo);
        return todo;
    }

    public void MarkCompleted(Guid id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.State = ToDoItemState.Completed;
            todo.StateChangedAt = DateTime.UtcNow;
        }
    }

    public void Delete(Guid id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
    }
}