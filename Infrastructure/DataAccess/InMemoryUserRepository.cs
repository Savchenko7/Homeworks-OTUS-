
//реализация интерфейса IUserRepository для работы с пользователями
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new List<ToDoUser>();

    public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_users.FirstOrDefault(u => u.TelegramUserId == telegramUserId));
    }

    public async Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
    {
        await Task.Run(() => _users.Add(user), cancellationToken);
    }
}