
//Program.cs — точка входа в приложение, где создается инфраструктура (репозитории, сервисы, обработчик) и запускается бот

using Homeworks__OTUS_;
using Otus.ToDoList.ConsoleBot;

class Program
{
    static async Task Main()
    {
        var botClient = new ConsoleBotClient();              // Клиент для общения с ботом

        var todoRepo = new InMemoryToDoRepository();        // Создали репозиторий задач
        var userRepo = new InMemoryUserRepository();        // Создали репозиторий
        
        // Передаем репозитории в сервисы

        var userService = new UserService(userRepo);         // Сервис для работы с пользователями
        var toDoService = new ToDoService(todoRepo);         // Сервис для работы с задачами

        var handler = new UpdateHandler(botClient, userService, toDoService, todoRepo); // Обработчик обновлений

        botClient.StartReceiving(handler);                   // Начало приёма сообщений

        Console.WriteLine("Бот запущен...");
        await Task.Delay(-1);                                // Работа продолжается бесконечно
    }
}