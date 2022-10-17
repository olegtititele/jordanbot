using PostgreSQL;
using ProjectFunctions;

namespace ConfigFile
{
    public static class GenerateMessageText
    {
        public static string MenuText(long chatId)
        {
            string text = $"<b>BLACKGUARD PARSER</b>\n\n🆔 <b>ID:</b> <code>{chatId}</code>";
            return text;
        }

        public static string ConfigurationText(long chatId)
        {
            string sellerRegDate = DB.GetSellerRegData(chatId);
            string adRegDate = DB.GetAdvRegData(chatId);
            DateTime dt;

            if(DateTime.TryParse(sellerRegDate, out dt))
            {
                sellerRegDate = dt.ToString("dd.MM.yyyy");
            }

            if(DateTime.TryParse(adRegDate, out dt))
            {
                adRegDate = dt.ToString("dd.MM.yyyy");
            }

            string text = $"<b>📌 Количество товара — </b><code>{DB.GetAnnounCount(chatId)}</code>\n<b>📌 Количество объявлений продавца — </b><code>{DB.GetSellerAdvCount(chatId)}</code>\n<b>📌 Рейтинг продавца — </b><code>{DB.GetSellerRating(chatId)}</code>\n<b>📌 Дата регистрации продавца — </b><code>{sellerRegDate}</code>\n<b>📌 Дата регистрации объявления — </b><code>{adRegDate}</code>";

            return text;
        }

        public static string SettingsText(long chatId)
        {
            string text = $"⚒ Ваши настройки:\n\n<u><b>Текст для WhatsApp:</b></u> <code>{DB.GetWhatsappText(chatId)}</code>\n\n<u><b>Стартовая страница:</b></u> <code>{DB.GetStartPage(chatId)}</code>\n\n<u><b>Токен OLX:</b></u> <code>{Functions.EncryptKey(DB.GetToken(chatId))}</code>";

            return text;
        }
    }
}
