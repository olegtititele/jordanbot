// using HtmlAgilityPack;
// using System.Net;
// using System.IO;

// // using System;
// // using System.Collections.Generic;
// // using System.Linq;
// // using System.Text;
// // using System.Threading.Tasks;
// // using System.Net.Http;
// // using System.Globalization;
// // using System.Net;
// // using System.Collections.Specialized;

// using PostgreSQL;

// namespace Parser
// {
//     public class EbayKleinanzeigen
//     {
//         private static int ann_cnd;
//         private static string adv_title = "Не указано";
//         private static string adv_link = "";
//         private static string adv_price = "Не указана";
//         private static string adv_image = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
//         private static string seller_name = "Не указано";
//         private static string seller_link = "Не указана";
//         private static string seller_total_ads = "Не указано";
//         private static DateTime seller_reg = DateTime.Today;
//         private static DateTime adv_reg = DateTime.Today;
//         private static string adv_business = "Частное лицо";
// 		private static string adv_location = "Не указано";
//         private static string phone_number = "";
//         private static long user_id;
//         private static string platform = "dba.dk";
//         private static int page;
//         private static string link = "";
//         private static int announ_count;
//         private static string seller_adv_count="";
//         private static string adv_reg_data = "";
// 		private static string seller_reg_data = "";
//         private static string business = "";
//         private static string local_blacklist = "";
//         private static string global_blacklist = "";
//         private static int pages_passed = 1;
//         private static int ads_passed;
//         private static int user_agent_count = 0;


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
//                 Console.WriteLine(generate_link(link, page));
//                 HtmlWeb web = new HtmlWeb();
//                 HtmlDocument document = web.Load(generate_link(link, page));
//                 var advertisements = document.DocumentNode.SelectNodes("//li[contains(@class, 'ad-listitem')]//a[@class=\"ellipsis\"]");
//                 if(advertisements != null)
//                 {
//                     foreach (HtmlNode advertisement in advertisements)
//                     {
//                         if(ann_cnd < announ_count && DB.GetState(user_id)!="StopParser")
//                         {
//                             adv_link = advertisement.GetAttributeValue("href", "");
                            
//                             Console.WriteLine(adv_link);
//                             ads_passed++;
//                             DB.UpdateStatistic(user_id, pages_passed, ads_passed);
//                             if(DB.CheckAdvestisement(user_id, adv_link))
//                             {
//                                 continue;   
//                             }
//                             else
//                             {
//                                 pars_adv_info(get_content(adv_link).Result);
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
        
