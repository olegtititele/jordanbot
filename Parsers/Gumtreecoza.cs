// using HtmlAgilityPack;

// using System.Globalization;

// using PostgreSQL;

// namespace Parser
// {
//     public class GumtreeCoZa
//     {
//         private static int ann_cnd;
//         private static string adv_title = "Не указано";
//         private static string adv_link = "";
//         private static string adv_price = "Не указана";
//         private static string adv_image = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
//         private static string seller_name = "";
//         private static string seller_link = "";
//         private static float seller_total_ads = 1;
//         private static DateTime seller_reg = DateTime.Today;
//         private static DateTime adv_reg = DateTime.Today;
//         private static string adv_business = "Не указано";
// 		private static string adv_location = "Не указана";
//         private static string phone_number="Не указан";
//         private static long user_id;
//         private static string platform="";
//         private static int page;
//         private static string link="";
//         private static int announ_count;      
//         private static string seller_adv_count="";
//         private static string adv_reg_data="";
// 		private static string seller_reg_data="";
//         private static string business="";
//         private static string local_blacklist = "";
//         private static string global_blacklist = "";
//         private static int pages_passed = 1;
//         private static int ads_passed;

//         public static int start_parsing(long usid)
//         {
//             user_id = usid;
//             ann_cnd = 0;
//             var parameters = DB.GetAllParameters(user_id);
//             platform = parameters[0];
//             link = parameters[1];
//             announ_count = Int32.Parse(parameters[2]);
//             seller_adv_count = parameters[3];
//             seller_reg_data = parameters[4];
//             adv_reg_data = parameters[5];
//             business = parameters[6];
//             page = Int32.Parse(parameters[7]);
//             local_blacklist = parameters[8];
//             global_blacklist = parameters[9];
//             pages_passed = 1;
//             ads_passed = 0;

//             while(true)
//             {
//                 HtmlWeb web = new HtmlWeb();
//                 HtmlDocument document = web.Load(generate_link(link, page));
//                 if(DB.GetState(user_id)=="StopParser")
//                 {
//                     DB.UpdateStatistic(user_id, pages_passed, ads_passed);
//                     return 1;
//                 }
//                 var advertisements = document.DocumentNode.SelectNodes("//a[contains(@class, 'related-ad-title')]");
//                 if (advertisements != null)
//                 {
//                     foreach (HtmlNode advertisement in advertisements)
//                     {
//                         if(ann_cnd < announ_count)
//                         {
//                             adv_link = "https://www.gumtree.co.za" + advertisement.GetAttributeValue("href", "");
//                             ads_passed++;
                            
//                             if(DB.GetState(user_id)=="StopParser")
//                             {
//                                 DB.UpdateStatistic(user_id, pages_passed, ads_passed);
//                                 return 1;
//                             }
//                             else
//                             {
//                                 if(DB.CheckAdvestisement(user_id, adv_link))
//                                 {
//                                     HtmlDocument adv_html = web.Load(adv_link);
//                                     pars_adv_info(adv_html);  
//                                 }
//                                 else
//                                 {
//                                     continue;
//                                 }
//                             }

//                         }
//                         else
//                         {
//                             string state = "StopParser";
//                             DB.UpdateState(user_id, state);
//                             DB.UpdateStatistic(user_id, pages_passed, ads_passed);
//                             return 1;
//                         }
//                     }    
//                 }
//                 else
//                 {
//                     string state = "StopParser";
//                     DB.UpdateState(user_id, state);
//                     DB.UpdateStatistic(user_id, pages_passed, ads_passed);
//                     return 1;
//                 }
//                 pages_passed++;
//                 page++;
//             }
//         }


//         static void pars_adv_info(HtmlDocument adv_html)
//         {
//             // Проверка на наличие номера телефона 
//             try
//             {
//                 var phone_number_block = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'seller-phone')]").InnerText;
//             }
//             catch (Exception)
//             {
//                 return;
//             }
            
//             // Название объявления
//             try
//             {
//                 adv_title = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-container ad-info')]//div[contains(@class, 'title')]").InnerText;
//             }
//             catch (Exception){ }

//             // Цена объявления
//             try
//             {
//                 adv_price = convert_price(adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-container ad-info')]//span[contains(@class, 'ad-price')]").InnerText);
//             }
//             catch (Exception){ }

