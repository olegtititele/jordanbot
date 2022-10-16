using ConfigFile;


namespace Modules
{
    public static class CountryLink
    {      
        public static string GetCountryLink(string country)
        {
            string text = "";
            
            switch (country)
            {
                case "homeless.co.il":
                    text = $"<b>📒 Введите вашу ссылку с категорией. Обязательно вводите ссылку без указания страницы.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.homeless_coil_link}";
                    break;
                case "gumtree.co.za":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.gumtree_coza_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "dba.dk":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.dba_dk_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "ebay-kleinanzeigen.de":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.ebay_kleinanzeigen_de_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "bolha.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.bolha_com_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "sbazar.cz":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.sbazar_cz_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "kijiji.it":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.kijiji_it_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "jofogas.hu":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.jofogas_hu_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "oglasnik.hr":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.oglasnik_hr_link}";
                    break;
                case "tutti.ch":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.tutti_ch_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "bazar.bg":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.bazar_bg_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "quoka.de":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.quoka_de_link}";
                    break;
                case "anibis.ch":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.anibis_ch_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "gumtree.uk":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.gumtree_uk_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "guloggratis.dk":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.guloggratis_dk_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "bazar.sk":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.bazar_sk_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "bazos.pl":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.bazos_pl_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.qa":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_qa_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.com.om":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_com_om_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.pt":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_pt_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.pl":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_pl_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.ro":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_ro_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "olx.bg":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.olx_bg_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "list.am":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.list_am_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "kijiji.ca":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.kijiji_ca_link}";
                    break;
                case "lajumate.ro":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.lajumate_ro_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
                case "om.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.om_opensooq_com_link}";
                    break;
                case "ye.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.ye_opensooq_com_link}";
                    break;
                case "ae.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.ae_opensooq_com_link}";
                    break;
                case "iq.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.iq_opensooq_com_link}";
                    break;
                case "eg.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.eg_opensooq_com_link}";
                    break;
                case "lb.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.lb_opensooq_com_link}";
                    break;
                case "ly.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.ly_opensooq_com_link}";
                    break;
                case "sa.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.sa_opensooq_com_link}";
                    break;
                case "sd.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.sd_opensooq_com_link}";
                    break;
                case "bh.opensooq.com":
                    text = $"<b>📒 Введите вашу ссылку с категорией.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.bh_opensooq_com_link}";
                    break;
                case "kuldnebors.ee":
                    text = $"<b>📒 Введите вашу ссылку с категорией или ключевое слово.</b>\n\n<b>✔️ Пример ссылки: </b>{Links.kuldnebors_ee_link}\n\n<b>✔️ Пример слова:</b> <u>apple</u>";
                    break;
            }
            return text;
        }
    }
}