//         static async Task<string> get_content(string link)
//         {

            // var values = new Dictionary<string, string>
            // {
            //     { "targetUrl", link},
            //     { "loginWenkseSessionId", "943d02d9-00d2-4f48-9c34-f54432c930de" },
            //     { "loginMail", "paykowski@hotmail.de" },
            //     { "password", "szeretlek" },
            //     { "_csrf", "c39a1b3a-c76f-480d-b31f-7394c74f4143" },
            //     { "fingerprint", "999e246df536cbf253c8704d367a2c3a" }
            // };

            // var data = new FormUrlEncodedContent(values);

            // var url = "https://www.ebay-kleinanzeigen.de/m-einloggen.html";
            // using var client = new HttpClient();
            // // create reader & open file
            
            
           
            // client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            // client.DefaultRequestHeaders.Add("Cookie", "ekConsentTcf2={%22customVersion%22:3%2C%22encodedConsentString%22:%22CPWXnL5PWXnL5E1ABADECHCgAP_AAAAAAAYgIptf_X__b3_n-_7___t0eY1f9_7__-0zjhfdt-8N3f_X_L8X_2M7vF36tr4KuR4ku3bBIQdtHOncTUmx6olVrzPsbk2cr7NKJ7Pkmnsbe2dYGH9_n93T_ZKZ7______7________________________-_____9___________________________-AAAAkVABgACCKYyADAAEEUx0AGAAIIpkIAMAAQRTJQAYAAgimUgAwABBFMJABgACCKYaADAAEEUxEAGAAIIpg.YAAAAAAAAyoA%22%2C%22googleConsentGiven%22:true%2C%22consentInterpretation%22:{%22googleAdvertisingFeaturesAllowed%22:true%2C%22googleAnalyticsAllowed%22:true%2C%22infonlineAllowed%22:true%2C%22theAdexAllowed%22:true}}; ekConsentBucketTcf2=full2-exp; _ga=GA1.2.207587028.1648141594; clientId=207587028.1648141594; overlayV21=seen; liberty=%7B%22gdprPropertiesForUser%22%3A%7B%22isGdprFullConsentGiven%22%3Atrue%7D%7D; iom_consent=0103ff03ff&1648142267565; __gads=ID=b1f1df1ee7f2f5f0:T=1648142801:S=ALNI_MYCjof_NCfgOMYwpR_i1G9rOsFcjQ; axd=4287953264151123075; __gsas=ID=d749d6b202e4c289:T=1648152304:S=ALNI_MY25suqXGh__Wj9gv1eaqcD64D6xA; uh=%7B%22sh%22%3A%22c%3D173%3A%3Aatt%3Dhandy_telekom.art_s%40apple%26p%3D2%26c%3D173%26handy_telekom.art_s%3Dapple%22%7D; route_97385225_bd99_4ddb_a1c6_c108f3714d67=b1af5d2e3ed44a24f91dacabd3b68e5e; CSRF-TOKEN=c39a1b3a-c76f-480d-b31f-7394c74f4143; GCLB=CI_4zaOas5aqZQ; AKA_A2=A; bm_sz=48ECC63C4ACE8D07F92DEB49E52F1A4D~YAAQx6EkF87x47p/AQAAa+K8zA/R5EQUbZxKW6tl7h2VnbSszwhWl1u6vr7xaXn3w0fVHXDRpeqZ7VkxwwX7/Ydhq77rIv2MQ0z/pP8arqcKRD1/ohNvBz5u08Z8EVrVFe5VbXdsXjKutsy4XVm31XTqjAil6E96tC9UsMe2XIks/SHx5u2y8vrjVSq1yrA1L+AOw2GMeCo7e81eB5ljuYmMT/WgU4S5FpiELVz4cYsv/TzL/AHR/Q9j4HueYTcDSRbpSBbGm+NbJCiupowR86smACayzXq+3nx4C+gSjoSD/Tatx5Vd/etwmzbmzw==~3485747~3293766; rbzid=lQZ1zCKT3QWFIcsZTiM0zB02SuZDgStrlLQ3TmaqB0aPjNIpq5vvZxCDlUcBg6OA3iVPvgx/bIXPAKeaRSqFtDxqjZjHQcsO0KSclnGlJZnvtu40KgYdnR22uheV31EHq8GzDXxvRA5ZUp5ET4MUDE3T2mtSKcxYzQcXUPM1h/eWDSar7c9OO+Iv63ySf0l3K0vJyq3pkWKPFo9e3cgDDtkp76D8llPK/zPkDU5LBEBOL1925UryggaPBaNEop94; rbzsessionid=030bbc9fc0b7e6bf03ac9a48d1389673; ak_bmsc=ACD63546DCB749241B0FB05150E88B2F~000000000000000000000000000000~YAAQx6EkF/Py47p/AQAAsfa8zA/3NCz2aNvR1RRzgrPwQOUzAtCINNYLxiSsJO9JReq/eYLDiY+pZFG55HlqXR3kzu3Po8tENroznCq16rhDkyQdv7rqE5/Q0AG/Uq+8tBayzHjKbekCjpJuaBmqozS6EWvo7b3ZekjzVuW0F+eLtl5mqZi1gXOp5IPOsBe4eQ30UUGtzRzwTEvJSpOzyBs2JQ+9A1Yk3CG0SfKWB2Wf9TzhlNuLFbuVXRF6S2/SWsPep+MaxMAViyRSgSDDcWs+hYJddw9RUXceRrsgRmrlCv1Fxx2kUppd3T47BAdbzAT7kTnYDJvIsYHiT1A3mvsj6gjxSrqWwqXhz1Dn1oUL9ysJxWKc7KTg6nOibLjtJfZegFXYG/RWg2foMOoyQZk1X/BNKUJfOKjp1S2aSNomYDuYGmvlboHh8puXslnQEda+TfiPkhp7ReZry/Okew6jXTKMJlZmUWVhMIKs2l8HYLuWwdlRg/6vvWTxfqdo2E0zHKjZAg==; _gid=GA1.2.1812958234.1648407413; POPUPCHECK=1648493814247; _abck=30D6D82273D573B6E9A3CF0BF42702BC~0~YAAQ2KEkF5/bGZt/AQAAKcu/zAd0yuQlpkDcVnkKWS9o5y3tAj5A6pBOA5kmO6lTiRon0BK4XmaYsQe2lwh+wpC3T7f4ImNE6lxLGWEehU3qM8m2Wkdbkcgu9X3hQvX3FoyzcJ+lj6n42GG1f0tZTIOkg2kQfzNzC0f1LG5lohKb5xqATyvhbKuYT5mehtTqWTevKe4p9gq3ezteO5LOrF03XsIFkjC2sHpP2q+BLFkBz+JhllDqWA6BArOti+WRAuAuqyyXoXflV5LD3UMTAwHD+vU7nqpsIF2gbdcXX1KnnhCNtxI7y/b8bm/D4Vzami1F+zplBZ/T2B1cJoY6eS+rddmmV3JcOl2QiIhQyyMnRzvgeQHFJPZBfIzwD4ERTFBc5rvEhk8Fd+T3dvXY5LE7XZbnxAq13MEDpc351Pq1eg==~-1~-1~1648410995; sc=%7B%22va%22%3A%222060846937%22%7D; tis=EP287%3A3122; up=%7B%22ln%22%3A%22892754934%22%2C%22lln%22%3A%2296d61d99-e185-4baa-bb73-015e8a920ea0%22%2C%22llstv%22%3A%2297-js-errorlog%3DA%7CBLN-18532_highlight%3DB%7CBLN-20749-maps_on_vip%3DB%7CBLN-18275-lazy-load-image%3DB%7CDesktop_Test%3DB%7CEKMO-25_MyAdsC2b%3DB%7Cperformance-test-desktop%3DC%7CEKMO-11%3DB%7Cga-behind-consent%3DB%7Crebranding-m2www%3DA%7Cconsent-banner%3DA%7Cdesktop_decision_engine%3DA%7CBLN-18221_srp_rebrush_ads%3DB%7CBLN-19952-set-branding%3DB%7CBLN-19787-biz-upselling%3DB%7Cprebid-update%3DA%7Cliberty_gcp_desktop%3DA%7Cgdpr-validate-cookie%3DA%7CEBAYKAD-2252_group-assign%3DB%22%2C%22ls%22%3A%22c%3D173%22%2C%22va%22%3A%222058876893%2C60025%2C56598%2C-3013014%2C-4016953%2C7037381%2C-18985373%2C19002885%2C19380%2C-4282%2C371109%2C-419286278%2C414231124%2C6497442%22%2C%22vapwrncats%22%3A2%2C%22vapwrnscrty%22%3A1%2C%22saSe%22%3A%22%22%7D; ioam2018=0019d0494a80985ee623ca7bb:1679591867595:1648142267595:.ebay-kleinanzeigen.de:46:ebaykanz:ebayK-3-173:noevent:1648410307821:70p9h6; cto_bundle=aQX7dF82R2drWVVTUG1zQXVla0wlMkJxSEwlMkZvJTJCMUdDUCUyRkRHTDZGamNyc2x2Z291JTJCJTJGclJHOWNCenhOZCUyQmROOElXS1NGQlJtVFRQZTluTjE0SHpHUWFCV2JQb213dE9qcFRFWW9iSGpJc01zYnVJdENjY2lkakdiV0NGQUZLY0V0MTRmTVJka3pPaWFwT09XMTFsZm5BUVBYJTJGMnJNQ3RRJTJCeE0wTkRsaUhBOUpSTmliTkUlM0Q; bm_sv=372EE27F450B8E138DC3E3BC76C69EB0~A2k0goixFXs1uvTRIaj5PniXHPWsIcrsMtMVxoeDvfJtovioxJJL27xUZQzrZcBF71ECgqCRzgPkUkVgY95GBtbBcNIBEUmotfw1y0cOa+Tk0LY96woFiLCQeplpsTOT5rWs3dubJcklB4ux0+CHhVQNKC3F0/fMSpkEVltNJVc=; RT=\"z=1&dm=www.ebay-kleinanzeigen.de&si=347d2769-6fd0-4257-8324-fb58a0030ee3&ss=l19oxfqf&sl=3&tt=4pq&obo=1&rl=1&nu=3dduqri2&cl=1l2p&ld=1l32&r=a1tgp0e0&ul=1l34\"");
            // var response = await client.PostAsync(url, data);
            // var content = await response.Content.ReadAsStringAsync();
            // return await Task.FromResult(content);
