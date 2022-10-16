using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using Handlers;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace CommandsSpace
{
    public static class Commands
    {
        public static async void StartCommand(ITelegramBotClient botClient, string messageText, long chatId, int messageId, string firstName, string mainMenuPhoto, string username)
        {
            if(DB.CheckUser(chatId))
            {
                if(MessHandler.CheckSubChannel(botClient.GetChatMemberAsync(Config.workChat, chatId).Result.Status.ToString()))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"<b>У вас нет доступа к боту!!!!</b>",
                        parseMode: ParseMode.Html
                    );
                    return;
                }
                if(DB.GetState(chatId) != "Parser")
                {
                    string state = "MainMenu";
                    DB.UpdateState(chatId, state);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"⚡️",
                        replyMarkup: Keyboards.MenuKb(chatId)
                    );
                    
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
                }  
            }
            else
            {
                try
                {
                    DB.CreateUser(chatId, username);
                    DB.CreateAdvertisementTable(chatId);
                    DB.CreateHashTable(chatId);
                    
                    if(MessHandler.CheckSubChannel(botClient.GetChatMemberAsync(Config.workChat, chatId).Result.Status.ToString()))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"<b>У вас нет доступа к боту!!!!</b>",
                            parseMode: ParseMode.Html
                        );
                        return;
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: Config.logsChat,
                        text: $"@{username} запустил бота.",
                        parseMode: ParseMode.Html
                    );

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"⚡️",
                        replyMarkup: Keyboards.MenuKb(chatId)
                    );

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
                }
                catch(Exception)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"<b>Чтобы пользоваться ботом нужно указать @username.</b>",
                        parseMode: ParseMode.Html
                    );
                }
            }
            return;
        }
    }
}