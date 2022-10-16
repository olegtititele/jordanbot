using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using Modules;

using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;


namespace States
{
    public static class ParserState
    {
        public static async void CallBack(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId, int messageId, string firstName, string mainMenuPhoto)
        {
            try
            {
                // КНОПКИ ДЛЯ ПАРСИНГА
                switch(callbackQuery.Data)
                {
                    case "start_pars":
                        Platforms.GetParser(DB.GetPlatform(chatId), botClient, chatId, messageId);
                        return;
                    case "stop_parser":
                        string state = "StopParser";
                        DB.UpdateState(chatId, state);
                        return;
                    case "back_from_pars":
                        state = "MainMenu";
                        DB.UpdateState(chatId, state);

                        using (var fileStream = new FileStream(Config.menuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await botClient.EditMessageMediaAsync(
                                chatId: chatId,
                                messageId: messageId,
                                media: new InputMediaPhoto(new InputMedia(fileStream, Config.menuPhoto))
                            );
                        }
                
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: GenerateMessageText.MenuText(chatId),
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.MainMenuButtons
                        );

                        return;
                }
            }
            catch
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: Config.errorMessage,
                    showAlert:false
                );
            }
        }
    }
}