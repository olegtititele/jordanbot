using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using ProjectFunctions;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;


namespace States
{
    public static class CountryLinkState
    {
        public static async void MessageHandler(ITelegramBotClient botClient, dynamic messageText, long chatId, int messageId)
        {
            try
            {
                string oldLink = DB.GetLink(chatId);

                if(Functions.StringIsNumber(messageText))
                {
                    using (var fileStream = new FileStream(Config.menuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: new InputOnlineFile(fileStream),
                            caption: "<b>❗️ Ссылка не должна быть числом. Введите повторно.</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToCountries
                        );
                    } 
                }
                else
                {
                    try
                    {
                        DB.UpdateLink(chatId, messageText!);

                        using (var fileStream = new FileStream(Config.warningPhoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            string platform = DB.GetPlatform(chatId);
                            string link = DB.GetLink(chatId);
                            int announCount = DB.GetAnnounCount(chatId);
                            string sellerTotalAds = DB.GetSellerAdvCount(chatId);
                            string sellerRegDate = DB.GetSellerRegData(chatId);
                            string adRegDate = DB.GetAdvRegData(chatId);
                            decimal sellerRating = DB.GetSellerRating(chatId);
                            string sellerType = DB.GetSellerType(chatId);
                            int startPage = DB.GetStartPage(chatId);
                            string localBlacklist = DB.GetLocalAndGlobalBl(chatId)[0];
                            string globalBlacklist = DB.GetLocalAndGlobalBl(chatId)[1];

                            DateTime dt;
                            if(DateTime.TryParse(adRegDate, out dt))
                            {
                                adRegDate = dt.ToString("dd.MM.yyyy");
                            }

                            if(DateTime.TryParse(sellerRegDate, out dt))
                            {
                                sellerRegDate = dt.ToString("dd.MM.yyyy");
                            }

                            await botClient.SendPhotoAsync(
                                chatId: chatId,
                                photo: new InputOnlineFile(fileStream),
                                caption: $"<b><u>Ваши параметры для парсинга: </u></b>\n\n<b>🛍 Площадка ⇒ </b>{platform}\n\n<b>📨 Ссылка ⇒ </b>{link}\n\n<b>📦 Кол-во товара ⇒ </b>{announCount}\n\n<b>🗄 Кол-во объявлений ⇒ </b>{sellerTotalAds}\n\n<b>📆 Регистрация продавца ⇒ </b>{sellerRegDate}\n\n<b>📅 Создание объявления ⇒ </b>{adRegDate}\n\n<b>⭐️ Рейтинг продавца ⇒ </b>{sellerRating}\n\n<b>👤 Тип аккаунтов ⇒ </b>{sellerType}\n\n<b>🔎 Стартовая страница ⇒ </b>{startPage}\n\n<b>📂 Локальный ЧС ⇒ </b>{localBlacklist}\n\n<b>🗂 Глобальный ЧС ⇒ </b>{globalBlacklist}",
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.StartPars
                            );
                        }
                        string state = "Parser";
                        DB.UpdateState(chatId, state);
                    }
                    catch(Exception)
                    {
                        DB.UpdateLink(chatId, oldLink);
                        using (var fileStream = new FileStream(Config.menuPhoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await botClient.SendPhotoAsync(
                                chatId: chatId,
                                photo: new InputOnlineFile(fileStream),
                                caption: "<b>❗️ Введите ссылку корректно.</b>",
                                parseMode: ParseMode.Html,
                                replyMarkup: Keyboards.BackToCountries
                            );
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}