//             // Ссылка на изображение
//             try
//             {
//                 adv_image = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-container ad-info')]//img[contains(@class, 'lazyloaded')]").GetAttributeValue("src", "");
//             }
//             catch (Exception){ }

//             // Дата регистрации объявления
//             try
//             {
//                 var adv_reg_data_block = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-container ad-info')]//span[contains(@class, 'creation-date')]").InnerText;
//                 adv_reg = get_data(adv_reg_data_block);
//             }
//             catch (Exception){ }
            
//             // Имя и ссылка на продавца
//             try
//             {
//                 seller_name = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-seller-reply-area')]//div[contains(@class, 'seller-name')]").InnerText.Replace('\'', '`');;
//                 seller_link = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-seller-reply-area')]//a[contains(@class, 'seller-link')]").GetAttributeValue("href", "");
//                 if(check_blacklist_ads(seller_link)){ }else{ return; }
//             }
//             catch (Exception)
//             {
//                 return;
//             }

//             // Дата регистрации продавца
//             try
//             {
//                 var seller_reg_block = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'vip-seller-reply-area')]//span[contains(@class, 'color-green')]").InnerText;
//                 seller_reg = get_data(seller_reg_block);

                
//             }
//             catch (Exception){ }
            
//             // Количество объявлений продавца
//             try
//             {
//                 var seller_total_ads_block = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'seller-header')]//span[contains(@class, 'active-ad')]//span[contains(@class, 'color-green')]").InnerText;
//                 if(seller_total_ads_block.Contains("K"))
//                 {
//                     seller_total_ads = float.Parse(seller_total_ads_block.Split("K")[0], CultureInfo.InvariantCulture.NumberFormat)*1000;
//                 }
//                 else
//                 {
//                     seller_total_ads = float.Parse(seller_total_ads_block, CultureInfo.InvariantCulture.NumberFormat);
//                 }
                
//             }
//             catch (Exception)
//             { 
//                 return;
//             }

//             // Местоположение и тип объявления
//             try
//             {
//                 var adv_general_details = adv_html.DocumentNode.SelectNodes("//div[contains(@class, 'vip-container ad-info')]//div[contains(@class, 'attributes')]//div[contains(@class, 'attribute')]");
//                 foreach(var agd in adv_general_details)
//                 {
//                     if(agd.InnerText.Contains("Location:"))
//                     {
//                         adv_location = agd.InnerText.Split("Location:")[1].Replace('\'', '`');
//                     }
//                     else if(agd.InnerText.Contains("For Sale By:"))
//                     {
//                         adv_business = agd.InnerText.Split("For Sale By:")[1];
//                         if(adv_business.Contains("Dealer") || adv_business.Contains("Agency"))
//                         {
//                             adv_business = "Бизнесс аккаунт";
//                         }
//                         else
//                         {
//                             adv_business = "Частное лицо";
//                         }
//                     }
//                 }
//             }
//             catch (Exception){ }


//             check_type_acc();
//         }


//         // Сгенерировать ссылку
//         static string generate_link(string link, int page)
//         {
//             string page_link;
//             if(link.Contains("https://www.gumtree.co.za/"))
//             {
//                 string first_link = link.Split("/")[^1];
//                 string second_link = first_link.Split("p")[0];
//                 page_link = link.Split(second_link)[0] + second_link + "p" + page.ToString();
//                 return page_link;
//             }
//             else
//             {
//                 page_link = "https://www.gumtree.co.za/s-all-the-ads/v1b0p" + page.ToString() + "?q=" + link;
//                 return page_link;
//             }
//         }

//         static bool check_blacklist_ads(string slink)
//         {
//             if(global_blacklist == "Отключить")
//             {
//                 if(local_blacklist == "Отключить")
//                 {
//                     return true;
//                 }
//                 else
//                 {
//                     if(DB.CheckUserlink(user_id, slink))
//                     {
//                         return false;
//                     }
//                     else
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else
//             {
//                 if(DB.CheckBlackListSellerLink(user_id, slink))
//                 {
//                     return false;
//                 }
//                 else
//                 {
//                     if(local_blacklist == "Отключить")
//                     {
//                         return true;
//                     }
//                     else
//                     {
//                         if(DB.CheckUserlink(user_id, slink))
//                         {
//                             return false;
//                         }
//                         else
//                         {
//                             return true;
//                         }
//                     }
//                 }
//             }
//         }
//         // Форматировать цену
//         static string convert_price(string price_block)
//         {
//             string price = "";
//             for (int i = 0; i < price_block.Length; i++)
//             {
//                 if (Char.IsDigit(price_block[i]))
//                 {
//                     price += price_block[i];
//                 }
//                 else if(Char.IsPunctuation(price_block[i]))
//                 {
//                     price += price_block[i];
//                 }
//                 else
//                 {
//                     continue;
//                 }
//             }
//             return price;
//         }    

