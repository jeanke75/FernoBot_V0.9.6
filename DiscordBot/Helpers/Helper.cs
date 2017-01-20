using System;
using System.Data.SQLite;
using System.Linq;
using Discord;
using DiscordBot.Model;

namespace DiscordBot.Helpers
{
    static class Helper
    {
        internal static Random rng = new Random();
        internal static string getDiscordDisplayName(User user) {
            return (user.Nickname != null ? user.Nickname : user.Name);
        }

        internal static User getUserFromMention(Server server, string mention)
        {
            if (mention == "") return null;
            try
            {
                return server.FindUsers(mention).First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        internal static Item parseItem(SQLiteDataReader reader)
        {
            Item item;
            switch ((string)reader["Type"])
            {
                case "W":
                    item = new Weapon((long)reader["ID"],
                                      (string)reader["Name"],
                                      (string)reader["Type"],
                                      (int)reader["Level"],
                                      (int)reader["ValueBuy"],
                                      (int)reader["ValueSell"],
                                      (int)reader["AttackMin"],
                                      (int)reader["AttackMax"],
                                      (int)reader["Critical"],
                                      (int)reader["Strength"],
                                      (int)reader["Dexterity"],
                                      (int)reader["Stamina"],
                                      (int)reader["Sense"]);
                    break;
                case "A":
                    item = new Armor((long)reader["ID"],
                                     (string)reader["Name"],
                                     (string)reader["Type"],
                                     (string)reader["SubType"],
                                     (int)reader["Level"],
                                     (int)reader["ValueBuy"],
                                     (int)reader["ValueSell"],
                                     (int)reader["Defense"],
                                     (int)reader["Strength"],
                                     (int)reader["Dexterity"],
                                     (int)reader["Stamina"],
                                     (int)reader["Sense"]);
                    break;
                case "P":
                    item = new Potion((long)reader["ID"],
                                     (string)reader["Name"],
                                     (string)reader["Type"],
                                     (int)reader["Level"],
                                     (int)reader["ValueBuy"],
                                     (int)reader["ValueSell"],
                                     (int)reader["Heal"]);
                    break;
                default:
                    item = new Item((long)reader["ID"],
                                     (string)reader["Name"],
                                     (string)reader["Type"],
                                     (int)reader["Level"],
                                     (int)reader["ValueBuy"],
                                     (int)reader["ValueSell"]);
                    break;
            }
            return item;
        }

        internal static DateTime StringToDateTime(string datetime)
        {
            return DateTime.ParseExact(datetime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
        }

        internal static string DateTimeToString(DateTime datetime)
        {
            return datetime.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
