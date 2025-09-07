
public interface IUserRepository
{
    Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
    Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
}