//         }

//         static void pars_adv_info(string content)
//         {
//             var htmlDoc = new HtmlDocument();
//             htmlDoc.LoadHtml(content);
//             Console.WriteLine(content);
//             // Получить номер телефона
//             try
//             {
//                 phone_number = "+49" + htmlDoc.DocumentNode.SelectSingleNode("//span[@id=\"viewad-contact-phone\"]//a").InnerText.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);
//                 if(check_blacklist_ads(phone_number)){ }else{ return; }
//             }
//             catch(Exception ){ return; }

//             // Название объявления
//             try
//             {
//                 adv_title = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'contentbox--vip')]//h1[@id=\"viewad-title\"]").InnerText.Trim();
//             }
//             catch (Exception){ }

//             // Цена объявления
//             try
//             {
//                 adv_price = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'contentbox--vip')]//h2[@class=\"boxedarticle--price\"]").InnerText.Trim().Split(" ")[0];
//             }
//             catch (Exception){ }

//             // Ссылка на изображение
//             try
//             {
//                 adv_image = adv_image = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'galleryimage-element current')]//img").GetAttributeValue("src", "");
//             }
//             catch (Exception)
//             {
//                 adv_image = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
//             }

//             // Дата регистрации объявления
//             try
//             {
//                 adv_reg = Convert.ToDateTime(htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"viewad-extra-info\"]").InnerText.Trim());
            
