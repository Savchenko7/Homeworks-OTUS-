

//интерфейс для работы с пользователями (регистрация, поиск).

namespace Homeworks__OTUS_
{
    public interface IUserService
    {
        ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
        ToDoUser? GetUser(long telegramUserId);
    }
}