﻿// Infrastructure/DataAccess/InMemoryUserRepository.cs
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new List<ToDoUser>();

    public ToDoUser? GetUser(Guid userId)
    {
        return _users.FirstOrDefault(u => u.UserId == userId);
    }

    public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
    {
        return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }

    public void Add(ToDoUser user)
    {
        _users.Add(user);
    }
}