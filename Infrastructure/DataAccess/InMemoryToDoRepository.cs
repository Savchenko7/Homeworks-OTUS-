
//реализация интерфейса IToDoRepository для работы с задачами ]

public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _todos = new List<ToDoItem>();

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(t => t.User.UserId == userId).ToList());
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList());
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.FirstOrDefault(t => t.Id == id));
        }

        public async Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            await Task.Run(() => _todos.Add(item), cancellationToken);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            var existingItem = _todos.FirstOrDefault(t => t.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.State = item.State;
                existingItem.StateChangedAt = item.StateChangedAt;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                await Task.Run(() => _todos.Remove(item), cancellationToken);
            }
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Any(t => t.User.UserId == userId && t.Name == name));
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active));
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(predicate).ToList());
        }
    }