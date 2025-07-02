using Otus.ToDoList.ConsoleBot;

namespace Homeworks__OTUS_.Otus.ToDoList.ConsoleBot
{
    public class ConsoleBotClientBase
    {

        internal void StartReceiving(IUpdateHandler updateHandler)
        {
            ArgumentNullException.ThrowIfNull(updateHandler);
            throw new NotImplementedException();
        }
    }
}