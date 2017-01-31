using System;
using System.Configuration;
using System.Data.SqlClient;
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

        internal static Item parseItem(SqlDataReader reader)
        {
            Item item;
            switch (((string)reader["Type"])[0])
            {
                case 'W':
                    item = new Weapon((int)reader["ItemID"],
                                      (string)reader["Name"],
                                      ((string)reader["Type"])[0],
                                      (byte)reader["Level"],
                                      (int)reader["ValueBuy"],
                                      (int)reader["ValueSell"],
                                      (short)reader["AttackMin"],
                                      (short)reader["AttackMax"],
                                      (byte)reader["Critical"],
                                      (byte)reader["Strength"],
                                      (byte)reader["Dexterity"],
                                      (byte)reader["Stamina"],
                                      (byte)reader["Sense"]);
                    break;
                case 'A':
                    item = new Armor((int)reader["ItemID"],
                                     (string)reader["Name"],
                                     ((string)reader["Type"])[0],
                                     ((string)reader["SubType"])[0],
                                     (byte)reader["Level"],
                                     (int)reader["ValueBuy"],
                                     (int)reader["ValueSell"],
                                     (short)reader["Defense"],
                                     (byte)reader["Strength"],
                                     (byte)reader["Dexterity"],
                                     (byte)reader["Stamina"],
                                     (byte)reader["Sense"]);
                    break;
                case 'P':
                    item = new Potion((int)reader["ItemID"],
                                      (string)reader["Name"],
                                      ((string)reader["Type"])[0],
                                      (byte)reader["Level"],
                                      (int)reader["ValueBuy"],
                                      (int)reader["ValueSell"],
                                      (short)reader["Heal"]);
                    break;
                default:
                    item = new Item((int)reader["ItemID"],
                                    (string)reader["Name"],
                                    ((string)reader["Type"])[0],
                                    (byte)reader["Level"],
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

        /*private static SqlConnection getConnection()
        {
            return new SqlConnection("Data Source=RPG.sqlite;Version=3;");
        }*/

        internal static SqlConnection getConnection()
        {
            Uri uri = new Uri(ConfigurationManager.AppSettings["SQLSERVER_URI"]);
            string connectionString = new SqlConnectionStringBuilder
            {
                DataSource = uri.Host,
                InitialCatalog = uri.AbsolutePath.Trim('/'),
                UserID = uri.UserInfo.Split(':').First(),
                Password = uri.UserInfo.Split(':').Last(),
            }.ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
