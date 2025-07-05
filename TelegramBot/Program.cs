
//Program.cs — точка входа в приложение, где создается инфраструктура (репозитории, сервисы, обработчик) и запускается бот
using Otus.ToDoList.ConsoleBot;
using Homeworks__OTUS_;

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
                                                                                      
        void OnProcessingStarted(string message)
        {
            Console.WriteLine($"Началась обработка сообщения '{message}'.");
        }

        void OnProcessingFinished(string message)
        {
            Console.WriteLine($"Закончена обработка сообщения '{message}'.");
        }

        handler.OnHandleUpdateStarted += OnProcessingStarted;
        handler.OnHandleUpdateCompleted += OnProcessingFinished;

        // Создаем токен отмены
        var cts = new CancellationTokenSource();

        // Подписываемся на событие завершения работы приложения
        Console.CancelKeyPress += (_, _) =>
        {
            cts.Cancel();
            handler.OnHandleUpdateStarted -= OnProcessingStarted;
            handler.OnHandleUpdateCompleted -= OnProcessingFinished;
            cts.Dispose();
        };
        // Запускаем обработку сообщений 
        botClient.StartReceiving(handler, cts.Token);                   // Начало приёма сообщений

        Console.WriteLine("Бот запущен...");
        await Task.Delay(-1, cts.Token);                                // Работа продолжается бесконечно

    }
}