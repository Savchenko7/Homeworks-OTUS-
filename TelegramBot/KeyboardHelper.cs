using Telegram.Bot.Types.ReplyMarkups;

public class KeyboardHelper
{
    public static InlineKeyboardMarkup CreateStartButton()
    {
        return new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("/start", "/start")
        });
    }

        public static ReplyKeyboardMarkup CreateRegisteredButtons()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[] {new KeyboardButton("/showalltasks")},
            new[] {new KeyboardButton("/showtasks")},
            new[] {new KeyboardButton("/report")}
        })
        {
            ResizeKeyboard = true
        };
    }
   
}
