public class UserService : IUserService
{
    private readonly Dictionary<long, ToDoUser> _users = new Dictionary<long, ToDoUser>();

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        if (!_users.ContainsKey(telegramUserId))
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.UtcNow
            };
            _users.Add(telegramUserId, user);
        }
        return _users[telegramUserId];
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return _users.ContainsKey(telegramUserId) ? _users[telegramUserId] : null;
    }
}