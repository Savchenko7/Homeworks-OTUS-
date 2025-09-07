using System.Globalization;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoRepository _toDoRepository;
    private readonly IToDoReportService _toDoReportService;
    private readonly IScenarioContextRepository _contextRepository;

    public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoRepository toDoRepository, IToDoReportService toDoReportService, IScenarioContextRepository contextRepository)
    {
        _botClient = botClient;
        _userService = userService;
        _toDoService = toDoService;
        _toDoRepository = toDoRepository;
        _toDoReportService = toDoReportService;
        _contextRepository = contextRepository;
    }
    // Объявляем делегат
    public delegate void MessageEventHandler(string message);

    // Добавляем события
    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;
    // Метод для экранирования спецсимволов Markdown
    private string EscapeMarkdownCharacters(string input)
    {
        return input
            .Replace("\\", "\\\\")          // Экранируем сам символ \
            .Replace("-", "\\-");           // Экранируем дефис -
    }

    // Процесс обработки сообщений
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Логирование начала обработки сообщения
        OnHandleUpdateStarted?.Invoke(update.Message.Text);

        // Получаем контекст сценария пользователя
        var context = await _contextRepository.GetContext(update.Message.From.Id, cancellationToken);

        // Если присутствует активный сценарий
        if (context != null)
        {
            // Проверяем, не пришла ли команда /cancel
            if (update.Message.Text == "/cancel")
            {
                // Сбрасываем контекст сценария
                await _contextRepository.ResetContext(update.Message.From.Id, cancellationToken);
                await botClient.SendMessage(update.Message.Chat.Id, "Действие отменено.", cancellationToken: cancellationToken);
                return;
            }

            // Выполняем продолжение сценария
            await ProcessScenario(context, update, cancellationToken);
            return;
        }

        // Основная логика обработки входящих сообщений
        await ProcessMessage(botClient, update.Message, update, cancellationToken);
    }


    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка при обработке обновления: {exception.Message}");
        Console.WriteLine(exception.StackTrace); // вывод полного трассировки ошибки

        if (exception is Telegram.Bot.Exceptions.ApiRequestException apiEx && !string.IsNullOrEmpty(apiEx.Message))
        {
            await botClient.SendMessage(-1 /*Здесь укажите реальный Chat ID*/, "Возникла внутренняя ошибка. Попробуйте позже.", cancellationToken: cancellationToken);
        }
        else
        {
            Console.WriteLine("Сообщение об ошибке невозможно передать пользователю.");
        }
    }
    private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct) 
    { 
        var scenario = GetScenario(context.CurrentScenario);
        var result = await scenario.HandleMessageAsync(_botClient, context, update, ct);

        if (result == ScenarioResult.Completed) 
        { 
            await _contextRepository.ResetContext(context.UserId, ct); } 
        else 
        { 
            await _contextRepository.SetContext(context.UserId, context, ct); 
        } 
    }
    private IScenario GetScenario(ScenarioType type)
    {
        switch (type)
        {
            case ScenarioType.AddTask:
                return new AddTaskScenario(_userService, _toDoService);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Сценарий не поддерживается");
        }
    }
    public class AddTaskScenario : IScenario 
    { 
        private readonly IUserService _userService; 
        private readonly IToDoService _toDoService; 
        public AddTaskScenario(IUserService userService, IToDoService toDoService) 
        { 
            _userService = userService;
            _toDoService = toDoService; 
        } 
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask; 
        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct) 
        { 
            var message = update.Message!.Text!; 
            switch (context.CurrentStep) { case null: var user = await _userService.GetUserAsync(update.Message.From.Id, ct);
                    context.Data["CurrentUser"] = user; await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", cancellationToken: ct); 
                    context.CurrentStep = "Name"; 
                    return ScenarioResult.Transition; 
                case "Name": var taskName = message; 
                    context.Data["TaskName"] = taskName; 
                    await bot.SendMessage(update.Message.Chat.Id, "Введите срок выполнения задачи (ДД.ММ.ГГГГ):", cancellationToken: ct); 
                    context.CurrentStep = "Deadline"; 
                    return ScenarioResult.Transition; 
                case "Deadline": 
                    if (!DateTime.TryParseExact(message, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadline)) 
                    { 
                        await bot.SendMessage(update.Message.Chat.Id, "Некорректный формат даты. Повторите ввод.", cancellationToken: ct); return ScenarioResult.Transition; 
                    } 
                    var userFromContext = (ToDoUser)context.Data["CurrentUser"]; 
                    var taskNameFromContext = (string)context.Data["TaskName"]; 
                    await _toDoService.AddAsync(userFromContext, taskNameFromContext, deadline, ct); 
                    await bot.SendMessage(update.Message.Chat.Id, "Задача успешно добавлена!", cancellationToken: ct); 
                    return ScenarioResult.Completed; default: return ScenarioResult.Completed; 
            } 
        } 
    }
    private async Task ProcessMessage(ITelegramBotClient botClient, Message message, Update update, CancellationToken cancellationToken)
    {
        try
        {
            ToDoUser? user = await _userService.GetUserAsync(message.From.Id, cancellationToken);

            if (user == null)
            {
                await SendStartMenu(botClient, message.Chat.Id, cancellationToken);
            }
            else
            {
                await SendRegisteredMenu(botClient, message.Chat.Id, cancellationToken);
            }

            await ProcessCommand(botClient, message, update, cancellationToken); // Все аргументы на месте
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            await botClient.SendMessage(message.Chat.Id, "Что-то пошло не так. Попробуйте ещё раз позже.", cancellationToken: cancellationToken);
        }
    }


    private async Task SendStartMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId, "Для начала работы нажмите /start",
            replyMarkup: KeyboardHelper.CreateStartButton(), cancellationToken: cancellationToken);
    }

    private async Task SendRegisteredMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId, "Выберите нужную команду:",
            replyMarkup: KeyboardHelper.CreateRegisteredButtons(), cancellationToken: cancellationToken);
    }

    private async Task ProcessCommand(ITelegramBotClient botClient, Message message, Update update, CancellationToken cancellationToken)
    {
        if (message.Text.StartsWith('/'))
        {
            var parts = message.Text.Split(' ', 2);
            string command = parts.Length > 0 ? parts[0].ToLower() : "";
            string argument = parts.Length > 1 ? parts[1].Trim() : null;

            ToDoUser? user = await _userService.GetUserAsync(message.From.Id, cancellationToken);

            if (command == "/start")
            {
                if (user == null)
                {
                    user = await _userService.RegisterUserAsync(message.From.Id, message.From.Username ?? "", cancellationToken);
                    await botClient.SendMessage(message.Chat.Id, $"Привет, {user.TelegramUserName}! Вы успешно зарегистрировались.",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, $"Привет, {user.TelegramUserName}!",
                        cancellationToken: cancellationToken);
                }
                return;
            }

            if (user == null)
            {
                if (command == "/help" || command == "/info")
                {
                    await ExecutePublicCommand(command, message, cancellationToken);
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Вы не зарегистрированы. Для использования функций бота выполните команду /start.",
                        cancellationToken: cancellationToken);
                }
                return;
            }

            switch (command)
            {
                case "/help": await OnHelp(message, user, cancellationToken); break;
                case "/info": await OnInfo(message, user, cancellationToken); break;
                case "/addtask":
                    var addTaskContext = new ScenarioContext(message.From.Id, ScenarioType.AddTask);
                    await ProcessScenario(addTaskContext, update, cancellationToken); // Передаем update сюда
                    break;
                case "/showtasks": await OnShowTasks(message, user, cancellationToken); break;
                case "/removetask": await OnRemoveTask(message, user, argument, cancellationToken); break;
                case "/completetask": await OnCompleteTask(message, user, argument, cancellationToken); break;
                case "/showalltasks": await OnShowAllTasks(message, user, cancellationToken); break;
                case "/report": await OnReport(message, user, cancellationToken); break;
                case "/find": await OnFind(message, user, argument, cancellationToken); break;
                default:
                    await botClient.SendMessage(message.Chat.Id, "Не распознана команда. Используйте /help для просмотра доступных команд.",
                                              cancellationToken: cancellationToken); break;
            }
        }
        else
        {
            await botClient.SendMessage(message.Chat.Id, "Команды должны начинаться с символа '/', введите правильную команду.",
                                      cancellationToken: cancellationToken);
        }
    }

    private async Task ExecutePublicCommand(string cmd, Message message, CancellationToken cancellationToken)
    {
        switch (cmd)
        {
            case "/help": await OnHelp(message, null, cancellationToken); break;
            case "/info": await OnInfo(message, null, cancellationToken); break;
        }
    }

    private async Task OnHelp(Message message, ToDoUser? user, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(message.Chat.Id, @"Доступные команды:
/start - Начало работы
/help - Справочная информация
/info - Информативная справка
/addtask Название_задачи - Добавить новую задачу
/showtasks - Показать активные задачи
/removetask ID_задачи - Удалить задачу
/completetask ID_задачи - Завершить задачу
/showalltasks - Показать все задачи
/report - Получить отчёт по задачам
/find Поисковая фраза - Найти задачи по имени", cancellationToken: cancellationToken);
    }

    private async Task OnInfo(Message message, ToDoUser? user, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(message.Chat.Id, "Этот бот помогает вам управлять вашими задачами и получать отчеты о ваших делах.",
                                     cancellationToken: cancellationToken);
    }

    //private async Task OnAddTask(Message message, ToDoUser user, string? taskName, CancellationToken cancellationToken)
    //{
    //    if (string.IsNullOrWhiteSpace(taskName))
    //    {
    //        await _botClient.SendMessage(message.Chat.Id, "Имя задачи не должно быть пустым.", cancellationToken: cancellationToken);
    //        return;
    //    }

    //    try
    //    {
    //        var addedItem = await _toDoService.AddAsync(user, taskName!, deadline, cancellationToken);

    //        var safeTaskName = EscapeMarkdownCharacters(addedItem.Name);
    //        await _botClient.SendMessage(message.Chat.Id,
    //                                    $"Задача добавлена: {safeTaskName} - {addedItem.CreatedAt} - `{addedItem.Id}`",
    //                                    parseMode: ParseMode.Markdown,
    //                                    cancellationToken: cancellationToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        await _botClient.SendMessage(message.Chat.Id, $"Ошибка добавления задачи: {ex.Message}",
    //                                      cancellationToken: cancellationToken);
    //    }
    //}

    private async Task OnShowTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
    {
        try
        {
            var activeTasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, cancellationToken);
            if (activeTasks.Any())
            {
                var formattedOutput = string.Join("\n", activeTasks.Select((task, i) =>
                    $"{i + 1}. `{EscapeMarkdownCharacters(task.Name)}` - {task.CreatedAt} - `{task.Id}`"));

                await _botClient.SendMessage(message.Chat.Id, formattedOutput, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(message.Chat.Id, "Нет активных задач.", cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в OnShowTasks(): {ex.Message}");
            await _botClient.SendMessage(message.Chat.Id, "Что-то пошло не так при показе задач. Попробуйте снова.", cancellationToken: cancellationToken);
        }
    }
        private async Task OnRemoveTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(taskIdStr))
        {
            await _botClient.SendMessage(message.Chat.Id, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
            return;
        }

        if (!Guid.TryParse(taskIdStr, out Guid taskId))
        {
            await _botClient.SendMessage(message.Chat.Id, "Неверный формат ID задачи.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            await _toDoService.DeleteAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat.Id, "Задача успешно удалена.", cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await _botClient.SendMessage(message.Chat.Id, $"Ошибка удаления задачи: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }

    private async Task OnCompleteTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(taskIdStr))
        {
            await _botClient.SendMessage(message.Chat.Id, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
            return;
        }

        if (!Guid.TryParse(taskIdStr, out Guid taskId))
        {
            await _botClient.SendMessage(message.Chat.Id, "Неверный формат ID задачи.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat.Id, "Задача успешно выполнена.", cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await _botClient.SendMessage(message.Chat.Id, $"Ошибка выполнения задачи: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }

    private async Task OnShowAllTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
    {
        var allTasks = await _toDoService.GetAllByUserIdAsync(user.UserId, cancellationToken);
        if (allTasks.Any())
        {
            var formattedOutput = string.Join("\n", allTasks.Select((task, i) => $"{i + 1}. `{task.Name}` - {task.CreatedAt} - `{task.Id}`"));
            await _botClient.SendMessage(message.Chat.Id, formattedOutput, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
        else
        {
            await _botClient.SendMessage(message.Chat.Id, "Нет ни одной задачи.", cancellationToken: cancellationToken);
        }
    }

    private async Task OnReport(Message message, ToDoUser user, CancellationToken cancellationToken)
    {
        var stats = await _toDoReportService.GetUserStatsAsync(user.UserId, cancellationToken);
        await _botClient.SendMessage(
            message.Chat.Id,
            $"Отчёт по вашим задачам:\n" +
            $"Всего задач: {stats.Total}\n" +
            $"Выполнено: {stats.Completed}\n" +
            $"Активных: {stats.Active}\n" +
            $"Сгенерировано: {stats.GeneratedAt}",
            cancellationToken: cancellationToken
        );
    }

    private async Task OnFind(Message message, ToDoUser user, string? searchQuery, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await _botClient.SendMessage(message.Chat.Id, "Укажите фразу для поиска.", cancellationToken: cancellationToken);
            return;
        }

        var matchingTasks = await _toDoService.FindAsync(user.UserId, searchQuery!, cancellationToken);
        if (matchingTasks.Any())
        {
            var formattedOutput = string.Join("\n", matchingTasks.Select((task, i) => $"{i + 1}. `{task.Name}` - {task.CreatedAt} - `{task.Id}`"));
            await _botClient.SendMessage(message.Chat.Id, formattedOutput, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
        else
        {
            await _botClient.SendMessage(message.Chat.Id, "По вашей задаче ничего не найдено.", cancellationToken: cancellationToken);
        }
    }
}