using PostgreSQL;
using Bot_Keyboards;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace States
{
    public static class AdCountState
    {
        public static async void MessageHandler(ITelegramBotClient botClient, string messageText, long chatId, int messageId, string mainMenuPhoto)
        {
            try
            {
                FileStream fileStream = new FileStream(mainMenuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read);
            
                if(int.TryParse(messageText, out int number))
                {
                    if(Int32.Parse(messageText) <= 100 && Int32.Parse(messageText) != 0)
                    {
                        DB.UpdateAnnounCount(chatId, messageText);
                        await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: new InputOnlineFile(fileStream),
                            caption: $"<b>Количество товара обновлено на:</b> <code>{DB.GetAnnounCount(chatId)}</code>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToConfiguration
                        );

                        string state = "MainMenu";
                        DB.UpdateState(chatId, state);
                    }
                    else
                    {
                        await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: new InputOnlineFile(fileStream),
                            caption: "<b>❗️ Не должно превышать 100 и должно быть больше 0.</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToConfiguration
                        );
                    }
                }
                else
                {
                    await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: new InputOnlineFile(fileStream),
                        caption: "<b>❗️ Должно быть цифрой и не должно превышать 100. Введите повторно.</b>",
                        parseMode: ParseMode.Html,
                        replyMarkup: Keyboards.BackToConfiguration
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