
//интерфейс для работы с пользователями (создание, чтение и поиск пользователей).
public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
        Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
    }