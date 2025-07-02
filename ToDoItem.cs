public enum ToDoItemState
{
    Active,
    Completed
}

public class ToDoItem
{
    public Guid Id { get; set; }
    public ToDoUser User { get; set; } = new ToDoUser(); // Устанавливаем начальное значение
    public string Name { get; set; } = ""; // Устанавливаем пустую строку
    public DateTime CreatedAt { get; set; }
    public ToDoItemState State { get; set; }
    public DateTime? StateChangedAt { get; set; }
}