using PostgreSQL;
using Bot_Keyboards;
using ProjectFunctions;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace States
{
    public static class TokenState
    {
        public static async void MessageHandler(ITelegramBotClient botClient, dynamic messageText, long chatId, int messageId, string mainMenuPhoto)
        {
            try
            {
                FileStream fileStream = new FileStream(mainMenuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read);
                string oldToken = DB.GetToken(chatId);

                try
                {
                    DB.UpdateToken(chatId, messageText!);
                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(fileStream),
                        caption: $"<b>Токен обновлен на:</b> <code>{DB.GetToken(chatId)}</code>",
                        parseMode: ParseMode.Html,
                        replyMarkup: Keyboards.BackToSettings
                    );

                    string state = "MainMenu";
                    DB.UpdateState(chatId, state);
                }
                catch(Telegram.Bot.Exceptions.ApiRequestException)
                {
                    DB.UpdateToken(chatId, oldToken);
                    fileStream = new FileStream(mainMenuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(fileStream),
                        caption: "<b>❗️ Введите корректный токен.</b>",
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