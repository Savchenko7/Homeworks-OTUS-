using System;

class Program
{
    static void Main()
    {
        string userName = "";
        bool isNameSet = false;

        // Приветственное сообщение при запуске программы
        Console.WriteLine("Добро пожаловать в бот! Доступные команды: /start, /help, /info, /exit");

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

            default:
                Default(userName, isNameSet);
                break;
        }
    }

    // Метод для команды /start
    private static void Start(ref string userName, ref bool isNameSet)
    {
        if (!isNameSet)
        {
            Console.Write("Введите ваше имя: ");
            userName = Console.ReadLine().Trim();
            if (userName.Length > 0)
            {
                isNameSet = true;
                Console.WriteLine($"Привет, {userName}! Теперь вы можете использовать команды /help, /info и /echo.");
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
        if (isNameSet)
        {
            Console.WriteLine($"{userName}, справочная информация:\nДоступные команды:\n/start — начало работы\n/help — получение справки\n/info — информация о боте\n/echo — отображение введённого текста\n/exit — завершение работы");
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Метод для команды /info
    private static void Info(string userName, bool isNameSet)
    {
        string outputStr = isNameSet ?
            $"{userName}, версия программы: 1.0\nДата создания: июнь 2025" :
            "Сначала представьтесь командой /start.";
        Console.WriteLine(outputStr);
    }

    // Метод для команды /echo
    private static void Echo(string userName, bool isNameSet, string arg)
    {
        if (isNameSet)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                Console.WriteLine($"{userName}, вы ввели: '{arg}'");
            }
            else
            {
                Console.WriteLine($"{userName}, введите текст после команды /echo.");
            }
        }
        else
        {
            Console.WriteLine("Сначала представьтесь командой /start.");
        }
    }

    // Метод для команды /exit
    private static void Exit(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            Console.WriteLine($"{userName}, выход из программы. До свидания!");
        }
        else
        {
            Console.WriteLine("До свидания!");
        }
        Environment.Exit(0); // Завершаем программу
    }

    // Метод для ошибок или неизвестных команд
    private static void Default(string userName, bool isNameSet)
    {
        if (isNameSet)
        {
            Console.WriteLine($"{userName}, неверная команда. Используйте доступные команды.");
        }
        else
        {
            Console.WriteLine("Неверная команда. Сначала представьтесь командой /start.");
        }
    }
}