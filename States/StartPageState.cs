using PostgreSQL;
using Bot_Keyboards;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace States
{
    public static class StartPageState
    {
        public static async void MessageHandler(ITelegramBotClient botClient, string messageText, long chatId, int messageId, string mainMenuPhoto)
        {
            try
            {
                FileStream fileStream = new FileStream(mainMenuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read);

                if(int.TryParse(messageText, out int number) && Int32.Parse(messageText)>0)
                {
                    DB.UpdateStartPage(chatId, Int32.Parse(messageText));
                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(fileStream),
                        caption: $"<b>Стартовая страница обновлена на:</b> <code>{DB.GetStartPage(chatId)}</code>",
                        parseMode: ParseMode.Html,
                        replyMarkup: Keyboards.BackToSettings
                    );

                    string state = "MainMenu";
                    DB.UpdateState(chatId, state);      
                }
                else
                {
                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(fileStream),
                        caption: "<b>❗️ Стартовая страница должна быть числом и должна быть больше 0. Введите еще раз.</b>",
                        parseMode: ParseMode.Html,
                        replyMarkup: Keyboards.BackToSettings
                    );
                }
            }
            catch
            {
                return;
            }
        }
    }
}