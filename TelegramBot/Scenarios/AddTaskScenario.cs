using System.Globalization;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

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

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!.Text!;
        switch (context.CurrentStep)
        {
            case null:
                // Первый шаг: узнаём пользователя
                var currentUser = await _userService.GetUserAsync(update.Message.From.Id, cancellationToken);
                if (currentUser == null)
                {
                    await bot.SendMessage(update.Message.Chat.Id, "Сначала зарегистрируйся.", cancellationToken: cancellationToken);
                    return ScenarioResult.Completed;
                }

                // Запоминаем пользователя
                context.Data["CurrentUser"] = currentUser;

                // Просим ввести название задачи
                await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", cancellationToken: cancellationToken);
                context.CurrentStep = "Name";
                return ScenarioResult.Transition;

            case "Name":
                // Второй шаг: запоминаем название задачи
                var taskName = message;
                context.Data["TaskName"] = taskName;

                // Просим ввести дедлайн
                await bot.SendMessage(update.Message.Chat.Id, "Введите дедлайн задачи в формате dd.MM.yyyy:", cancellationToken: cancellationToken);
                context.CurrentStep = "Deadline";
                return ScenarioResult.Transition;

            case "Deadline":
                // Третий шаг: парсим дедлайн
                if (!DateTime.TryParseExact(message, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadline))
                {
                    await bot.SendMessage(update.Message.Chat.Id, "Ошибка формата даты. Повторите попытку.", cancellationToken: cancellationToken);
                    return ScenarioResult.Transition;
                }

                // Получаем пользователя и название задачи
                var savedUser = (ToDoUser)context.Data["CurrentUser"];
                var savedTaskName = (string)context.Data["TaskName"];

                // Добавляем задачу
                await _toDoService.AddAsync(savedUser, savedTaskName, deadline, cancellationToken);

                // Уведомляем пользователя
                await bot.SendMessage(update.Message.Chat.Id, "Задача успешно добавлена!", cancellationToken: cancellationToken);
                return ScenarioResult.Completed;

            default:
                return ScenarioResult.Completed;
        }
    }
}
