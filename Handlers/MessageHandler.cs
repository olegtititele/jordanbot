using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using CommandsSpace;
using States;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace Handlers
{
    public static class MessHandler
    {
        private static string mainMenuPhoto = Config.menuPhoto;
        public static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {

            try
            {
                long chatId = message.Chat.Id;
                string? messageText = message.Text;
                int messageId = message.MessageId;
                string? username = message.Chat.Username;
                string? firstName = message.Chat.FirstName;
                string state = DB.GetState(chatId);
                
                if(chatId.ToString()[0]=='-')
                {
                    return;
                }

                if(messageText == null)
                {
                    messageText = "-------";
                }

                if(DB.GetState(chatId) == "Parser")
                {
                    try
                    {
                        await botClient.DeleteMessageAsync(
                            chatId: chatId,
                            messageId: messageId
                        );
                    }
                    catch(Exception){ }
                    return;
                }

                if(messageText![0]=='/')
                {
                    if(messageText!.Contains("/start"))
                    {
                        Commands.StartCommand(botClient, messageText!, chatId, messageId, firstName!, mainMenuPhoto, username!);
                        return;
                    }
                }
                else
                {
                    if(CheckSubChannel(botClient.GetChatMemberAsync(Config.workChat, chatId).Result.Status.ToString()))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"<b>У вас нет доступа к боту!!!!</b>",
                            parseMode: ParseMode.Html
                        );
                        return;
                    }
                    switch(messageText)
                    {
                        case "Меню":
                            state = "MainMenu";
                            DB.UpdateState(chatId, state);
                            using (var fileStream = new FileStream(mainMenuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(fileStream),
                                    caption: GenerateMessageText.MenuText(chatId),
                                    parseMode: ParseMode.Html,
                                    replyMarkup: Keyboards.MainMenuButtons
                                );
                            }
                            return;
                        case "Админ":
                            if(Config.adminChatsId.Contains(chatId))
                            {
                                state = "MainMenu";
                                DB.UpdateState(chatId, state);
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"<b>👑 Админ панель 👑</b>\n\n<b>Кол-во пользователей бота:</b> <code>{DB.GetUsersDbLenght()}</code>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: Keyboards.AdminKeyboard
                                );
                            }
                            return;
                    }
                    switch (state)
                    {
                        case "WhatsappText":
                            WhatsappTextState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);  
                            return;
                        case "StartPage":
                            StartPageState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "Token":
                            TokenState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "CountryLink":
                            CountryLinkState.MessageHandler(botClient, messageText!, chatId, messageId); 
                            return;
                        case "AdvCount":
                            AdCountState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "SellerAdvCount":
                            SellerAdCountState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "SellerRegData":
                            SellerRegDateState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "SellerRating":
                            SellerRatingState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "AdvRegData":
                            AdRegDateState.MessageHandler(botClient, messageText!, chatId, messageId, mainMenuPhoto);
                            return;
                        case "AdminAlert":
                            AdminAlertState.Alert(botClient, message, chatId);
                            return;
                        default:
                            return;
                    }
                }
            }   
            catch(Exception)
            {
                return;
            }
        }
        public static bool CheckSubChannel(string chatMember)
        {
            if(chatMember == "Left")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}