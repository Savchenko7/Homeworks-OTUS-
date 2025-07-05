
//главный обработчик команд и сообщений от пользователя

using Homeworks__OTUS_.Core.Exceptions;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace Homeworks__OTUS_
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoRepository _toDoRepository;

        // Делегаты и события
        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService toDoService,
            IToDoRepository toDoRepository)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _toDoRepository = toDoRepository;
        }
        // Реализация метода HandleErrorAsync
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendMessage(new Chat { Id = 1 }, "Возникла ошибка: " + exception.Message, cancellationToken);
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                {
                    OnHandleUpdateStarted?.Invoke(update.Message.Text); // Начали обрабатывать сообщение

                    await ProcessMessage(update.Message, cancellationToken);

                    OnHandleUpdateCompleted?.Invoke(update.Message.Text); // Завершаем обработку сообщения
                }
            }
            catch (OperationCanceledException)
            {
                // Ничего не делаем
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, cancellationToken); // Добавляем CancellationToken
            }
        }

        public async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(new Chat { Id = 1 }, "Возникла ошибка: " + exception.Message, cancellationToken);
        }

        private async Task ProcessMessage(Message message, CancellationToken cancellationToken)
        {
            if (message.Text.StartsWith('/'))
            {
                var parts = message.Text.Split(' ', 2);
                string command = parts.Length > 0 ? parts[0].ToLower() : "";
                string argument = parts.Length > 1 ? parts[1].Trim() : null;

                // Получаем пользователя
                ToDoUser? user = await _userService.GetUserAsync(message.From.Id, cancellationToken);

                // Регистрация пользователя при команде /start
                 if (command == "/start")
                {
                    if (user == null)
                    {
                        user =await _userService.RegisterUserAsync(message.From.Id, message.From.Username ?? "", cancellationToken);
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}! Вы успешно зарегистрированы.", cancellationToken);
                    }
                    else
                    {
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!", cancellationToken);
                    }
                    return;
                }


                // Незарегистрированный пользователь видит только команды /help и /info
                if (user == null)
                {
                    if (command == "/help" || command == "/info")
                    {
                        await ExecutePublicCommand(command, message, user, cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(message.Chat, "Вы не зарегистрированы. Для использования программы зарегистрируйтесь командой /start.\n" +
                                                            "Доступны только команды:\n" +
                                                            "/help - показать помощь\n" +
                                                            "/info - получить информацию о приложении", cancellationToken);
                    }
                    return;
                }

                  // Обработка команд зарегистрированных пользователей
                switch (command)
                {
                    case "/help":
                        await OnHelp(message, user, cancellationToken);
                        break;
                    case "/info":
                        await OnInfo(message, user, cancellationToken);
                        break;
                    case "/addtask":
                        await OnAddTask(message, user, argument, cancellationToken);
                        break;
                    case "/showtasks":
                        await OnShowTasks(message, user, cancellationToken);
                        break;
                    case "/removetask":
                        await OnRemoveTask(message, user, argument, cancellationToken);
                        break;
                    case "/completetask":
                        await OnCompleteTask(message, user, argument, cancellationToken);
                        break;
                    case "/showalltasks":
                        await OnShowAllTasks(message, user, cancellationToken);
                        break;
                    case "/report":
                        await OnReport(message, user, cancellationToken);
                        break;
                    case "/find":
                        await OnFind(message, user, argument, cancellationToken);
                        break;
                    default:
                        await _botClient.SendMessage(message.Chat, "Команды не найдено. Используйте /help для справки.", cancellationToken);
                        break;
                }
            }
            else
            {
                 await _botClient.SendMessage(message.Chat, "Команда должна начинаться с символа '/'.", cancellationToken);
            }
        }

        private async Task ExecutePublicCommand(string cmd, Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            switch (cmd)
            {
                case "/help":
                    await OnHelp(message, user, cancellationToken);
                    break;
                case "/info":
                    await OnInfo(message, user, cancellationToken);
                    break;
            }
        }

        private async Task OnHelp(Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(message.Chat, @"Доступные команды:
/start - Приветствие
/help - Справочная информация
/info - Информационная справка
/addtask Название_задачи - Добавить новую задачу
/showtasks - Показать активные задачи
/removetask ID_задачи - Удалить задачу
/completetask ID_задачи - Завершить задачу
/showalltasks - Показать все задачи
/report - Получить отчёт по задачам
/find Поисковая фраза - Найти задачи по началу названия", cancellationToken);
        }

        private async Task OnInfo(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(message.Chat, "Приложение ToDo-менеджер для отслеживания задач.", cancellationToken);
        }

        private async Task OnAddTask(Message message, ToDoUser user, string? taskName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                await _botClient.SendMessage(message.Chat, "Название задачи не должно быть пустым.", cancellationToken);
                return;
            }

            try
            {
                var addedItem = await _toDoService.AddAsync(user, taskName!, cancellationToken);
                await _botClient.SendMessage(message.Chat, $"Задача добавлена: {addedItem.Name} - {addedItem.CreatedAt} - {addedItem.Id}", cancellationToken);
            }
            catch (TaskLengthLimitException ex)
            {
                await _botClient.SendMessage(message.Chat, ex.Message, cancellationToken); // Сообщение о длине задачи
            }
            catch (TaskCountLimitException ex)
            {
                await _botClient.SendMessage(message.Chat, ex.Message, cancellationToken); // Сообщение о количестве задач
            }
            catch (DuplicateTaskException ex)
            {
                await _botClient.SendMessage(message.Chat, ex.Message, cancellationToken); // Сообщение о дубликате задачи
            }
        }

        private async Task OnShowTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var activeTasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, cancellationToken);
            if (activeTasks.Any())
            {
                int idx = 1;
                foreach (var task in activeTasks)
                {
                    await _botClient.SendMessage(message.Chat, $"{idx++}. {task.Name} - {task.CreatedAt} - {task.Id}", cancellationToken);
                }
            }
            else
            {
                await _botClient.SendMessage(message.Chat, "Нет активных задач.", cancellationToken);
            }
        }

        private async Task OnRemoveTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskIdStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken);
                return;
            }

            await _toDoService.DeleteAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача успешно удалена.", cancellationToken);
        }

        private async Task OnCompleteTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskIdStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken);
                return;
            }

            await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача отмечена как завершённая.", cancellationToken);
        }

        private async Task OnShowAllTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, cancellationToken);
            if (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    await _botClient.SendMessage(message.Chat, $"{task.Name} ({task.State}) - {task.CreatedAt} - {task.Id}", cancellationToken);
                }
            }
            else
            {
                await _botClient.SendMessage(message.Chat, "Нет задач.", cancellationToken);
            }
        }

        private async Task OnReport(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var reportService = new ToDoReportService(_toDoRepository);
            var stats = await reportService.GetUserStatsAsync(user.UserId, cancellationToken);
            await _botClient.SendMessage(message.Chat, $"Отчёт по задачам:\nВсего задач: {stats.Total}\nВыполнено: {stats.Completed}\nАктивно: {stats.Active}", cancellationToken);
        }

        private async Task OnFind(Message message, ToDoUser user, string? searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                await _botClient.SendMessage(message.Chat, "Введите фразу для поиска.", cancellationToken);
                return;
            }

            var results = await _toDoService.FindAsync(user.UserId, searchTerm!, cancellationToken);
            if (results.Any())
            {
                foreach (var task in results)
                {
                    await _botClient.SendMessage(message.Chat, $"{task.Name} - {task.CreatedAt} - {task.Id}", cancellationToken);
                }
            }
            else
            {
                await _botClient.SendMessage(message.Chat, "Задачи не найдены.", cancellationToken);
            }
        }
    }
}