
//класс, представляющий пользователя (идентификатор, имя, дата регистрации).

public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; } = ""; // Изначально устанавливаем пустую строку
        public DateTime RegisteredAt { get; set; }
    }