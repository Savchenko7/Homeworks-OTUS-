public enum ToDoItemState
{
    Active,
    Completed
}

public class ToDoItem
{
    public Guid Id { get; set; }
    public ToDoUser ToDoUser { get; set; } 
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public ToDoItemState State { get; set; }
    public DateTime? StateChangedAt { get; set; }
}