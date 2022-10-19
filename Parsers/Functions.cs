using PostgreSQL;
using System.Globalization;


namespace Parser
{
    static class Functions
    {
        public static CultureInfo culture = CultureInfo.GetCultureInfo("de-DE");
        // Форматировать номер телефона
        public static string convert_phone(string phone_block)
        {
            string phone = "";
            for (int i = 0; i < phone_block.Length; i++)
            {
                if (Char.IsDigit(phone_block[i]))
                {
                    phone += phone_block[i];
                }
                else if(phone_block[i] == '+')
                {
                    phone += phone_block[i];
                }
                else
                {
                    continue;
                }
            }
            return phone;
        }

        // Форматировать цену
        public static string convert_price(string price_block, string currency)
        {
            string price = "";
            for (int i = 0; i < price_block.Length; i++)
            {
                if (Char.IsDigit(price_block[i]))
                {
                    price += price_block[i];
                }
                else if(price_block[i] == ',' || price_block[i] == '.' || price_block[i] == '\'')
                {
                    price += price_block[i];
                }
                else
                {
                    continue;
                }
            }
            return $"{price} {currency}";
        }

        // Оставить только цифры
        public static int leave_only_numbers(string adv_block)
        {
            string adv_string = "";
            for (int i = 0; i < adv_block.Length; i++)
            {
                if (Char.IsDigit(adv_block[i]))
                {
                    adv_string += adv_block[i];
                }
                else
                {
                    continue;
                }
            }
            return Int32.Parse(adv_string);
        }

        // Проверка номера телефона
        public static bool check_blacklist_ads(long user_id, string phone_number, string global_blacklist, string local_blacklist)
        {
            if(global_blacklist == "Отключить")
            {
                if(local_blacklist == "Отключить")
                {
                    return true;
                }
                else
                {
                    if(DB.CheckPhoneNumber(user_id, phone_number))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                if(DB.CheckBlackListSellerPhone(user_id, phone_number))
                {
                    return false;
                }
                else
                {
                    if(local_blacklist == "Отключить")
                    {
                        return true;
                    }
                    else
                    {
                        if(DB.CheckPhoneNumber(user_id, phone_number))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }

        // Проверка ссылки продавца
        public static bool CheckIfSellerLinkNotExists(long userId, string sellerLink, string globalBlacklist, string localBlacklist)
        {
            if(globalBlacklist == "Отключить")
            {
                if(localBlacklist == "Отключить")
                {
                    return true;
                }
                else
                {
                    if(DB.CheckSellerlink(userId, sellerLink))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                if(DB.CheckBlackListSellerLink(userId, sellerLink))
                {
                    return false;
                }
                else
                {
                    if(localBlacklist == "Отключить")
                    {
                        return true;
                    }
                    else
                    {
                        if(DB.CheckSellerlink(userId, sellerLink))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }

        // Проверка типа аккаунта
        public static bool check_type_acc(string input_acc_type, string parsed_acc_type)
        {
            if(input_acc_type == "Частное лицо")
            {
                if(parsed_acc_type == "Частное лицо")
                {
                    return true;
                }
                else if(parsed_acc_type == "Бизнесс аккаунт")
                {
                    return false;
                }
                return false;
            }
            else if(input_acc_type == "Все типы аккаунтов")
            {
                return true;
            }
            return false;
        }

        // Проверка количества объявлений продавца
        public static bool check_seller_adv_count(string input_seller_adv_count, int parsed_seller_adv_count)
        {
            if(input_seller_adv_count == "Отключить")
            {
                return true;
            }  
            else
            {
                if(parsed_seller_adv_count < Int32.Parse(input_seller_adv_count))
                {
                    return true;
                }
                else{return false;}
            }
        }

        // Проверка даты регистрации объявления
        public static bool check_adv_reg_data(string input_adv_reg_data, DateTime parsed_adv_reg_data)
        {
            if(input_adv_reg_data == "Отключить")
            {
                return true;
            }
            else
            {
                DateTime d = Convert.ToDateTime(input_adv_reg_data);
                if(d <= parsed_adv_reg_data)
                {
                    return true;
                }
                else{return false;}
            }
        }
        // Проверка даты регистрации продавца
        public static bool check_seller_reg_data(string input_seller_reg_data, DateTime parsed_seller_reg_data)
        {
            if(input_seller_reg_data == "Отключить")
            {
                return true;
            }
            else
            {
                DateTime d = Convert.ToDateTime(input_seller_reg_data);
                if(d <= parsed_seller_reg_data)
                {
                    return true;
                }
                else{return false;}
            }
        }

        public static bool CheckSellerRating(decimal inputSellerRating, decimal parsedSellerRating)
        {
            if(parsedSellerRating <= inputSellerRating)
            {
                return true;
            }
            return false;
        }

        // Добавить объявление в бд
        public static void InsertNewAd(long userId, string platform, string adTitle, string adPrice, string adRegDate, string adLink, string adLocation, string adImage, string sellerName, string sellerLink, string phoneNumber, string sellerTotalAds, string sellerRegDate, string sellerType, string sellerRating, string globalBlacklist)
        {
            try
            {
                adTitle = adTitle.Replace(">", "").Replace("<", "").Replace('"', '`');
                DB.AddAdvertisementToHash(userId, platform, adTitle, adPrice, adRegDate, adLink, adLocation, adImage, sellerName, sellerLink, phoneNumber, sellerTotalAds, sellerRegDate, sellerType, sellerRating);
                DB.AddAdvertisementToMain(userId, platform, adTitle, adPrice, adRegDate, adLink, adLocation, adImage, sellerName, sellerLink, phoneNumber, sellerTotalAds, sellerRegDate, sellerType, sellerRating);


                if(globalBlacklist=="Включить")
                {
                    DB.AddAdvertisemtToBlackList(platform, sellerLink, phoneNumber);
                }
            }
            catch(Exception e){ Console.WriteLine(e); return; }
        }
    }
}
