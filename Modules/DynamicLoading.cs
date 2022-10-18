using PostgreSQL;
using Bot_Keyboards;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;


namespace Modules
{
    public static class Loading
    {
        async public static void DynamicLoading(ITelegramBotClient botClient, long chatId, int messageId)
        {
            int oldHashLength = DB.LengthHashData(chatId);
            StartLoading(botClient, chatId, messageId);
            
            while(true)
            {
                int total_length = DB.GetAnnounCount(chatId);
                string line;
                List<string> statistic = DB.GetStatistic(chatId);
                int lengthHash = DB.LengthHashData(chatId);
                if(DB.GetState(chatId)=="StopParser" || DB.GetState(chatId)=="MainMenu")
                {
                    if(lengthHash == 0)
                    {
                        line = $"❌<b><u>Поиск объявлений завершен. Парсер не получил ни одного объявления.</u></b>\n\n╔Получено объявлений: <b>{DB.LengthHashData(chatId)}</b>\n╠Пройдено страниц: <b>{statistic[0]}</b>\n╚Пройдено объявлений: <b>{statistic[1]}</b>";

                        try
                        {
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: line,
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.BackFromParse
                            );
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        
                        
                        string state = "MainMenu";
                        DB.UpdateState(chatId, state);
                        DB.UpdateStatistic(chatId, 0, 0);
                        return;
                    }
                    else
                    {
                        int length = DB.LengthHashData(chatId);
                        line = $"✅<b><u>Поиск объявлений завершен.</u></b>\n\n╔Получено объявлений: <b>{length}</b>\n╠Пройдено страниц: <b>{statistic[0]}</b>\n╚Пройдено объявлений: <b>{statistic[1]}</b>";

                        try
                        {
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        
                        Display.HashDisplayAds(botClient, chatId, length);
                        return;
                    }
                }
                else if(DB.GetState(chatId)=="ChangeToken")
                {
                    if(lengthHash == 0)
                    {
                        line = $"❌<b>Необходимо заменить токен.</b>\n\n╔Получено объявлений: <b>{DB.LengthHashData(chatId)}</b>\n╠Пройдено страниц: <b>{statistic[0]}</b>\n╚Пройдено объявлений: <b>{statistic[1]}</b>";

                        try
                        {
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: line,
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.BackFromParse
                            );
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        
                        string state = "MainMenu";
                        DB.UpdateState(chatId, state);
                        DB.UpdateStatistic(chatId, 0, 0);
                        return;
                    }
                    else
                    {
                        int length = DB.LengthHashData(chatId);
                        line = $"✅<b>Необходимо заменить токен.\n<u>Поиск объявлений завершен.</u></b>\n\n╔Получено объявлений: <b>{length}</b>\n╠Пройдено страниц: <b>{statistic[0]}</b>\n╚Пройдено объявлений: <b>{statistic[1]}</b>";

                        try
                        {
                            await botClient.EditMessageCaptionAsync(
                                chatId: chatId,
                                messageId: messageId,
                                caption: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: line,
                                parseMode: ParseMode.Html
                            );
                        }
                        Display.HashDisplayAds(botClient, chatId, length);
                        return;
                    }
                }
                else
                {
                    if(DB.LengthHashData(chatId) > oldHashLength)
                    {
                        StartLoading(botClient, chatId, messageId);
                        lengthHash = DB.LengthHashData(chatId);
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(5000);
                    }
                }
            }
        }

        async private static void StartLoading(ITelegramBotClient botClient, long chatId, int messageId)
        {
            int total_length = DB.GetAnnounCount(chatId);
            var statistic = DB.GetStatistic(chatId);
            int lengthHash = DB.LengthHashData(chatId);
            string loadCaption = $"<b> Поиск объявлений в процессе.</b>\n\n<b>Пройдено объявлений:</b> <code>{statistic[1]}</code>\n\n<b>Получено объявлений:</b> <code>{lengthHash} ➤ {total_length}</code>";
            
            try
            {
                await botClient.EditMessageCaptionAsync(
                    chatId: chatId,
                    messageId: messageId,
                    caption: loadCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: Keyboards.StopPars
                );
            }
            catch{ return; }
        }
    }
}