//             }
//             catch (Exception){ }

//             // Имя и ссылка на продавца
//             try
//             {
//                 seller_name = htmlDoc.DocumentNode.SelectSingleNode("//span[@class=\"text-body-regular-strong text-force-linebreak\"]").InnerText.Trim();
//                 seller_link = "https://www.ebay-kleinanzeigen.de" + htmlDoc.DocumentNode.SelectSingleNode("//span[@class=\"text-body-regular-strong text-force-linebreak\"]//a").GetAttributeValue("href", "");
//             }
//             catch (Exception){ }
            
//             // Дата регистрации продавца
//             try
//             {
//                 seller_reg = Convert.ToDateTime(htmlDoc.DocumentNode.SelectSingleNode("//span[@class=\"text-body-regular text-light\"]").InnerText.Trim().Split(" ")[^1]);

                
//             }
//             catch (Exception){ }
            
//             try
//             {
//                 seller_total_ads = htmlDoc.DocumentNode.SelectSingleNode("//a[@id=\"poster-other-ads-link\"]").InnerText.Trim().Split(" ")[0];

                
//             }
//             catch (Exception){ }

//             // Местоположение
//             try
//             {
//                 adv_location = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'contentbox--vip')]//span[@id=\"viewad-locality\"]").InnerText.Trim();
//             }
//             catch (Exception){ }  

//             // Проверка типа объявления
//             try
//             {
//                 adv_business = htmlDoc.DocumentNode.SelectSingleNode("//span[@class=\"text-body-regular text-light\"]").InnerText.Trim();
//                 if(adv_business.Contains("Privater"))
//                 {
//                     adv_business = "Частное лицо";
//                 }
//                 else
//                 {
//                     adv_business = "Бизнесс аккаунт";
//                 }
//             }
//             catch (Exception){ }

//             check_type_acc();

