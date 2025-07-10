// Подключаем необходимые пространства имен
using Telegram.Bot;              // Основной клиент для взаимодействия с Telegram API
using Telegram.Bot.Polling;      // Для настройки и запуска приема обновлений от Telegram
using Telegram.Bot.Types;        // Типы объектов, используемых в Telegram API
using Telegram.Bot.Types.Enums;  // Перечисления, используемые в работе с Telegram API

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

        // Создаем экземпляр клиента Telegram с указанным токеном
        var botClient = new TelegramBotClient(token);

        // Настраиваем опции приемника обновлений
        var receiverOptions = new ReceiverOptions
        {
            // Уточняем, что принимаем только обновления типа "сообщение"
            AllowedUpdates = new[] { UpdateType.Message },
            // Пропускаем ожидающие обновления, чтобы начать работу с чистого листа
            DropPendingUpdates = true
        };

        // Создание экземпляров хранилищ данных (репозитории)
        var todoRepo = new InMemoryToDoRepository();       // Репозиторий задач
        var userRepo = new InMemoryUserRepository();       // Репозиторий пользователей

        // Сервисы для управления задачами и пользователями
        var userService = new UserService(userRepo);           // Работа с пользователями
        var toDoService = new ToDoService(todoRepo);          // Управление задачами
        var toDoReportService = new ToDoReportService(todoRepo);// Генерация отчетов по задачам

        // Создаем обработчик обновлений, передавая сервисы и репозитории
        var handler = new UpdateHandler(botClient, userService, toDoService, todoRepo, toDoReportService);

        // Подписываемся на события начала и завершения обработки обновлений
        handler.OnHandleUpdateStarted += msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
        handler.OnHandleUpdateCompleted += msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");

        try
        {
            // Регистрируем доступные команды для нашего бота
            await botClient.SetMyCommands(new BotCommand[]
            {
                new BotCommand("/start", "Начало работы"),     // Стартовая команда
                new BotCommand("/help", "Справка по командам"),// Справочная команда
                new BotCommand("/info", "Информация о сервисе"),// Команда для вывода инфо о сервисе
                new BotCommand("/addtask", "Добавить задачу"), // Добавление новой задачи
                new BotCommand("/showtasks", "Просмотреть активные задачи"),// Просмотр текущих задач
                new BotCommand("/showalltasks", "Просмотреть все задачи"),// Просмотр всех задач
                new BotCommand("/removetask", "Удалить задачу"),// Удаление задачи
                new BotCommand("/completetask", "Завершить задачу"),// Завершение задачи
                new BotCommand("/report", "Получить отчет по задачам"),// Формирование отчета
                new BotCommand("/find", "Найти задачу по названию")// Поиск задачи по имени
            });

            // Используем объект для отмены операции, если потребуется остановка
            using CancellationTokenSource cts = new();

            // Начинаем получать обновления от Telegram сервера
            botClient.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, receiverOptions, cts.Token);

            // Запрашиваем информацию о нашем боте
            var me = await botClient.GetMe();
            Console.WriteLine($"{me.Username} запущен"); // Сообщаем, что бот успешно запустился
            Console.WriteLine("Нажмите A для выхода."); // Инструкция для остановки бота

            // Блок бесконечного цикла, ожидающего нажатия клавиши 'A' для выхода
            while (true)
            {
                var key = Console.ReadKey(true); // Читаем символ клавиатуры без отображения
                if (key.KeyChar == 'a')          // Если нажата клавиша 'A'
                {
                    cts.Cancel();                 // Останавливаем процесс получения обновлений
                    break;                        // Выходим из цикла
                }
            }
        }
        finally
        {
            // После завершения подписки удаляем регистрацию событий
            handler.OnHandleUpdateStarted -= msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
            handler.OnHandleUpdateCompleted -= msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");
        }
    }
}