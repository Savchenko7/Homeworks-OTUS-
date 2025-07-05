

//интерфейс для работы с пользователями (регистрация, поиск).

namespace Homeworks__OTUS_
{
    public interface IUserService
    {
        Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
        Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUsername, CancellationToken cancellationToken);
    }
}