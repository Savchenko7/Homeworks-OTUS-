// Core/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        var existingUser = _repository.GetUserByTelegramUserId(telegramUserId);
        if (existingUser != null)
        {
            return existingUser;
        }

        var newUser = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            RegisteredAt = DateTime.UtcNow
        };
        _repository.Add(newUser);
        return newUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return _repository.GetUserByTelegramUserId(telegramUserId);
    }
}