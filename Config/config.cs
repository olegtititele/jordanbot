using System.Globalization;
using PostgreSQL;
using ProjectFunctions;

namespace ConfigFile
{
    public class Config
    {
        public static string Bot_Token = "5674528375:AAHim3cCNR0_5ve9TGQtRlK9vhwC0DBRBFA";
        public static string menuPhoto = "background.jpg";
        public static string warningPhoto = "warning.jpg";       
        public static string errorMessage = "Произошла неизвестная ошибка. Попробуйте еще раз.";
        public static long workChat = -1001732715528;
        public static string workChatLink = "https://t.me/chimes_rebot";
        public static long logsChat = -1001787694544;
        public static long[] adminChatsId = {5740336806, 5370091665};
        public static CultureInfo culture = new CultureInfo("ru-RU", false);
        public static string[] Countries = {"dba.dk", "homeless.co.il", "gumtree.co.za", "ebay-kleinanzeigen.de", "bolha.com", "olx.pt", "olx.pl", "olx.ro", "olx.bg", "sbazar.cz", "kijiji.it", "jofogas.hu", "oglasnik.hr", "tutti.ch", "bazar.bg", "quoka.de", "anibis.ch", "gumtree.uk", "guloggratis.dk", "bazar.sk", "bazos.pl", "olx.qa", "olx.com.om", "list.am", "kijiji.ca", "lajumate.ro", "om.opensooq.com", "ye.opensooq.com", "ae.opensooq.com", "iq.opensooq.com", "eg.opensooq.com", "lb.opensooq.com", "ly.opensooq.com", "sa.opensooq.com", "sd.opensooq.com", "bh.opensooq.com", "kuldnebors.ee"};
    }
}
