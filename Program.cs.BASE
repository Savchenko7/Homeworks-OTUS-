using Otus.ToDoList.ConsoleBot;

class Program
{
    static async Task Main()
    {
        var botClient = new ConsoleBotClient();              // Клиент для общения с ботом
        var userService = new UserService();                 // Сервис для работы с пользователями
        var toDoService = new ToDoService();                // Сервис для работы с задачами
        var handler = new UpdateHandler(botClient, userService, toDoService); // Обработчик обновлений

        botClient.StartReceiving(handler);                   // Начало приёма сообщений

        Console.WriteLine("Бот запущен...");
        await Task.Delay(-1);                                // Работа продолжается бесконечно
    }
}