//         // Форматировать время
//         static DateTime get_data(string data_block)
//         {
//             string reg_data_digit = "";
//             for (int i = 0; i < data_block.Length; i++)
//             {
//                 if (Char.IsLetter(data_block, i))
//                     continue;
//                 else if (Char.IsDigit(data_block[i]))
//                     reg_data_digit += data_block[i];
//                 else 
//                     continue;
//             }

//             if(data_block.Contains("day"))
//             {
//                 DateTime reg_data_full = DateTime.Today.AddDays(-Int32.Parse(reg_data_digit));
//                 return reg_data_full;
//             }
//             else if(data_block.Contains("month"))
//             {
//                 DateTime reg_data_full = DateTime.Today.AddDays(-Int32.Parse(reg_data_digit)*30);
//                 return reg_data_full;
//             }
//             else if(data_block.Contains("year"))
//             {
//                 DateTime reg_data_full = DateTime.Today.AddDays(-Int32.Parse(reg_data_digit)*365);
//                 return reg_data_full;
//             }
//             else
//             {
//                 DateTime reg_data_full = DateTime.Today;
//                 return reg_data_full;
//             }
//         }

//         // Проверка типа аккаунта
//         static void check_type_acc()
//         {
//             if(business.Contains("Частное лицо"))
//             {
//                 if(adv_business.Contains("Частное лицо"))
//                 {
//                     check_seller_adv_count();
//                 }
//                 else if(adv_business.Contains("Бизнесс аккаунт"))
//                 {
//                     return;
//                 }
//             }
//             else if(business.Contains("Все типы аккаунтов"))
//             {
//                 check_seller_adv_count();
//             }
//         }


//         // Проверка количества объявлений
//         static void check_seller_adv_count()
//         {
//             if(seller_adv_count.Contains("Отключить") )
//             {
//                 check_adv_reg_data();
//             }  
//             else
//             {
//                 if(seller_total_ads < Int32.Parse(seller_adv_count))
//                 {
//                     check_adv_reg_data();
//                 }
//             }
//         }

//         // Проверка даты регистрации объявления
//         static void check_adv_reg_data()
//         {
//             if(adv_reg_data.Contains("Отключить"))
//             {
//                 check_seller_reg_data();
//             }
//             else
//             {
//                 DateTime d = DateTime.ParseExact(adv_reg_data, "dd.MM.yyyy", null);
//                 if(d < adv_reg)
//                 {
//                     check_seller_reg_data();
//                 }
//             }
//         }

//         // Проверка даты регистрации продавца
//         static void check_seller_reg_data()
//         {
//             if(seller_reg_data.Contains("Отключить"))
//             {
                
//                 try
//                 {
//                     DB.AddAdvertisementToHash(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                     DB.AddAdvertisementToMain(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                     DB.AddAdvertisemtToBlackList(platform, seller_link, phone_number);
//                     ann_cnd++;
//                 }
//                 catch(Exception exception)
//                 {
//                     Console.WriteLine(exception.StackTrace);
//                 }
                
                
//             }
//             else
//             {
//                 DateTime d = DateTime.ParseExact(seller_reg_data, "dd.MM.yyyy", null);
//                 if(d < seller_reg)
//                 {
//                     Console.WriteLine(adv_link);
//                     try
//                     {
//                         DB.AddAdvertisementToHash(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                         DB.AddAdvertisementToMain(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                         DB.AddAdvertisemtToBlackList(platform, seller_link, phone_number);
//                         ann_cnd++;
//                     }
//                     catch(Exception exception)
//                     {
//                         Console.WriteLine(exception.StackTrace);
//                     }
                    
//                 }
//             }
//         }
       
//     }
// }