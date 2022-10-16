using States;
using ProjectFunctions;
using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;



namespace Handlers
{
    public static class CallHandler
    {
        private static string mainMenuPhoto = Config.menuPhoto;
        public static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            
            try
            {
                long chatId = callbackQuery.Message!.Chat.Id;
                int messageId = callbackQuery.Message.MessageId;
                string? firstName = callbackQuery.Message.Chat.FirstName;
                string state = DB.GetState(chatId);
                
                if(chatId.ToString()[0]=='-')
                {
                    return;
                }

                if(MessHandler.CheckSubChannel(botClient.GetChatMemberAsync(Config.workChat, chatId).Result.Status.ToString()))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"<b>У вас нет доступа к боту!!!!</b>",
                        parseMode: ParseMode.Html
                    );
                    return;
                }

                switch(callbackQuery.Data)
                {
                    // КНОПКИ НАЗАД
                    case "back_to_menu":
                        if(DB.GetState(chatId)=="MainMenu" || DB.GetState(chatId)=="WhatsappText" || DB.GetState(chatId)=="StartPage" || DB.GetState(chatId)=="BlackList" || DB.GetState(chatId)=="CountryLink")
                        {
                            state = "MainMenu";
                            DB.UpdateState(chatId, state);
                    
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: GenerateMessageText.MenuText(chatId),
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.MainMenuButtons
                            );
                        }
                        return;
                    case "back_to_settings":
                        if(DB.GetState(chatId)=="MainMenu" || DB.GetState(chatId)=="WhatsappText" || DB.GetState(chatId)=="StartPage" || DB.GetState(chatId)=="BlackList" || DB.GetState(chatId)=="Token" || DB.GetState(chatId)=="BtcBanker")
                        {
                            state = "MainMenu";
                            DB.UpdateState(chatId, state);
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: GenerateMessageText.SettingsText(chatId),
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.SettingsKb
                            );
                        }
                        return;
                    case "back_to_configuration":
                        if(DB.GetState(chatId)=="MainMenu" || DB.GetState(chatId)=="AdvCount" || DB.GetState(chatId)=="SellerAdvCount" || DB.GetState(chatId)=="SellerRegData" || DB.GetState(chatId)=="AdvRegData"|| DB.GetState(chatId)=="SellerRating")
                        {
                            state = "MainMenu";
                            DB.UpdateState(chatId, state);
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: GenerateMessageText.ConfigurationText(chatId),
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.ConfigurationKb(chatId)
                            );
                        }
                        return;
                    case "back_to_countries":
                        if(DB.GetState(chatId) != "Parser")
                        {
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
                                caption: "<b>🌍 Выберите площадку, где вы хотите найти объявления.</b>",
                                parseMode: ParseMode.Html,
                                replyMarkup: CountriesKeyboards.CountriesSitesKb
                            );
                        }
                        return;
                    case "back_to_admin":
                        if(DB.GetState(chatId) != "Parser")
                        {
                            state = "MainMenu";
                            DB.UpdateState(chatId, state);

                            await botClient.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"<b>👑 Админ панель 👑</b>\n\n<b>Кол-во пользователей бота:</b> <code>{DB.GetUsersDbLenght()}</code>",
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.AdminKeyboard
                            );
                        }
                        return;
                    case "hide_message":
                        try
                        {
                            await botClient.DeleteMessageAsync(
                                chatId: chatId,
                                messageId: messageId
                            );
                        }
                        catch
                        {
                            await botClient.AnswerCallbackQueryAsync(
                                callbackQueryId: callbackQuery.Id,
                                text: Config.errorMessage,
                                showAlert:false
                            );
                        }

                        return;
                }

            
                switch(state)
                {
                    case "MainMenu":
                        MainMenu.CallBack(botClient, callbackQuery, chatId, messageId, firstName!, mainMenuPhoto);
                        return;
                    case "Parser":
                        ParserState.CallBack(botClient, callbackQuery, chatId, messageId, firstName!, mainMenuPhoto);
                        return;
                    case "AdvRegData":
                        AdRegDateState.CallBackHandler(botClient, callbackQuery, chatId, messageId);
                        return;
                    case "SellerRegData":
                        SellerRegDateState.CallBackHandler(botClient, callbackQuery, chatId, messageId);
                        return;
                    case "SellerAdvCount":
                        SellerAdCountState.CallBackHandler(botClient, callbackQuery, chatId, messageId);
                        return;
                    case "SellerRating":
                        SellerRatingState.CallBackHandler(botClient, callbackQuery, chatId, messageId);
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
                return; 
            }
        }
    }            
}