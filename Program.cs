using System;
using System.Collections.Generic;

class Program
{
    static List<string> tasks = new List<string>();
    static int maxTaskCount;       // Максимальное количество задач
    static int maxTaskLength;      // Максимальная длина задачи

    static void Main()
    {
        try
        {
            // Установка предельных значений прямо при запуске программы
            maxTaskCount = GetMaxTaskCount();
            maxTaskLength = GetMaxTaskLength();

            string userName = "";
            bool isNameSet = false;

            Console.WriteLine("Добро пожаловать в бот! Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit");

            while (true)
            {
                Console.Write("Введите команду: ");
                string inputCommand = Console.ReadLine().Trim();

                if (inputCommand.StartsWith("/"))
                {
                    HandleCommand(inputCommand, ref userName, ref isNameSet);
                }
                else
                {
                    Console.WriteLine("Команда должна начинаться с символа '/'. Повторите попытку.");
                }
            }
        }
        catch (Exception ex)
        {
            // Общий блок обработки неожиданных ошибок
            Console.WriteLine($"Произошла непредвиденная ошибка: Type={ex.GetType().Name}, Message={ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: Type={ex.InnerException.GetType().Name}, Message={ex.InnerException.Message}");
            }
            Console.WriteLine(ex.StackTrace);
        }
    }

    // Основная логика обработки команд
    private static void HandleCommand(string command, ref string userName, ref bool isNameSet)
    {
        var parts = command.Split(' ', 2);
        string cmd = parts.Length > 0 ? parts[0].ToLower() : "";
        string arg = parts.Length > 1 ? parts[1].Trim() : null;

        try
        {
            switch (cmd)
            {
                case "/start":
                    Start(ref userName, ref isNameSet);
                    break;
                case "/help":
                    Help(userName, isNameSet);
                    break;
                case "/info":
                    Info(userName, isNameSet);
                    break;
                case "/echo":
                    Echo(userName, isNameSet, arg);
                    break;
                case "/exit":
                    Exit(userName, isNameSet);
                    break;
                case "/addtask":
                    AddTask(userName, isNameSet);
                    break;
                case "/showtasks":
                    ShowTasks(userName, isNameSet);
                    break;
                case "/removetask":
                    RemoveTask(userName, isNameSet);
                    break;
                default:
                    Default(userName, isNameSet);
                    break;
            }
        }
        catch (TaskCountLimitException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (TaskLengthLimitException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (DuplicateTaskException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // Остальные методы сохраняются без изменений

    // Метод начальной регистрации пользователя
    private static void Start(ref string userName, ref bool isNameSet)
    {
        if (!isNameSet)
        {
            Console.Write("Введите ваше имя: ");
            userName = Console.ReadLine().Trim();
            isNameSet = !string.IsNullOrWhiteSpace(userName);
            if (isNameSet)
            {
                Console.WriteLine($"Привет, {userName}! Чем могу помочь?");
            }
            else
            {
                Console.WriteLine("Имя не задано. Попробуйте снова.");
            }
        }
        else
        {
            Console.WriteLine($"{userName}, имя уже установлено ранее.");
        }
    }

    // Справочник команд
    private static void Help(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, справочная информация:\nДоступные команды:\n/start — начало работы\n/help — получение справки\n/info — информация о боте\n/echo — отображение введённого текста\n/addtask — добавление новой задачи\n/showtasks — просмотр текущих задач\n/removetask — удаление задачи\n/exit — завершение работы"
            : "Сначала представьтесь командой /start."
        );
    }

    // Информационная справка о версии
    private static void Info(string userName, bool isNameSet)
    {
        Console.WriteLine(
            isNameSet
                ? $"{userName}, версия программы: 1.0\nДата создания: июнь 2025"
                : "Сначала представьтесь командой /start."
        );
    }

    // Простое эхо-сообщение
    private static void Echo(string userName, bool isNameSet, string arg)
    {
        if (isNameSet)
        {
            Console.WriteLine(!string.IsNullOrEmpty(arg)
                ? $"{userName}, вы ввели: '{arg}'"
                : $"{userName}, введите текст после команды /echo."
            );
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Закрытие программы
    private static void Exit(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, выход из программы. До свидания!"
            : "До свидания!"
        );
        Environment.Exit(0);
    }

    // Сообщение при неизвестной команде
    private static void Default(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, неверная команда. Используйте доступные команды."
            : "Неверная команда. Сначала представьтесь командой /start."
        );
    }

    // Метод для добавления задачи
    private static void AddTask(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            if (tasks.Count >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount);
            }

            Console.Write("Введите описание задачи: ");
            string taskDescription = Console.ReadLine().Trim();
            ValidateString(taskDescription);

            if (taskDescription.Length > maxTaskLength)
            {
                throw new TaskLengthLimitException(taskDescription.Length, maxTaskLength);
            }

            if (tasks.Contains(taskDescription))
            {
                throw new DuplicateTaskException(taskDescription);
            }

            tasks.Add(taskDescription);
            Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Просмотр списка задач
    private static void ShowTasks(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            if (tasks.Count > 0)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {tasks[i]}");
                }
            }
            else
            {
                Console.WriteLine("Список задач пуст.");
            }
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Удаление задачи
    private static void RemoveTask(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            if (tasks.Count > 0)
            {
                ShowTasks(userName, true); // показываем список перед выбором

                Console.Write("Введите номер задачи для удаления: ");
                int index;
                if (int.TryParse(Console.ReadLine(), out index))
                {
                    if (index >= 1 && index <= tasks.Count)
                    {
                        string removedTask = tasks[index - 1];
                        tasks.RemoveAt(index - 1);
                        Console.WriteLine($"Задача №{index}: '{removedTask}' удалена.");
                    }
                    else
                    {
                        Console.WriteLine("Указанный индекс вне диапазона.");
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка формата индекса.");
                }
            }
            else
            {
                Console.WriteLine("Нет задач для удаления.");
            }
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Запрашивает ограничение на количество задач
    private static int GetMaxTaskCount()
    {
        while (true)
        {
            Console.Write("Введите максимально допустимое количество задач (от 1 до 100): ");
            string input = Console.ReadLine();
            try
            {
                return ParseAndValidateInt(input, 1, 100);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    // Запрашивает максимальную длину задачи
    private static int GetMaxTaskLength()
    {
        while (true)
        {
            Console.Write("Введите максимально допустимую длину задачи (от 1 до 100 символов): ");
            string input = Console.ReadLine();
            try
            {
                return ParseAndValidateInt(input, 1, 100);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    // Проверяет числовое значение и диапазон
    public static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (int.TryParse(str, out int result) && result >= min && result <= max)
        {
            return result;
        }
        throw new ArgumentException($"Значение должно быть числом между {min} и {max}.");
    }

    // Проверяет валидность строки
    public static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Строка не должна быть пустой или состоять только из пробелов.");
        }
    }

    // Пользовательские исключения

    // Превышение ограничения на количество задач
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit) : base($"Превышено максимальное количество задач равное {taskCountLimit}")
        { }
    }

    // Превышение ограничения на длину задачи
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit) : base($"Длина задачи ({taskLength}) превышает максимально допустимое значение {taskLengthLimit}")
        { }
    }

    // Задача уже существует
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task) : base($"Задача '{task}' уже существует.")
        { }
    }
}