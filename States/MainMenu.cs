using PostgreSQL;
using Bot_Keyboards;
using ConfigFile;
using Modules;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace States
{
    public static class MainMenu
    {
        public static async void CallBack(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId, int messageId, string firstName, string mainMenuPhoto)
        {
            string state;

            try
            {
                // ВЫБОР СТРАНЫ
                foreach(string country in Config.Countries)
                {
                    
                    if(callbackQuery.Data==country)
                    {
                        if(country=="olx.pt" || country=="olx.pl" || country=="olx.bg" || country=="olx.ro")
                        {
                            if(DB.GetToken(chatId)=="Не указан")
                            {
                                await botClient.AnswerCallbackQueryAsync(
                                    callbackQueryId: callbackQuery.Id,
                                    text: "❕ Не указан токен для парсинга! Обязательно укажите его в настройках.",
                                    showAlert:false
                                );
                                return;
                            }
                        }
                        state = "CountryLink";
                        DB.UpdateState(chatId, state);
                        DB.UpdatePlatform(chatId, callbackQuery.Data);

                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: CountryLink.GetCountryLink(country),
                            replyMarkup: Keyboards.BackToCountries,
                            parseMode: ParseMode.Html
                        );
                    }
                }
                
                switch(callbackQuery.Data)
                {
                    // ADMIN
                    case "alert":
                        state = "AdminAlert";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageTextAsync(
                            chatId: chatId,
                            messageId: messageId,
                            text: $"<b>📝 Отправьте текст/фото с текстом рассылки:</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToAdmin
                        );
                        return;

                    case "show_services":
                    // ПОСМОТРЕТЬ СЕРВИСЫ
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>🌍 Выберите площадку, где вы хотите найти объявления.</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: CountriesKeyboards.CountriesSitesKb
                        );
                        return;
                    case "settings":
                    // НАСТРОЙКИ
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: GenerateMessageText.SettingsText(chatId),
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.SettingsKb
                        );
                        return;
                    case "configuration":
                    // КОНФИГУРАЦИЯ
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: GenerateMessageText.ConfigurationText(chatId),
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.ConfigurationKb(chatId)
                        );
                        return;
                    case "olx":
                    // OLX
                        await botClient.EditMessageReplyMarkupAsync(
                            chatId: chatId,
                            messageId: messageId,
                            replyMarkup: CountriesKeyboards.OLXKb
                        );
                        return;
                    case "opensooq":
                    // opensooq
                        await botClient.EditMessageReplyMarkupAsync(
                            chatId: chatId,
                            messageId: messageId,
                            replyMarkup: CountriesKeyboards.OPENSOOQKb
                        );
                        return;
                    case "account_type":
                    // ВЫБОР ТИПА АККАУНТА
                        if(DB.ShowAccType(chatId) == "Частное лицо")
                        {
                            DB.UpdateAccType(chatId);
                            await botClient.EditMessageReplyMarkupAsync(
                                chatId: chatId,
                                messageId: messageId,
                                replyMarkup: Keyboards.ConfigurationKb(chatId)
                            );
                        }
                        else
                        {
                            DB.UpdateAccType(chatId);
                            await botClient.EditMessageReplyMarkupAsync(
                                chatId: chatId,
                                messageId: messageId,
                                replyMarkup: Keyboards.ConfigurationKb(chatId)
                            );
                        }
                        return;
                    case "quantity_of_goods":
                    // КОЛ-ВО ТОВАРА
                        state = "AdvCount";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>📕 Введите количество товара.\n\n✔️ Пример: </b><u>100</u>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToConfiguration
                        );
                        return;
                    case "seller_announ_count":
                    // КОЛ-ВО ОБЪЯВЛЕНИЙ ПРОДАВЦА
                        state = "SellerAdvCount";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>📙 Введите число количество объявлений продавца.\n\n✔️ Пример: </b><u>10</u> (парсер будет искать продавцов у которых кол-во объявлений не будет превышать 10)",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.sellerTotalAdsKb
                        );
                        return;
                    case "seller_rating":
                    // РЕЙТИНГ ПРОДАВЦА
                        state = "SellerRating";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>📝 Введите рейтинг продавца(Oт 0 до 5).\n\n✔️ Пример: </b><u>2,5</u> (парсер будет искать продавцов у которых рейтинг не будет превышать 2.5)",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.sellerRatingKb
                        );
                        return;
                    case "seller_reg":
                    // ДАТА РЕГИСТРАЦИИ ПРОДАВЦА
                        state = "SellerRegData";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>📗 Укажите дату регистрации продавца.\n\n✔️ Пример: </b><u>01.01.2022</u> (парсер будет искать продавцов, которые зарегистрировались с 01.01.2022 по текущую дату)",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.RegDateKb()
                        );
                        return;
                    case "adv_reg":
                    // ДАТА РЕГИСТРАЦИИ ОБЪЯВЛЕНИЯ
                        state = "AdvRegData";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>📘 Укажите дату создания объявления.\n\n✔️ Пример: </b><u>01.01.2022</u> (парсер будет искать объявления, которые были созданы с 01.01.2022 по текущую дату)",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.RegDateKb()
                        );
                        return;
                    case "whatsapp_text":
                    // ТЕКСТ ДЛЯ WHATSAPP
                        string text = "<b>🖊 Введите текст для WhatsApp (<u>макс. 500 символов</u>):</b>\n\n<b>Ключевые слова для вставки:</b>\n<code>@adlink</code>-<i>Подставит текущую ссылку</i>\n<code>@adname</code>-<i>Подставит название объявления</i>\n<code>@adprice</code>-<i>Подставит цену объявления</i>\n<code>@adlocation</code>-<i>Подставит местоположение объявления</i>\n<code>@sellername</code>-<i>Подставит имя продавца</i>\n\n✔️ <b>Вставьте ключевое слово в текст и вместо него подставится нужная информация.</b>";
                        state = "WhatsappText";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: text,
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToSettings
                        );
                        return;
                    case "starter_page":
                    // СТАРТОВАЯ СТРАНИЦА
                        state = "StartPage";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>🖊 Введите стартовую страницу:</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToSettings
                        );
                        return;
                    case "black_list":
                    // ЧЕРНЫЙ СПИСОК
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: $"📎 <b>Нажмите на фильтр чтобы поменять его.</b>\n\n<b>При включении <u>Личного Чёрного Списка</u> не будут показываться объявления продавцов, которых вы уже видели.\n\nПри включении <u>Общего Чёрного Списка</u> не будут показываться объявления продавцов, которыx видели другие пользователи.\n\n🗳 Продавцов в «Личном ЧС» — {DB.LocalBlacklistLength(chatId)}\n🌐 Продавцов в «Общем ЧС» — {DB.GlobalBlacklistLength() + 8324}</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BlackListKb(chatId)
                        );
                        return;
                    case "local_bl":
                        string blackList;
                        if(DB.GetLocalAndGlobalBl(chatId)[0]=="Включить")
                        {
                            blackList = "Отключить";
                            DB.UpdateLocalBlackList(chatId, blackList);
                        }
                        else
                        {
                            blackList = "Включить";
                            DB.UpdateLocalBlackList(chatId, blackList);
                        }
                        await botClient.EditMessageReplyMarkupAsync(
                            chatId: chatId,
                            messageId: messageId,
                            replyMarkup: Keyboards.BlackListKb(chatId)
                        );
                        return;
                    case "global_bl":
                        if(DB.GetLocalAndGlobalBl(chatId)[1]=="Включить")
                        {
                            blackList = "Отключить";
                            DB.UpdateGlobalBlackList(chatId, blackList);
                        }
                        else
                        {
                            blackList = "Включить";
                            DB.UpdateGlobalBlackList(chatId, blackList);
                        }
                        await botClient.EditMessageReplyMarkupAsync(
                            chatId: chatId,
                            messageId: messageId,
                            replyMarkup: Keyboards.BlackListKb(chatId)
                        );
                        return;
                    case "token":
                    // ТОКЕН
                        state = "Token";
                        DB.UpdateState(chatId, state);
                        await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: "<b>🖊 Введите ваш токен для парсинга (без Bearer):</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: Keyboards.BackToSettings
                        );
                        return;
                    case "back_from_pars":
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

        // Оставить только цифры
        private static string leave_only_numbers(string block)
        {
            string new_block = "";
            for (int i = 0; i < block.Length; i++)
            {
                if (Char.IsDigit(block[i]))
                {
                    new_block += block[i];
                }
                else
                {
                    continue;
                }
            }
            return new_block;
        }
    }
}