using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Helpers;
using DiscordBot.Model;

namespace DiscordBot.Data
{
    static class RPGDataHelper
    {
        private static SQLiteConnection getConnection()
        {
            return new SQLiteConnection("Data Source=RPG.sqlite;Version=3;");
        }

        #region Cooldowns
        internal async static Task<DateTime> GetTimeCommandUsed(ulong id, string command)
        {
            DateTime lastUse = new DateTime(0);
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select {0} from Cooldowns where User = @user", command);
                        cmd.Parameters.Add("@user", DbType.UInt64).Value = id;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                lastUse = Helper.StringToDateTime((string)reader[command]);
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return lastUse;
        }

        internal async static Task SetTimeCommandUsed(ulong id, string command)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = string.Format("Update Cooldowns set {0} = {1} where User = @user", command, Helper.DateTimeToString(DateTime.Now));
                            cmd.Parameters.Add("@user", DbType.UInt64).Value = id;
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        internal async static Task<bool> UserStartedAdventure(ulong userId)
        {
            bool status = true;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select 1 from Stats where User = {0}", userId);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) status = false;
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return status;
        }

        internal async static Task Create(ulong userId)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            
                            cmd.CommandText = string.Format("insert into Stats (User, Health, Level, Experience, Strength, Dexterity, Stamina, Luck, Gold) " +
                                                            "values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", userId, 72, 1, 0, 0, 0, 0, 0, 3000);
                            await cmd.ExecuteNonQueryAsync();
                            // give weapon
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 2, 1);
                            await cmd.ExecuteNonQueryAsync();
                            // give helmet
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 13, 1);
                            await cmd.ExecuteNonQueryAsync();
                            // give upper
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 23, 1);
                            await cmd.ExecuteNonQueryAsync();
                            // give pants
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 33, 1);
                            await cmd.ExecuteNonQueryAsync();
                            //give boots
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 43, 1);
                            await cmd.ExecuteNonQueryAsync();
                            // give gauntlets
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values ({0}, {1}, {2})", userId, 53, 1);
                            await cmd.ExecuteNonQueryAsync();
                            // equip gear
                            cmd.CommandText = string.Format("insert into Equipped (User, HelmetID, UpperID, PantsID, BootsID, GloveID, MantleID, ShieldID, WeaponID) " +
                                                            "values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", userId, 13, 23, 33, 43, 53, 0, 0, 2);
                            await cmd.ExecuteNonQueryAsync();

                            //create cooldowns
                            cmd.CommandText = string.Format("insert into Cooldowns(User, Start, Stats, Assign, Inventory, Equip, Donate, Info, Shop, Attack, Heal, Craft) " +
                                                            "values ({0}, {1}, {1}, {1}, {1}, {1}, {1}, {1}, {1}, {1}, {1}, {1})", userId, Helper.DateTimeToString(new DateTime(2000, 1, 1, 0, 0, 0)));
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task EquipItem(ulong userId, long itemId, string itemType)
        {
            string type = "";
            switch (itemType)
            {
                case "W":
                    type = "WeaponID";
                    break;
                case "H":
                    type = "HelmetID";
                    break;
                case "U":
                    type = "UpperID";
                    break;
                case "P":
                    type = "PantsID";
                    break;
                case "B":
                    type = "BootsID";
                    break;
                case "G":
                    type = "GloveID";
                    break;
                case "M":
                    type = "MantleID";
                    break;
                case "S":
                    type = "ShieldID";
                    break;
            }

            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            
                            cmd.CommandText = string.Format("update Equipped set {0} = {1} where user = {2}", type, itemId, userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task UnequipItems(ulong userId, bool weapon, bool helmet, bool upper, bool pants, bool boots, bool gauntlets, bool mantle, bool shield)
        {
            string s = "update Equipped set ";
            if (weapon) s += "WeaponID = 0,";
            if (helmet) s += "HelmetID = 0,";
            if (upper) s += "UpperID = 0,";
            if (pants) s += "PantsID = 0,";
            if (boots) s += "BootsID = 0,";
            if (gauntlets) s += "GloveID = 0,";
            if (mantle) s += "MantleID = 0,";
            if (shield) s += "ShieldID = 0,";

            s = s.Substring(0, s.Length - 1);
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = string.Format(s + " where user = {0}", userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task<Stats> GetUserInfo(User user)
        {
            Stats stats;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    int weaponId = 0;
                    int helmetId = 0;
                    int upperId = 0;
                    int pantsId = 0;
                    int bootsId = 0;
                    int gauntletsId = 0;
                    int mantleId = 0;
                    int shieldId = 0;

                    // retrieve equipped item Ids
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Equipped where User = @id";
                        cmd.Parameters.Add("@id", DbType.UInt64).Value = user.Id;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            weaponId = (int)reader["WeaponID"];
                            helmetId = (int)reader["HelmetID"];
                            upperId = (int)reader["UpperID"];
                            pantsId = (int)reader["PantsID"];
                            bootsId = (int)reader["BootsID"];
                            gauntletsId = (int)reader["GloveID"];
                            mantleId = (int)reader["MantleID"];
                            shieldId = (int)reader["ShieldID"];
                            reader.Close();
                        }
                    }

                    Weapon weapon = null;
                    if (weaponId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from WeaponsView where ID = {0}", weaponId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    weapon = (Weapon)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor helmet = null;
                    if (helmetId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", helmetId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    helmet = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor upper = null;
                    if (upperId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", upperId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    upper = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor pants = null;
                    if (pantsId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", pantsId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    pants = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor boots = null;
                    if (bootsId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", bootsId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    boots = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor gauntlets = null;
                    if (gauntletsId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", gauntletsId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    gauntlets = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor mantle = null;
                    if (mantleId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", mantleId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    mantle = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Armor shield = null;
                    if (shieldId > 0)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("select * from ArmorsView where ID = {0}", shieldId);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    shield = (Armor)Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                        }
                    }

                    Equipment equipment = new Equipment(weapon, shield, helmet, upper, pants, boots, gauntlets, mantle);

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Stats where User = @id";
                        cmd.Parameters.Add("@id", DbType.UInt64).Value = user.Id;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            stats = new Stats(user,
                                                  (int)reader["Level"],
                                                  (int)reader["Strength"],
                                                  (int)reader["Dexterity"],
                                                  (int)reader["Stamina"],
                                                  (int)reader["Luck"],
                                                  (int)reader["Health"],
                                                  (int)reader["Experience"],
                                                  (int)reader["Gold"],
                                                  equipment);
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return stats;
        }

        #region assign attributes
        internal async static Task<int> GetUnusedAttributePoints(ulong userId)
        {
            int unusedAttributes = 0;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select Level, Strength, Dexterity, Stamina, Luck from Stats where User = {0}", userId);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            int level = (int)reader["Level"];
                            int usedAttributes = (int)reader["Strength"] + (int)reader["Dexterity"] + (int)reader["Stamina"] + (int)reader["Luck"];
                            int totalAttributes = (level - 1) * 3 + (int)Math.Floor(level * 0.2) * 2;
                            unusedAttributes = totalAttributes - usedAttributes;
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return unusedAttributes;
        }

        internal async static Task AssignAttributePointsToStrength(ulong userId, int pointsToAssign)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Strength = (Strength + {0}) " +
                                                            "where User = {1}", pointsToAssign, userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task AssignAttributePointsToDexterity(ulong userId, int pointsToAssign)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Dexterity = (Dexterity + {0}) " +
                                                            "where User = {1}", pointsToAssign, userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task AssignAttributePointsToLuck(ulong userId, int pointsToAssign)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Luck = (Luck + {0}) " +
                                                            "where User = {1}", pointsToAssign, userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task AssignAttributePointsToStamina(ulong userId, int pointsToAssign)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Stamina = (Stamina + {0}) " +
                                                            "where User = {1}", pointsToAssign, userId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        internal async static Task<Tuple<List<InventoryItem>, int, int>> GetInventory(ulong userId, int page)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();
            int pageCount = 0;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        string command = "select {0} from (" +
                                         "select Items.ID, Items.Name, Items.Type, " +
                                         "case when Inventory.ItemID in (" +
                                         "select * from (" +
                                         "select HelmetID as ItemID from equipped " +
                                         "union " +
                                         "select UpperID as ItemID from equipped " +
                                         "union " +
                                         "select PantsID as ItemID from equipped " +
                                         "union " +
                                         "select BootsID as ItemID from equipped " +
                                         "union " +
                                         "select GloveID as ItemID from equipped " +
                                         "union " +
                                         "select MantleID as ItemID from equipped " +
                                         "union " +
                                         "select ShieldID as ItemID from equipped " +
                                         "union " +
                                         "select WeaponID as ItemID from equipped " +
                                         ") x " +
                                         "where x.ItemID <> 0 and user = @user) " +
                                         "then Inventory.Amount - 1 else Inventory.Amount end as Amount " +
                                         "from Inventory inner join Items on Items.ID = Inventory.ItemID " +
                                         "where user = @user) " +
                                         "where Amount > 0 ";


                        cmd.CommandText = string.Format(command, "CAST((count(1) / 10.0) + 0.9 as int) as Pages");
                        cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                pageCount = (int)(long)reader["Pages"];
                            }
                            reader.Close();
                        }

                        if (page < 1)
                            page = 1;
                        else if (page > pageCount)
                            page = pageCount;

                        // retrieve the inventory items
                        cmd.CommandText = string.Format(command + "limit {1}, 10", "*", (page - 1) * 10);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                inventory.Add(new InventoryItem((long)reader["ID"],
                                                                (string)reader["Name"],
                                                                (string)reader["Type"],
                                                                (long)reader["Amount"]));
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return new Tuple<List<InventoryItem>, int, int>(inventory, page, pageCount); ;
        }

        internal async static Task<int> GetAmountOfItemOwned(ulong userId, ulong itemId)
        {
            int amount = 0;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select Amount from Inventory where User = @userid and ItemID = @itemid";
                        cmd.Parameters.Add("@userid", DbType.UInt64).Value = userId;
                        cmd.Parameters.Add("@itemid", DbType.UInt64).Value = itemId;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                amount = (reader.HasRows ? (int)reader["Amount"] : 0);
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return amount;
        }

        internal async static Task<int> GetGold(ulong userId)
        {
            int gold;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select Gold from Stats where User = {0}", userId);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            gold = (int)reader["Gold"];
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return gold;
        }

        internal async static Task Donate(ulong callerId, ulong targetId, int gold)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Gold = (Gold - {0}) " +
                                                            "where User = {1}", gold, callerId);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Gold = (Gold + {0}) " +
                                                            "where User = {1}", gold, targetId);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }    

        internal async static Task<Item> GetItemInfoByID(ulong id)
        {
            if (id == 0) return null;
            Item item = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        List<string> views = new List<string>();
                        views.Add("WeaponsView");
                        views.Add("ArmorsView");
                        views.Add("PotionsView");
                        views.Add("ItemsView");

                        cmd.Parameters.Add("@id", DbType.UInt64).Value = id;

                        // do an exact search in each type of item
                        foreach (string view in views)
                        {
                            cmd.CommandText = string.Format("select * from {0} where ID = @id", view);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    item = Helper.parseItem(reader);
                                }
                                reader.Close();
                            }
                            if (item != null) break;
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return item;
        }

        internal async static Task<List<Item>> GetItemInfoByName(string name)
        {
            if (name == "") return null;
            List<Item> items = new List<Item>();
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        List<string> views = new List<string>();
                        views.Add("WeaponsView");
                        views.Add("ArmorsView");
                        views.Add("PotionsView");
                        views.Add("ItemsView");

                        cmd.Parameters.AddWithValue("@name", name);

                        // do an exact search in each type of item
                        foreach (string view in views)
                        {
                            cmd.CommandText = string.Format("select * from {0} where lower(Name) = lower(@name)", view);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    items.Add(Helper.parseItem(reader));
                                }
                                reader.Close();
                            }
                            if (items.Count != 0) break;
                        }

                        // if no exact match is found look through all the views to find anything that contains the search
                        if (items.Count == 0)
                        {
                            foreach (string view in views)
                            {
                                cmd.CommandText = string.Format("select * from {0} where lower(Name) like lower('%' || @name || '%')", view);
                                using (SQLiteDataReader reader = cmd.ExecuteReader())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        items.Add(Helper.parseItem(reader));
                                    }
                                    reader.Close();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return items;
        }

        internal async static Task<Monster> GetMonsterInfoByID(ulong id)
        {
            Monster monster = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Monsters where ID = @id";
                        cmd.Parameters.Add("@id", DbType.UInt64).Value = id;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                monster = new Monster((long)reader["ID"],
                                                      (string)reader["Name"],
                                                      (int)reader["Health"],
                                                      (int)reader["Health"],
                                                      (int)reader["HealthRegen"],
                                                      (int)reader["MinDef"],
                                                      (int)reader["CritDef"],
                                                      (int)reader["MinDmg"],
                                                      (int)reader["Experience"],
                                                      (int)reader["GoldMin"],
                                                      (int)reader["GoldMax"]);
                            }
                            reader.Close();
                        }

                        if (monster != null)
                        {
                            List<Item> items = new List<Item>();
                            cmd.CommandText = "select * from items where exists(select 1 from Drops where Drops.ItemID = items.ID and Drops.MonsterID = @id)";
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Drop((long)reader["ID"], (string)reader["name"], 0));
                                }
                                reader.Close();
                            }
                            monster.setDrops(items);
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }

            return monster;
        }

        internal async static Task<List<Monster>> GetMonsterInfoByName(string name)
        {
            List<Monster> monsters = new List<Monster>();
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Monsters where lower(Name) like lower('%' || @name || '%')";
                        cmd.Parameters.Add("@name", DbType.String).Value = name;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                monsters.Add(new Monster((long)reader["ID"],
                                                         (string)reader["Name"],
                                                         (int)reader["Health"],
                                                         (int)reader["Health"],
                                                         (int)reader["HealthRegen"],
                                                         (int)reader["MinDef"],
                                                         (int)reader["CritDef"],
                                                         (int)reader["MinDmg"],
                                                         (int)reader["Experience"],
                                                         (int)reader["GoldMin"],
                                                         (int)reader["GoldMax"]));
                            }
                            reader.Close();
                        }

                        if (monsters.Count == 1)
                        {
                            List<Item> items = new List<Item>();
                            cmd.CommandText = "select * from items where exists(select 1 from Drops where Drops.ItemID = items.ID and Drops.MonsterID = @id)";
                            cmd.Parameters.Add("@id", DbType.Int64).Value = monsters[0].id;
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Drop((long)reader["ID"], (string)reader["name"], 0));
                                }
                                reader.Close();
                            }
                            monsters[0].setDrops(items);
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }

            return monsters;
        }

        #region Shop
        internal async static Task<Tuple<List<Item>, int, int>> GetShopInfo(int page)
        {
            List<Item> items = new List<Item>();
            int pageCount = 0;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select CAST((count(1)/10.0) + 0.9 as int) as Pages from Items where ValueBuy > 0");
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                pageCount = (int)(long)reader["Pages"];
                            }
                            reader.Close();
                        }

                        if (page < 1)
                            page = 1;
                        else if (page > pageCount)
                            page = pageCount;

                        cmd.CommandText = string.Format("select * from Items where ValueBuy > 0 order by Level, ValueBuy limit {0}, 10", (page - 1) * 10);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new Item((long)reader["ID"],
                                                   (string)reader["Name"],
                                                   (string)reader["Type"],
                                                   (int)reader["Level"],
                                                   (int)reader["ValueBuy"],
                                                   (int)reader["ValueSell"]));
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return new Tuple<List<Item>, int, int>(items, page, pageCount);
        }

        internal async static Task<Item> GetShopItemByID(ulong id)
        {
            if (id == 0) return null;
            Item item = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select ID, Name, ValueBuy from Items where ID = @id and ValueBuy > 0";
                        cmd.Parameters.Add("@id", DbType.UInt64).Value = id;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                item = new Item((long)reader["ID"],
                                                (string)reader["Name"],
                                                "I",
                                                0,
                                                (int)reader["ValueBuy"],
                                                0);
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return item;
        }

        internal async static Task<List<Item>> GetShopItemByName(string name)
        {
            if (name == "") return null;
            List<Item> items = new List<Item>();
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.Add("@name", DbType.String).Value = name;

                        // do an exact search
                        cmd.CommandText = "select ID, Name, ValueBuy from Items where lower(Name) = lower(@name) and ValueBuy > 0";
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                items.Add(new Item((long)reader["ID"],
                                                   (string)reader["Name"],
                                                   "I",
                                                   0,
                                                   (int)reader["ValueBuy"],
                                                   0));
                            }
                            reader.Close();
                        }

                        // if no exact match is found find anything that contains the search
                        if (items.Count == 0)
                        {
                            cmd.CommandText = "select * from Items where lower(Name) like lower('%' || @name || '%') and ValueBuy > 0";
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Item((long)reader["ID"],
                                                       (string)reader["Name"],
                                                       "I",
                                                       0,
                                                       (int)reader["ValueBuy"],
                                                       0));
                                }
                                reader.Close();
                            }
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return items;
        }

        internal async static Task ShopBuy(ulong userId, int amount, long itemId)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                            cmd.Parameters.Add("@item", DbType.Int64).Value = itemId;
                            cmd.CommandText = string.Format("update Stats set Gold = Gold - (select (ValueBuy * {0}) from Items where ID = @item) where User = @user", amount);
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "select 1 from Inventory where User = @user and ItemID = @item";
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Close();
                                    cmd.CommandText = string.Format("update Inventory set Amount = Amount + {0} where User = @user and ItemID = @item", amount);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                                else
                                {
                                    reader.Close();
                                    cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) values (@user, @item, {0})", amount);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task ShopSell(ulong userId, int amount, long itemId)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                            cmd.Parameters.Add("@item", DbType.Int64).Value = itemId;
                            cmd.CommandText = string.Format("update Stats set Gold = Gold + (select (ValueSell * {0}) from Items where ID = @item) where User = @user", amount);
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "select Amount from Inventory where User = @user and ItemID = @item";
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                await reader.ReadAsync();
                                int left = (int)reader["Amount"] - amount;
                                reader.Close();
                                if (left < 0)
                                    throw new Exceptions.RPGException("you wut @ShopSell");
                                else if (left == 0)
                                    cmd.CommandText = "delete from Inventory where User = @user and ItemID = @item";
                                else
                                    cmd.CommandText = string.Format("update Inventory set Amount = Amount - {0} where User = @user and ItemID = @item", amount);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        #region Battle
        internal async static Task<Monster> GetFight(ulong userId)
        {
            Monster monster = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select * from UserFight where User = {0}", userId);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                monster = new Monster((long)reader["ID"],
                                                      (string)reader["Name"],
                                                      (int)reader["CurrentHealth"],
                                                      (int)reader["Health"],
                                                      (int)reader["HealthRegen"],
                                                      (int)reader["MinDef"],
                                                      (int)reader["CritDef"],
                                                      (int)reader["MinDmg"],
                                                      (int)reader["Experience"],
                                                      (int)reader["GoldMin"],
                                                      (int)reader["GoldMax"]);
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return monster;
        }

        internal async static Task CreateFight(User user, Monster monster)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("insert into Fight (User, MonsterId, CurrentHealth) values ({0}, {1}, {2})", user.Id, monster.id, monster.healthMax);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                conn.Close();
            }
        }

        internal async static Task BattleFinished(User user, Stats player, Monster monster, List<Drop> drops)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            if (monster.health <= 0 || player.health <= 0)
                            {
                                cmd.CommandText = string.Format("delete from Fight where User = {0}", user.Id);
                                await cmd.ExecuteNonQueryAsync();

                                if (monster.health <= 0)
                                {
                                    foreach(Drop drop in drops)
                                    {
                                        cmd.CommandText = string.Format("select 1 from Inventory where User = {0} and ItemID = {1}", user.Id, drop.id);
                                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                reader.Close();
                                                cmd.CommandText = string.Format("update Inventory set Amount = Amount + {0} where User = {1} and ItemID = {2}", drop.amount, user.Id, drop.id);
                                                await cmd.ExecuteNonQueryAsync();
                                            }
                                            else
                                            {
                                                reader.Close();
                                                cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) values ({0}, {1}, {2})", user.Id, drop.id, drop.amount);
                                                await cmd.ExecuteNonQueryAsync();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                cmd.CommandText = string.Format($"update Fight set CurrentHealth = {monster.health}, TimeLastHit = {Helper.DateTimeToString(DateTime.Now)} where User = {user.Id}");
                                await cmd.ExecuteNonQueryAsync();
                            }

                            cmd.CommandText = string.Format($"update Stats set Level = {player.level}, Health = {player.health}, Experience = {player.experience}, Gold = {player.gold} where User = {user.Id}");
                            await cmd.ExecuteNonQueryAsync();
                            tr.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task<List<Drop>> GetKilledMonsterDrops(long monsterId)
        {
            List<Drop> drops = new List<Drop>();
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select Items.ID, Items.Name, Drops.AmountMin, Drops.AmountMax " +
                                                        "from Drops inner join Items on Items.ID = Drops.ItemID " +
                                                        "where MonsterID = {0} and {1} between ChanceMin and ChanceMax", monsterId, Helper.rng.Next(0,100));
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while(await reader.ReadAsync())
                            {
                                drops.Add(new Drop((long)reader["ID"],
                                                   (string)reader["name"],
                                                   Helper.rng.Next((int)reader["AmountMin"], (int)reader["AmountMax"])));
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return drops;
        }
        #endregion

        internal async static Task<Potion> GetBestPotionInInventory(ulong userId)
        {
            Potion potion = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from PotionsView " +
                                          "where exists(select 1 from Inventory where ItemID = PotionsView.ID and amount > 0 and User = @user) " +
                                          "order by heal desc";
                        cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                potion = (Potion)Helper.parseItem(reader);
                            }
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return potion;
        }

        internal async static Task Heal(ulong userId, int health, long itemId)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                            cmd.Parameters.Add("@health", DbType.Int32).Value = health;
                            cmd.Parameters.Add("@item", DbType.Int64).Value = itemId;

                            cmd.CommandText = "update Stats set Health = @health where User = @user";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "select Amount from Inventory where User = @user and ItemID = @item";
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                await reader.ReadAsync();
                                int left = (int)reader["Amount"] - 1;
                                reader.Close();
                                if (left < 0)
                                    throw new Exceptions.RPGException("you wut @heal");
                                else if (left == 0)
                                    cmd.CommandText = "delete from Inventory where User = @user and ItemID = @item";
                                else
                                    cmd.CommandText = "update Inventory set Amount = Amount - 1 where User = @user and ItemID = @item";
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task<Tuple<List<Tuple<Item, long, long>>, int, int>> GetRequirementsCraftedItem(ulong userId, long itemId)
        {
            Tuple<List<Tuple<Item, long, long>>, int, int> result = null;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        int chance = 0;
                        int cost = 0;

                        cmd.CommandText = "select Cost, Chance from Upgrades where ItemID = @item";
                        cmd.Parameters.Add("@item", DbType.Int64).Value = itemId;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                cost = (int)reader["Cost"];
                                chance = (int)reader["Chance"];
                            }
                            reader.Close();
                        }

                        List<Tuple<Item, long, long>> itemsNeeded = new List<Tuple<Item, long, long>>();
                        cmd.CommandText = "select UpgradeCost.NeededItemId as ID, cast(UpgradeCost.Amount as INT) as AmountNeeded, ifnull(Inv.Amount, 0) as AmountOwned, " +
                                          "Items.Name, 'I' as Type, Items.Level, Items.valueBuy, Items.ValueSell " +
                                          "from UpgradeCost " +
                                          "left join (select User, ItemID, cast(case when exists(select 1 from(select HelmetID as ID from Equipped " +
                                          "union " +
                                          "select UpperID as ID from Equipped " +
                                          "union " +
                                          "select PantsID as ID from Equipped " +
                                          "union " +
                                          "select BootsID as ID from Equipped " +
                                          "union " +
                                          "select GloveID as ID from Equipped " +
                                          "union " +
                                          "select MantleID as ID from Equipped " +
                                          "union " +
                                          "select ShieldID as ID from Equipped " +
                                          "union " +
                                          "select WeaponID as ID from Equipped) x " +
                                          "where x.ID = Inventory.ItemID) then Amount-1 else Amount end as INT) as Amount " +
                                          "from inventory where Inventory.User = @user) Inv on Inv.ItemID = UpgradeCost.NeededItemId " +
                                          "inner join Items on Items.ID = UpgradeCost.NeededItemId " +
                                          "where UpgradeCost.UpgradeItemId = @item";
                        cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                Item item = Helper.parseItem(reader);
                                long needed = (long)reader["AmountNeeded"];
                                long owned = (long)reader["AmountOwned"];
                                itemsNeeded.Add(new Tuple<Item, long, long>(item, needed, owned));
                            }
                            reader.Close();
                        }

                        if (itemsNeeded.Count > 0) result = new Tuple<List<Tuple<Item, long, long>>, int, int>(itemsNeeded, cost, chance);
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        internal async static Task Craft(ulong userId, long savOrbId, long itemId, bool success)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.UInt64).Value = userId;
                            cmd.Parameters.Add("@savorb", DbType.Int64).Value = savOrbId;
                            cmd.Parameters.Add("@item", DbType.Int64).Value = itemId;

                            cmd.CommandText = "update Inventory set Amount = Amount - 1 where User = @user and ItemID = @savorb";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "update Inventory " +
                                              "set Amount = Amount - (select UpgradeCost.Amount " +
                                              "from UpgradeCost where UpgradeCost.NeededItemID = Inventory.ItemID and UpgradeCost.UpgradeItemID = @item) " +
                                              "where User = @user and ItemID in (select NeededItemID from UpgradeCost where UpgradeCost.UpgradeItemID = @item)";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "delete from Inventory where User = @user and Amount = 0";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "update Stats set Gold = Gold - (select Cost from Upgrades where ItemID = @item) where User = @user";
                            await cmd.ExecuteNonQueryAsync();

                            if (success)
                            {
                                cmd.CommandText = "select 1 from Inventory where User = @user and ItemID = @item";
                                using (SQLiteDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Close();
                                        cmd.CommandText = "update Inventory set Amount = Amount + 1 where User = @user and ItemID = @item";
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    else
                                    {
                                        reader.Close();
                                        cmd.CommandText = "insert into Inventory (User, ItemID, Amount) values (@user, @item, 1)";
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        #region test only
        internal async static Task setLevel(ulong id, int level)
        {
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Level = (Level + 1), " +
                                                            "Strength = 0, " +
                                                            "Dexterity = 0, " +
                                                            "Stamina = 0, " +
                                                            "Luck = 0 " +
                                                            "where user = {0}", id);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.CommandText = string.Format("update Stats set Level = @level where user = {0}", id);
                            cmd.Parameters.Add("@level", DbType.Int32).Value = level;
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal async static Task<string> SpawnItem(ulong userId, long itemId, int amount)
        {
            bool itemExists = false;
            using (SQLiteConnection conn = getConnection())
            {
                await conn.OpenAsync();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = string.Format("select 1 from Items where ID = @id", itemId);
                            cmd.Parameters.Add("@id", DbType.Int64).Value = itemId;
                            cmd.Parameters.Add("@amount", DbType.Int32).Value = amount;
                            
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows) itemExists = true;
                                reader.Close();
                            }

                            if (itemExists)
                            {
                                cmd.CommandText = string.Format("select 1 from Inventory where User = {0} and ItemID = @id", userId);
                                using (SQLiteDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Close();
                                        cmd.CommandText = string.Format("update Inventory set Amount = Amount + @amount where User = {0} and ItemID = @id", userId);
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    else
                                    {
                                        reader.Close();
                                        cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) values ({0}, @id, @amount)", userId);
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return (itemExists ? " succesfully spawned an item!" : " failed to spawn an item!");
        }
        #endregion
    }
}
