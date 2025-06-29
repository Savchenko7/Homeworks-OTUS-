using System;
using System.Collections.Generic;

class Program
{
    static List<string> tasks = new List<string>();
    static void Main()
    {
        string userName = "";
        bool isNameSet = false;

        // Приветственное сообщение при запуске программы
        Console.WriteLine("Добро пожаловать в бот! Доступные команды: /start, /help, /info, /addtask, /showtasks, /removetask, /exit");

        while (true)
        {
            Console.Write("Введите команду: ");
            string inputCommand = Console.ReadLine().Trim(); // Удаляем лишние пробелы

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

    private static void HandleCommand(string command, ref string userName, ref bool isNameSet)
    {
        var parts = command.Split(' ', 2); // Делим строку на команду и её аргумент
        string cmd = parts.Length > 0 ? parts[0].ToLower() : ""; // Получаем саму команду
        string arg = parts.Length > 1 ? parts[1].Trim() : null; // Аргумент (если есть)

        switch (cmd)
        {
            case "/start": Start(ref userName, ref isNameSet); break;
            case "/help": Help(userName, isNameSet); break;
            case "/info": Info(userName, isNameSet); break;
            case "/echo": Echo(userName, isNameSet, arg); break;
            case "/exit": Exit(userName, isNameSet); break;
            case "/addtask": AddTask(userName, isNameSet); break;
            case "/showtasks": ShowTasks(userName, isNameSet); break;
            case "/removetask": RemoveTask(userName, isNameSet); break;
            default: Default(userName, isNameSet); break;
        }
    }

    // Метод для команды /start
    private static void Start(ref string userName, ref bool isNameSet)
    {
        if (!isNameSet)
        {
            Console.Write("Введите ваше имя: ");
            userName = Console.ReadLine().Trim();
            isNameSet = !string.IsNullOrWhiteSpace(userName); // Проверка наличия имени
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

    // Метод для команды /help
    private static void Help(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, справочная информация:\nДоступные команды:\n/start — начало работы\n/help — получение справки\n/info — информация о боте\n/echo — отображение введённого текста\n/addtask — добавление новой задачи\n/showtasks — просмотр текущих задач\n/removetask — удаление задачи\n/exit — завершение работы"
            : "Сначала представьтесь командой /start."
        );
    }

    // Метод для команды /info
    private static void Info(string userName, bool isNameSet)
    {
        Console.WriteLine(
            isNameSet
                ? $"{userName}, версия программы: 1.0\nДата создания: июнь 2025"
                : "Сначала представьтесь командой /start."
        );
    }

    // Метод для команды /echo
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

    // Метод для команды /exit
    private static void Exit(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, выход из программы. До свидания!"
            : "До свидания!"
        );
        Environment.Exit(0); // Завершаем программу
    }

    // Метод для обработки неправильных команд
    private static void Default(string userName, bool isNameSet)
    {
        Console.WriteLine(isNameSet
            ? $"{userName}, неверная команда. Используйте доступные команды."
            : "Неверная команда. Сначала представьтесь командой /start."
        );
    }

    // Новая команда для добавления задачи
    private static void AddTask(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            Console.Write("Введите описание задачи: ");
            string taskDescription = Console.ReadLine().Trim();
            tasks.Add(taskDescription);
            Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Команда для вывода текущего списка задач
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

    // Команда для удаления задачи
    private static void RemoveTask(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            if (tasks.Count > 0)
            {
                ShowTasks(userName, true); // Показываем список перед выбором

                Console.Write("Введите номер задачи для удаления: ");
                int index;
                if (int.TryParse(Console.ReadLine(), out index))
                {
                    if (index >= 1 && index <= tasks.Count)
                    {
                        string removedTask = tasks[index - 1];
                        tasks.RemoveAt(index - 1);
                        Console.WriteLine($"Задача \"{removedTask}\" удалена.");
                    }
                    else
                    {
                        Console.WriteLine("Вы указали некорректный номер задачи.");
                    }
                }
                else
                {
                    Console.WriteLine("Некорректный ввод числа. Пожалуйста, введите число.");
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
}