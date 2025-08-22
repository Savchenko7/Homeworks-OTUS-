public enum ToDoItemState
{
    Active,
    Completed
}

public class ToDoItem
{
    public Guid Id { get; set; }
    public ToDoUser ToDoUser { get; set; } // Добавляем свойство ToDoUser
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public ToDoItemState State { get; set; }
    public DateTime? StateChangedAt { get; set; }
}