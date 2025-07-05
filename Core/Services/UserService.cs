
//реализация интерфейса для работы с пользователями.

using Homeworks__OTUS_;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _repository.GetUserAsync(telegramUserId, cancellationToken);
    }

    public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUsername, CancellationToken cancellationToken)
    {
        var user = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUsername,
            RegisteredAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user, cancellationToken);
        return user; // Возврат объекта
    }
}