//             Console.WriteLine(phone_number);
//             Console.WriteLine(adv_title);
//             Console.WriteLine(adv_price);
//             Console.WriteLine(adv_image);
//             Console.WriteLine(seller_name);
//             Console.WriteLine(seller_link);
//             Console.WriteLine(adv_location);
//             Console.WriteLine(adv_business);
//             Console.WriteLine(adv_reg);
//             Console.WriteLine(seller_reg);
//             Console.WriteLine(seller_total_ads);


//             // check_type_acc();
//         }

//         // Сгенерировать ссылку
//         static string generate_link(string link, int page)
//         {
//             // https://www.ebay-kleinanzeigen.de/s-seite:2/iphone/k0
//             string page_link;
//             if(link.Contains("ebay-kleinanzeigen.de"))
//             {
//                 if(link.Contains("m."))
//                 {
//                     link = link.Replace("m.ebay-kleinanzeigen.de", "www.ebay-kleinanzeigen.de");
//                 }
//                 string last_part = link.Split("/")[^1];
//                 page_link = link.Replace(last_part, string.Empty) + "seite:" + page.ToString() + "/" + last_part;
                
//             }
//             else
//             {
//                 page_link = "https://www.ebay-kleinanzeigen.de/s-seite:" + page.ToString() + "/" + link + "/k0";
//             }
//             return page_link;
//         }

//         static bool check_blacklist_ads(string phone_number)
//         {
//             if(global_blacklist == "Отключить")
//             {
//                 if(local_blacklist == "Отключить")
//                 {
//                     return true;
//                 }
//                 else
//                 {
//                     if(DB.CheckPhoneNumber(user_id, phone_number))
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
//                 if(DB.CheckBlackListSellerPhone(user_id, phone_number))
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
//                         if(DB.CheckPhoneNumber(user_id, phone_number))
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

//         // Проверка типа аккаунта
//         static void check_type_acc()
//         {
//             if(business == "Частное лицо")
//             {
//                 if(adv_business == "Частное лицо")
//                 {
//                     check_adv_reg_data();
//                 }
//                 else if(adv_business == "Бизнесс аккаунт")
//                 {
//                     return;
//                 }
//             }
//             else if(business == "Все типы аккаунтов")
//             {
//                 check_adv_reg_data();
//             }
//         }

//         // Проверка даты регистрации объявления
//         static void check_adv_reg_data()
//         {
//             if(adv_reg_data == "Отключить")
//             {
//                 check_seller_reg_data();
//             }
//             else
//             {
//                 DateTime d = Convert.ToDateTime(adv_reg_data);
//                 if(d < adv_reg)
//                 {
//                     check_seller_reg_data();
//                 }
//             }
//         }

//         // Проверка даты регистрации продавца
//         static void check_seller_reg_data()
//         {
//             adv_link = "https://www.ebay-kleinanzeigen.de" + adv_link;
//             if(seller_reg_data.Contains("Отключить"))
//             {
//                 try
//                 {
//                     DB.AddAdvertisementToHash(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads, seller_reg.ToString("d"), adv_business);
//                     DB.AddAdvertisementToMain(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads, seller_reg.ToString("d"), adv_business);
//                     DB.AddAdvertisemtToBlackList(platform, seller_link, phone_number);
//                     ann_cnd++;
//                 }
//                 catch(Exception){ } 
//             }
//             else
//             {
//                 DateTime d = DateTime.ParseExact(seller_reg_data, "dd.MM.yyyy", null);
//                 if(d < seller_reg)
//                 {
//                     try
//                     {
//                         DB.AddAdvertisementToHash(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                         DB.AddAdvertisementToMain(user_id, platform, adv_title, adv_price, adv_reg.ToString("d"), adv_link, adv_location, adv_image, seller_name, seller_link, phone_number, seller_total_ads.ToString(), seller_reg.ToString("d"), adv_business);
//                         DB.AddAdvertisemtToBlackList(platform, seller_link, phone_number);
//                         ann_cnd++;
//                     }
//                     catch(Exception){ }
//                 }
//             }
//         }
//     }
// }