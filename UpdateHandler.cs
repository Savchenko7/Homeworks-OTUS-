using System;
using System.Linq;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
    {
        _botClient = botClient;
        _userService = userService;
        _toDoService = toDoService;
    }

    // Реализуем интерфейс IUpdateHandler правильно, возвращая void
    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            if (update.Message != null && update.Message.Text != null)
            {
                ProcessMessage(update.Message).GetAwaiter().GetResult(); // Синхронно ждём завершения асинхронного метода
            }
        }
        catch (Exception ex)
        {
            _botClient.SendMessage(update.Message.Chat, $"Произошла ошибка: {ex.Message}");
        }
    }

    private async Task ProcessMessage(Message message)
    {
        if (message.Text.StartsWith('/'))
        {
            var parts = message.Text.Split(' ', 2);
            string cmd = parts.Length > 0 ? parts[0].ToLower() : "";
            string? arg = parts.Length > 1 ? parts[1].Trim() : null;

            // Получаем пользователя
            ToDoUser? user = _userService.GetUser(message.From.Id);

            // Регистрация пользователя при команде /start
            if (cmd == "/start")
            {
                if (user == null)
                {
                    user = _userService.RegisterUser(message.From.Id, message.From.Username ?? "");
                    _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}! Ты успешно зарегистрирован.");
                }
                else
                {
                    _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!");
                }
                return;
            }

            // Незарегистрированным пользователям доступно только /help и /info
            if (user == null)
            {
                if (cmd == "/help" || cmd == "/info")
                {
                    await ExecutePublicCommand(cmd, message, user);
                }
                else
                {
                    _botClient.SendMessage(message.Chat, "Вы не зарегистрированы. Для использования программы зарегистрируйтесь командой /start.\n" +
                                                        "Доступны только команды:\n" +
                                                        "/help - показать помощь\n" +
                                                        "/info - получить информацию о приложении");
                }
                return;
            }

            // Зарегистрированным пользователям доступны все команды
            switch (cmd)
            {
                case "/help":
                    await OnHelp(message, user);
                    break;
                case "/info":
                    await OnInfo(message, user);
                    break;
                case "/addtask":
                    await OnAddTask(message, user, arg);
                    break;
                case "/showtasks":
                    await OnShowTasks(message, user);
                    break;
                case "/removetask":
                    await OnRemoveTask(message, user, arg);
                    break;
                case "/completetask":
                    await OnCompleteTask(message, user, arg);
                    break;
                case "/showalltasks":
                    await OnShowAllTasks(message, user);
                    break;
                default:
                    _botClient.SendMessage(message.Chat, "Команды не найдено. Используйте /help для помощи.");
                    break;
            }
        }
        else
        {
            // Сообщение не начинается с '/', выводим подсказку
            _botClient.SendMessage(message.Chat, "Команда должна начинаться с символа '/'");
        }
    }

    // Обрабатываем команды /help и /info специально для незарегистрированных пользователей
    private async Task ExecutePublicCommand(string cmd, Message message, ToDoUser? user)
    {
        switch (cmd)
        {
            case "/help":
                await OnHelp(message, user);
                break;
            case "/info":
                await OnInfo(message, user);
                break;
        }
    }

    // Обработчики команд
    private async Task OnStart(Message message, ToDoUser user)
    {
        _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!"); // Без await
    }

    private async Task OnHelp(Message message, ToDoUser user)
    {
        _botClient.SendMessage(message.Chat, @"
Доступные команды:
/start - Приветствие
/help - Показать помощь
/info - Информация о приложении
/addtask название-задачи - Добавить задачу
/showtasks - Показать активные задачи
/removetask номер-задачи - Удалить задачу
/completetask guid-задачи - Завершить задачу
/showalltasks - Показать все задачи"); // Без await
    }

    private async Task OnInfo(Message message, ToDoUser? user)
    {
        if (user == null)
        {
            _botClient.SendMessage(message.Chat, "Версия программы: 1.0");
        }
        else
        {
            _botClient.SendMessage(message.Chat, "Версия программы: 1.0");
        }
    }

    private async Task OnAddTask(Message message, ToDoUser user, string? taskName)
    {
        if (string.IsNullOrWhiteSpace(taskName))
        {
            _botClient.SendMessage(message.Chat, "Название задачи не может быть пустым."); // Без await
            return;
        }

        var addedItem = _toDoService.Add(user, taskName!);
        _botClient.SendMessage(message.Chat, $"Задача добавлена: {addedItem.Name} - {addedItem.CreatedAt} - {addedItem.Id}"); // Без await
    }

    private async Task OnShowTasks(Message message, ToDoUser user)
    {
        var activeTasks = _toDoService.GetActiveByUserId(user.UserId);
        if (activeTasks.Any())
        {
            int idx = 1; // Номер задачи
            foreach (var task in activeTasks)
            {
                _botClient.SendMessage(message.Chat, $"{idx++}. {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
        else
        {
            _botClient.SendMessage(message.Chat, "Активных задач нет.");
        }
    }

    private async Task OnRemoveTask(Message message, ToDoUser user, string? taskNumberStr)
    {
        if (string.IsNullOrWhiteSpace(taskNumberStr))
        {
            _botClient.SendMessage(message.Chat, "Номер задачи не указан.");
            return;
        }

        if (!int.TryParse(taskNumberStr, out int taskNumber))
        {
            _botClient.SendMessage(message.Chat, "Некорректный номер задачи.");
            return;
        }

        // Получаем активный список задач
        var activeTasks = _toDoService.GetActiveByUserId(user.UserId);
        if (taskNumber > 0 && taskNumber <= activeTasks.Count)
        {
            var taskToDelete = activeTasks.ElementAt(taskNumber - 1); // Индекс начинается с нуля
            _toDoService.Delete(taskToDelete.Id);
            _botClient.SendMessage(message.Chat, "Задача успешно удалена.");
        }
        else
        {
            _botClient.SendMessage(message.Chat, "Некорректный номер задачи.");
        }
    }

    private async Task OnCompleteTask(Message message, ToDoUser user, string? taskIdStr)
    {
        if (Guid.TryParse(taskIdStr!, out var taskId))
        {
            _toDoService.MarkCompleted(taskId);
            _botClient.SendMessage(message.Chat, "Задача помечена как выполненная."); // Без await
        }
        else
        {
            _botClient.SendMessage(message.Chat, "Некорректный идентификатор задачи."); // Без await
        }
    }

    private async Task OnShowAllTasks(Message message, ToDoUser user)
    {
        var allTasks = _toDoService.GetAllByUserId(user.UserId);
        if (allTasks.Any())
        {
            foreach (var task in allTasks)
            {
                _botClient.SendMessage(message.Chat, $"({task.State}) {task.Name} - {task.CreatedAt} - {task.Id}"); // Без await
            }
        }
        else
        {
            _botClient.SendMessage(message.Chat, "Задач нет."); // Без await
        }
    }
}