
//интерфейс для работы с пользователями (создание, чтение и поиск пользователей).
public interface IUserRepository
{
    ToDoUser? GetUser(Guid userId);
    ToDoUser? GetUserByTelegramUserId(long telegramUserId);
    void Add(ToDoUser user);
}
