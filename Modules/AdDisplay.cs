using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using States;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Modules
{
    public static class Display
    {
        private static string state="";
        private static string error_image = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        public static void HashDisplayAds(ITelegramBotClient botClient, long chatId, int length)
        {

            List<List<string>> parsedAds = DB.GetHashData(chatId);

            int counter = 1;
            foreach(List<string> parsedAd in parsedAds)
            {
                List<string> ad = DeleteSpecialSymbols(parsedAd);
                ShowAd(botClient, chatId, length, ad, counter);
                System.Threading.Thread.Sleep(1000);
                counter++;
            }


            state = "MainMenu";
            DB.UpdateState(chatId, state);
            DB.UpdateStatistic(chatId, 0, 0);
            DB.ClearHash(chatId);
            return;
        }

        async static void ShowAd(ITelegramBotClient botClient, long chatId, int length, List<string> ad, int counter)
        {
            try
            {
                string adInfo;
                string adPlatform = ad[0];
                string adTitle = ad[1];
                string adPrice = ad[2];
                string adRegDate = ConvertDate(ad[3]);
                string adLink = ad[4];
                string adLocation = ad[5];
                string adImage = ad[6];
                string sellerName = ad[7];
                string sellerLink = ad[8];
                string sellerPhoneNumber = ad[9];
                string sellerTotalAds = ad[10];
                string sellerRegDate = ConvertDate(ad[11]);
                string sellerType = ad[12];
                string sellerRating = ad[13];
                
                string whatsappText = LinkGenerator.GenerateWhatsAppText(DB.GetWhatsappText(chatId), adLink, adTitle, adPrice, adLocation, sellerName);
                string whatsapp = $"<a href=\"https://api.whatsapp.com/send?phone={sellerPhoneNumber}&text={whatsappText}\">💚 WhatsApp</a>";
                string viber;
                if(sellerPhoneNumber[0] == '+')
                {
                    viber = $"<a href=\"https://viber.click/{sellerPhoneNumber.Split("+")[1]}\">💜 Viber</a>";
                }
                else
                {
                    viber = $"<a href=\"https://viber.click/{sellerPhoneNumber}\">💜 Viber</a>";
                }
                    
                    
                if(adPlatform == "kijiji.ca" || adPlatform.Contains("opensooq"))
                {
                    adInfo = $"<b>📦 Название объявления: </b><a href=\"{adLink}\">{adTitle}</a>\n<b>💲Цена: </b><code>{adPrice}</code>\n<b>🌏 Местоположение: </b><code>{adLocation}</code>\n<b>📅 Дата создания объявления: </b><code>{adRegDate}</code>\n\n<b>📞 Номер продавца: </b><code>{sellerPhoneNumber}</code>\n\n{whatsapp}\n{viber}\n\n<b>🧔🏻 Продавец: </b><a href=\"{sellerLink}\">{sellerName}</a>\n<b>⭐️ Рейтинг продавца: </b><code>{sellerRating}/5</code>\n<b>📝 Количество объявлений продавца: </b><code>{sellerTotalAds}</code>\n<b>📆 Дата регистрации продавца: </b><code>{sellerRegDate}</code>\n<b>📃 Тип аккаунта: </b><code>{sellerType}</code>\n\n<b>Объявление {counter}|{length}</b>";
                }     
                else
                {
                    adInfo = $"<b>📦 Название объявления: </b><a href=\"{adLink}\">{adTitle}</a>\n<b>💲Цена: </b><code>{adPrice}</code>\n<b>🌏 Местоположение: </b><code>{adLocation}</code>\n<b>📅 Дата создания объявления: </b><code>{adRegDate}</code>\n\n<b>📞 Номер продавца: </b><code>{sellerPhoneNumber}</code>\n\n{whatsapp}\n{viber}\n\n<b>🧔🏻 Продавец: </b><a href=\"{sellerLink}\">{sellerName}</a>\n<b>📝 Количество объявлений продавца: </b><code>{sellerTotalAds}</code>\n<b>📆 Дата регистрации продавца: </b><code>{sellerRegDate}</code>\n<b>📃 Тип аккаунта: </b><code>{sellerType}</code>\n\n<b>Объявление {counter}|{length}</b>";
                }

                ReplyKeyboardMarkup? kb = null;
                if(counter==length)
                {
                    kb = Keyboards.MenuKb(chatId);
                }

                try
                {
                    try
                    {
                        await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: adImage,
                            caption: adInfo,
                            replyMarkup: kb,
                            parseMode: ParseMode.Html
                        );
                    }
                    catch
                    {
                        await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: error_image,
                            caption: adInfo,
                            replyMarkup: kb,
                            parseMode: ParseMode.Html
                        );
                    }
                }
                catch{ return; }
            }
            catch
            {
                return;
            }
        }

        static List<string> DeleteSpecialSymbols(List<string> ad)
        {
            List<string> newAdList= new List<string>();
            
            foreach(string a in ad)
            {
                string info = a.Replace(">", "").Replace("<", "").Replace('"', '`');
                newAdList.Add(info);
            }

            return newAdList;
        }

        static string ConvertDate(string date)
        {
            string convertedDate;

            if(DateTime.TryParse(date, out DateTime dt))
            {
                convertedDate = dt.ToString("dd.MM.yyyy");
            }
            else
            {
                convertedDate = "Не указана";
            }

            return convertedDate;
        }
    }
}