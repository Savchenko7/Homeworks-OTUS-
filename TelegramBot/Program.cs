// Подключаем необходимые пространства имен
using Telegram.Bot;               // Основной клиент для взаимодействия с Telegram API
using Telegram.Bot.Polling;       // Для настройки и запуска приема обновлений от Telegram
using Telegram.Bot.Types;         // Типы объектов, используемых в Telegram API
using Telegram.Bot.Types.Enums;   // Перечисления, используемые в работе с Telegram API

class Program
{
    static async Task Main(string[] args)
    {

        // Получаем токен бота из переменных окружения операционной системы
        string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

        // Проверяем наличие токена
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Токен бота не найден.");
            return;
        }
        // Создаем собственный HTTP-клиент с увеличением таймаута
        var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(1) }; // Устанавливаем таймаут в минуту

        // Создаем экземпляр клиента Telegram с указанным токеном и передаем созданный HTTP-клиент в конструктор TelegramBotClient
        var botClient = new TelegramBotClient(token, httpClient);

        //string appBaseDirectory = AppContext.BaseDirectory; // получаем базовую директорию приложения
        //string dataDir = Path.Combine(appBaseDirectory, "data"); // формируем относительную директорию данных

        string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Console.WriteLine($"Базовая директория данных: {dataDir}");

        try
        {
            Directory.CreateDirectory(dataDir);
            Console.WriteLine($"Директория создана: {dataDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании директории {dataDir}: {ex.Message}");
            return;
        }

        // Настраиваем опции приемника обновлений
        var receiverOptions = new ReceiverOptions
        {
            // Уточняем, что принимаем только обновления типа "сообщение"
            AllowedUpdates = new[] { UpdateType.Message },
            // Пропускаем ожидающие обновления, чтобы начать работу с чистого листа
            DropPendingUpdates = true
        };

        // Создаем репозитории
        var todoRepo = new FileToDoRepository(dataDir);       // Репозиторий задач
        var userRepo = new FileUserRepository(dataDir);       // Репозиторий пользователей

        // Передача максимального числа задач и длины названия задачи в конструктор ToDoService
        var toDoService = new ToDoService(todoRepo, 10, 50); // Ограничение: 10 задач на пользователя, название задачи до 50 символов

        // Сервисы для управления задачами и пользователями
        var userService = new UserService(userRepo);           // Работа с пользователями
        var toDoReportService = new ToDoReportService(todoRepo);// Генерация отчетов по задачам

        // Создаем репозиторий контекстов
        var contextRepository = new InMemoryScenarioContextRepository(); // создается экземпляр репозитория контекстов


        // Создаем обработчик обновлений, передавая сервисы и репозитории
        var handler = new UpdateHandler(botClient, userService, toDoService, todoRepo, toDoReportService, contextRepository);

        // Подписываемся на события начала и завершения обработки обновлений
        handler.OnHandleUpdateStarted += msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
        handler.OnHandleUpdateCompleted += msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");

        try
        {
            // Регистрируем доступные команды для нашего бота
            await botClient.SetMyCommands(new BotCommand[]
            {
                new BotCommand("/start", "Начало работы"),
                new BotCommand("/help", "Справка по командам"),
                new BotCommand("/info", "Информация о сервисе"),
                new BotCommand("/addtask", "Добавить задачу"),
                new BotCommand("/showtasks", "Просмотреть активные задачи"),
                new BotCommand("/showalltasks", "Просмотреть все задачи"),
                new BotCommand("/removetask", "Удалить задачу"),
                new BotCommand("/completetask", "Завершить задачу"),
                new BotCommand("/report", "Получить отчет по задачам"),
                new BotCommand("/find", "Найти задачу по названию")
            });

            // Используем объект для отмены операции, если потребуется остановка
            using CancellationTokenSource cts = new();

            // Начинаем получать обновления от Telegram сервера
            botClient.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, receiverOptions, cts.Token);

            // Запрашиваем информацию о нашем боте
            var me = await botClient.GetMe();
            Console.WriteLine($"{me.Username} запущен"); // Сообщаем, что бот успешно запустился
            Console.WriteLine("Нажмите A для выхода."); // Инструкция для остановки бота

            // Бесконечный цикл ожидания ввода буквы 'A' для выхода
            while (true)
            {
                var key = Console.ReadKey(true); // Читаем символ клавиатуры без отображения
                if (key.KeyChar == 'a')          // Если нажата клавиша 'A'
                {
                    cts.Cancel();                 // Останавливаем получение обновлений
                    break;                        // Выходим из цикла
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Главная ошибка программы: {ex.Message}");
        }
        finally
        {
            // Отменяем регистрацию событий после завершения подписки
            handler.OnHandleUpdateStarted -= msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
            handler.OnHandleUpdateCompleted -= msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");
        }
    }
}