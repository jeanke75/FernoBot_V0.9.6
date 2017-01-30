﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Helpers;
using DiscordBot.Model;

namespace DiscordBot.Data
{
    static class RPGDataHelper
    {
        #region Cooldowns
        internal async static Task<DateTime> GetTimeCommandUsed(long userId, string command)
        {
            DateTime lastUse = new DateTime(0);
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select Cooldowns.{0} from Cooldowns where UserID = @user", command);
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                lastUse = (DateTime)reader[command];
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

        internal async static Task SetTimeCommandUsed(long userId, string command)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = string.Format("Update Cooldowns set {0} = sysdatetime() where UserID = @user", command);
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
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

        internal async static Task<long> GetUserID(ulong discordId)
        {
            long userId = 0;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.Add("@DiscordID", DbType.String).Value = discordId.ToString();
                        cmd.CommandText = "select UserID from Users where DiscordID = @DiscordID";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                userId = (long)reader["UserID"];
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
            return userId;
        }

        internal async static Task Create(ulong discordId)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@discord", DbType.String).Value = discordId.ToString();
                            // add user to the database
                            cmd.CommandText = "insert into Users(DiscordID) values (@discord)";
                            await cmd.ExecuteNonQueryAsync();

                            // retrieve userid
                            long userId = 0;
                            cmd.CommandText = "select UserID from Users where DiscordID = @discord";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                await reader.ReadAsync();
                                userId = (long)reader["UserID"];
                                reader.Close();
                            }

                            // create character stats
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.CommandText = "insert into Stats (UserID, Health, Level, Experience, Strength, Dexterity, Stamina, Luck, Gold) " +
                                              "values (@user, 72, 1, 0, 0, 0, 0, 0, 0)";
                            await cmd.ExecuteNonQueryAsync();
                            
                            /*
                            // give weapon
                            cmd.CommandText = "insert into Inventory (UserID, ItemID, Amount) values (@user, {0}, 1)", 2);
                            await cmd.ExecuteNonQueryAsync();

                            // give helmet
                            cmd.CommandText = string.Format("insert into Inventory (UserID, ItemID, Amount) " +
                                                            "values (@user, {0}, 1)", 13);
                            await cmd.ExecuteNonQueryAsync();
                            // give upper
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values (@user, {0}, 1)", 23);
                            await cmd.ExecuteNonQueryAsync();
                            // give pants
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values (@user, {0}, 1)", 33);
                            await cmd.ExecuteNonQueryAsync();
                            //give boots
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values (@user, {0}, 1)", 43);
                            await cmd.ExecuteNonQueryAsync();
                            // give gauntlets
                            cmd.CommandText = string.Format("insert into Inventory (User, ItemID, Amount) " +
                                                            "values (@user, {0}, 1)", 53);
                            await cmd.ExecuteNonQueryAsync();*/

                            // equip gear
                            cmd.CommandText = "insert into Equipped (UserID, HelmetID, UpperID, PantsID, BootsID, GloveID, MantleID, ShieldID, WeaponID) " +
                                              "values (@user, null, null, null, null, null, null, null, null)"; // 13, 23, 33, 43, 53, 0, 0, 2);
                            await cmd.ExecuteNonQueryAsync();

                            //create cooldowns
                            cmd.Parameters.Add("@DateTime", DbType.DateTime).Value = new DateTime(2000, 1, 1, 0, 0, 0);
                            cmd.CommandText = "insert into Cooldowns(UserID, Start, Stats, Assign, Inventory, Equip, Donate, Info, Shop, Attack, Heal, Craft) " +
                                              "values (@user, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime)";//, Helper.DateTimeToString(new DateTime(2000, 1, 1, 0, 0, 0)));
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

        internal async static Task EquipItem(ulong userId, long itemId, char itemType)
        {
            string type = "";
            switch (itemType)
            {
                case 'W':
                    type = "WeaponID";
                    break;
                case 'H':
                    type = "HelmetID";
                    break;
                case 'U':
                    type = "UpperID";
                    break;
                case 'P':
                    type = "PantsID";
                    break;
                case 'B':
                    type = "BootsID";
                    break;
                case 'G':
                    type = "GloveID";
                    break;
                case 'M':
                    type = "MantleID";
                    break;
                case 'S':
                    type = "ShieldID";
                    break;
            }

            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            
                            cmd.CommandText = string.Format("update Equipped set {0} = @item where UserID = @user", type);
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
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

        internal async static Task UnequipItems(long userId, bool weapon, bool helmet, bool upper, bool pants, bool boots, bool gauntlets, bool mantle, bool shield)
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
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = s + " where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
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

        internal async static Task<Stats> GetUserInfo(long userId, User user)
        {
            Stats stats;
            using (SqlConnection conn = Helper.getConnection())
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
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Equipped where UserID = @user";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            weaponId = reader["WeaponID"] as int? ?? 0;
                            helmetId = reader["HelmetID"] as int? ?? 0;
                            upperId = reader["UpperID"] as int? ?? 0;
                            pantsId = reader["PantsID"] as int? ?? 0;
                            bootsId = reader["BootsID"] as int? ?? 0;
                            gauntletsId = reader["GloveID"] as int? ?? 0;
                            mantleId = reader["MantleID"] as int? ?? 0;
                            shieldId = reader["ShieldID"] as int? ?? 0;
                            reader.Close();
                        }
                    }

                    Weapon weapon = null;
                    if (weaponId > 0)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from WeaponsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = weaponId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = helmetId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = upperId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = pantsId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = bootsId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = gauntletsId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = mantleId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select * from ArmorsView where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = shieldId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Stats where UserID = @user";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            stats = new Stats(user,
                                              (byte)reader["Level"],
                                              (short)reader["Strength"],
                                              (short)reader["Dexterity"],
                                              (short)reader["Stamina"],
                                              (short)reader["Luck"],
                                              (short)reader["Health"],
                                              (int)reader["Experience"],
                                              (long)reader["Gold"],
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
        internal async static Task<short> GetUnusedAttributePoints(long userId)
        {
            short unusedAttributes = 0;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select Level, Strength, Dexterity, Stamina, Luck from Stats where UserID = @user";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            byte level = (byte)reader["Level"];
                            int usedAttributes = (short)reader["Strength"] + (short)reader["Dexterity"] + (short)reader["Stamina"] + (short)reader["Luck"];
                            int totalAttributes = (level - 1) * 3 + (int)Math.Floor(level * 0.2) * 2;
                            unusedAttributes = (short)(totalAttributes - usedAttributes);
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

        internal async static Task AssignAttributePointsToStrength(long userId, short pointsToAssign)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = "update Stats set Strength = (Strength + @pta) where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@pta", DbType.Int16).Value = pointsToAssign;
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

        internal async static Task AssignAttributePointsToDexterity(long userId, short pointsToAssign)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = "update Stats set Dexterity = (Dexterity + @pta) where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@pta", DbType.Int16).Value = pointsToAssign;
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

        internal async static Task AssignAttributePointsToLuck(long userId, int pointsToAssign)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = "update Stats set Luck = (Luck + @pta) where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@pta", DbType.Int16).Value = pointsToAssign;
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

        internal async static Task AssignAttributePointsToStamina(long userId, int pointsToAssign)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = "update Stats set Stamina = (Stamina + @pta) where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@pta", DbType.Int16).Value = pointsToAssign;
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

        internal async static Task<Tuple<List<InventoryItem>, int, int>> GetInventory(long userId, int page)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();
            int pageCount = 0;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        string command = "select {0} from (" +
                                         "select Items.ItemID, Items.Name, Items.Type, " +
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
                                         "where x.ItemID <> 0 and UserID = @id) " +
                                         "then Inventory.Amount - 1 else Inventory.Amount end as Amount " +
                                         "from Inventory inner join Items on Items.ItemID = Inventory.ItemID " +
                                         "where UserID = @user) " +
                                         "where Amount > 0 ";


                        cmd.CommandText = string.Format(command, "CAST((count(1) / 10.0) + 0.9 as int) as Pages");
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                inventory.Add(new InventoryItem((int)reader["ItemID"],
                                                                (string)reader["Name"],
                                                                (char)reader["Type"],
                                                                (int)reader["Amount"]));
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

        internal async static Task<int> GetAmountOfItemOwned(long userId, int itemId)
        {
            int amount = 0;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select Amount from Inventory where UserID = @user and ItemID = @item";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
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

        internal async static Task<long> GetCurrentUserGold(long userId)
        {
            long gold;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select Gold from Stats where UserID = @user";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            await reader.ReadAsync();
                            gold = (long)reader["Gold"];
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

        internal async static Task Donate(long callerId, long targetId, long gold)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Gold = (Gold - {0}) " +
                                                            "where UserID = {1}", gold, callerId);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.CommandText = string.Format("update Stats set " +
                                                            "Gold = (Gold + {0}) " +
                                                            "where UserID = {1}", gold, targetId);
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

        internal async static Task<Item> GetItemInfoByID(int itemId)
        {
            if (itemId == 0) return null;
            Item item = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        List<string> views = new List<string>();
                        views.Add("WeaponsView");
                        views.Add("ArmorsView");
                        views.Add("PotionsView");
                        views.Add("ItemsView");

                        cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;

                        // do an exact search in each type of item
                        foreach (string view in views)
                        {
                            cmd.CommandText = string.Format("select * from {0} where ItemID = @item", view);
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
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
                            using (SqlDataReader reader = cmd.ExecuteReader())
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
                                using (SqlDataReader reader = cmd.ExecuteReader())
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

        internal async static Task<Monster> GetMonsterInfoByID(int monsterId)
        {
            Monster monster = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Monsters where MonsterID = @monster";
                        cmd.Parameters.Add("@monster", DbType.Int32).Value = monsterId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                monster = new Monster((int)reader["MonsterID"],
                                                      (string)reader["Name"],
                                                      (int)reader["Health"],
                                                      (int)reader["Health"],
                                                      (short)reader["HealthRegen"],
                                                      (short)reader["MinDef"],
                                                      (short)reader["CritDef"],
                                                      (short)reader["MinDmg"],
                                                      (int)reader["Experience"],
                                                      (int)reader["GoldMin"],
                                                      (int)reader["GoldMax"]);
                            }
                            reader.Close();
                        }

                        if (monster != null)
                        {
                            List<Item> items = new List<Item>();
                            cmd.CommandText = "select * from items where exists(select 1 from Drops where Drops.ItemID = items.ItemID and Drops.MonsterID = @monster)";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Drop((int)reader["ItemID"], (string)reader["name"], 0));
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
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from Monsters where lower(Name) like lower('%' || @name || '%')";
                        cmd.Parameters.Add("@name", DbType.String).Value = name;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                monsters.Add(new Monster((int)reader["MonsterID"],
                                                         (string)reader["Name"],
                                                         (int)reader["Health"],
                                                         (int)reader["Health"],
                                                         (short)reader["HealthRegen"],
                                                         (short)reader["MinDef"],
                                                         (short)reader["CritDef"],
                                                         (short)reader["MinDmg"],
                                                         (int)reader["Experience"],
                                                         (int)reader["GoldMin"],
                                                         (int)reader["GoldMax"]));
                            }
                            reader.Close();
                        }

                        if (monsters.Count == 1)
                        {
                            List<Item> items = new List<Item>();
                            cmd.CommandText = "select * from items where exists(select 1 from Drops where Drops.ItemID = items.ItemID and Drops.MonsterID = @monster)";
                            cmd.Parameters.Add("@monster", DbType.Int32).Value = monsters[0].id;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Drop((int)reader["ItemID"], (string)reader["name"], 0));
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
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select CAST((count(1)/10.0) + 0.9 as int) as Pages from Items where ValueBuy > 0");
                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new Item((int)reader["ItemID"],
                                                   (string)reader["Name"],
                                                   (char)reader["Type"],
                                                   (byte)reader["Level"],
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

        internal async static Task<Item> GetShopItemByID(int itemId)
        {
            if (itemId == 0) return null;
            Item item = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select ItemID, Name, ValueBuy from Items where ItemID = @item and ValueBuy > 0";
                        cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                item = new Item((int)reader["ItemID"],
                                                (string)reader["Name"],
                                                'I',
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
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.Add("@name", DbType.String).Value = name;

                        // do an exact search
                        cmd.CommandText = "select ItemID, Name, ValueBuy from Items where lower(Name) = lower(@name) and ValueBuy > 0";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                items.Add(new Item((int)reader["ItemID"],
                                                   (string)reader["Name"],
                                                   'I',
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
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (await reader.ReadAsync())
                                {
                                    items.Add(new Item((int)reader["ItemID"],
                                                       (string)reader["Name"],
                                                       'I',
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

        internal async static Task ShopBuy(long userId, int amount, int itemId)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                            cmd.CommandText = string.Format("update Stats set Gold = Gold - (select (ValueBuy * {0}) from Items where ItemID = @item) where UserID = @user", amount);
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "select 1 from Inventory where UserID = @user and ItemID = @item";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Close();
                                    cmd.CommandText = string.Format("update Inventory set Amount = Amount + {0} where UserID = @user and ItemID = @item", amount);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                                else
                                {
                                    reader.Close();
                                    cmd.CommandText = string.Format("insert into Inventory (UserID, ItemID, Amount) values (@user, @item, {0})", amount);
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

        internal async static Task ShopSell(long userId, int amount, int itemId)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                            cmd.CommandText = string.Format("update Stats set Gold = Gold + (select (ValueSell * {0}) from Items where ItemID = @item) where UserID = @user", amount);
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "select Amount from Inventory where UserID = @user and ItemID = @item";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                await reader.ReadAsync();
                                int left = (int)reader["Amount"] - amount;
                                reader.Close();
                                if (left < 0)
                                    throw new Exceptions.RPGException("you wut @ShopSell");
                                else if (left == 0)
                                    cmd.CommandText = "delete from Inventory where UserID = @user and ItemID = @item";
                                else
                                    cmd.CommandText = string.Format("update Inventory set Amount = Amount - {0} where UserID = @user and ItemID = @item", amount);
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
        internal async static Task<Monster> GetFight(long userId)
        {
            Monster monster = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from UserFight where UserID = @user";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                monster = new Monster((int)reader["MonsterID"],
                                                      (string)reader["Name"],
                                                      (int)reader["CurrentHealth"],
                                                      (int)reader["Health"],
                                                      (short)reader["HealthRegen"],
                                                      (short)reader["MinDef"],
                                                      (short)reader["CritDef"],
                                                      (short)reader["MinDmg"],
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

        internal async static Task CreateFight(long userId, Monster monster)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.CommandText = "insert into Fight (UserID, MonsterID, CurrentHealth) values (@user, @monster, @hpmax)";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@monster", DbType.Int32).Value = monster.id;
                            cmd.Parameters.Add("@hpmax", DbType.Int32).Value = monster.healthMax;
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

        internal async static Task BattleFinished(long userId, Stats player, Monster monster, List<Drop> drops)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            if (monster.health <= 0 || player.health <= 0)
                            {
                                cmd.CommandText = "delete from Fight where UserID = @user";
                                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                                await cmd.ExecuteNonQueryAsync();

                                if (monster.health <= 0)
                                {
                                    foreach(Drop drop in drops)
                                    {
                                        cmd.CommandText = "select 1 from Inventory where UserID = @user and ItemID = @item";
                                        cmd.Parameters.Add("@item", DbType.Int32).Value = drop.id;
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                reader.Close();
                                                cmd.CommandText = string.Format("update Inventory set Amount = Amount + {0} where UserID = @user and ItemID = @item", drop.amount);
                                                await cmd.ExecuteNonQueryAsync();
                                            }
                                            else
                                            {
                                                reader.Close();
                                                cmd.CommandText = string.Format("insert into Inventory (UserID, ItemID, Amount) values (@user, @item, {0})", drop.amount);
                                                await cmd.ExecuteNonQueryAsync();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                cmd.CommandText = string.Format($"update Fight set CurrentHealth = {monster.health} where UserID = {userId}");
                                await cmd.ExecuteNonQueryAsync();
                            }

                            cmd.CommandText = string.Format($"update Stats set Level = {player.level}, Health = {player.health}, Experience = {player.experience}, Gold = {player.gold} where UserID = {userId}");
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

        internal async static Task<List<Drop>> GetKilledMonsterDrops(int monsterId)
        {
            List<Drop> drops = new List<Drop>();
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select Items.ItemID, Items.Name, Drops.AmountMin, Drops.AmountMax " +
                                                        "from Drops inner join Items on Items.ItemID = Drops.ItemID " +
                                                        "where MonsterID = {0} and {1} between ChanceMin and ChanceMax", monsterId, Helper.rng.Next(0,100));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(await reader.ReadAsync())
                            {
                                drops.Add(new Drop((int)reader["ItemID"],
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

        internal async static Task<Potion> GetBestPotionInInventory(long userId)
        {
            Potion potion = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from PotionsView " +
                                          "where exists(select 1 from Inventory where ItemID = PotionsView.ID and amount > 0 and UserID = @user) " +
                                          "order by heal desc";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
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

        internal async static Task Heal(long userId, short health, int itemId)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId.ToString();
                            cmd.Parameters.Add("@health", DbType.Int16).Value = health;

                            cmd.CommandText = "update Stats set Health = @health where UserID = @user";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                            cmd.CommandText = "select Amount from Inventory where UserID = @user and ItemID = @item";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                await reader.ReadAsync();
                                int left = (int)reader["Amount"] - 1;
                                reader.Close();
                                if (left < 0)
                                    throw new Exceptions.RPGException("you wut @heal");
                                else if (left == 0)
                                    cmd.CommandText = "delete from Inventory where UserID = @user and ItemID = @item";
                                else
                                    cmd.CommandText = "update Inventory set Amount = Amount - 1 where UserID = @user and ItemID = @item";
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

        internal async static Task<Tuple<List<Tuple<Item, int, int>>, int, int>> GetRequirementsCraftedItem(long userId, long itemId)
        {
            Tuple<List<Tuple<Item, int, int>>, int, int> result = null;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        int chance = 0;
                        int cost = 0;

                        cmd.CommandText = "select Cost, Chance from Upgrades where ItemID = @item";
                        cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();
                                cost = (int)reader["Cost"];
                                chance = (int)reader["Chance"];
                            }
                            reader.Close();
                        }

                        List<Tuple<Item, int, int>> itemsNeeded = new List<Tuple<Item, int, int>>();
                        cmd.CommandText = "select UpgradeCost.NeededItemId as ID, cast(UpgradeCost.Amount as INT) as AmountNeeded, ifnull(Inv.Amount, 0) as AmountOwned, " +
                                          "Items.Name, 'I' as Type, Items.Level, Items.valueBuy, Items.ValueSell " +
                                          "from UpgradeCost " +
                                          "left join (select UserID, ItemID, cast(case when exists(select 1 from(select HelmetID as ID from Equipped " +
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
                                          "from inventory where Inventory.UserID = @user) Inv on Inv.ItemID = UpgradeCost.NeededItemId " +
                                          "inner join Items on Items.ItemID = UpgradeCost.NeededItemId " +
                                          "where UpgradeCost.UpgradeItemId = @item";
                        cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                Item item = Helper.parseItem(reader);
                                int needed = (int)reader["AmountNeeded"];
                                int owned = (int)reader["AmountOwned"];
                                itemsNeeded.Add(new Tuple<Item, int, int>(item, needed, owned));
                            }
                            reader.Close();
                        }

                        if (itemsNeeded.Count > 0) result = new Tuple<List<Tuple<Item, int, int>>, int, int>(itemsNeeded, cost, chance);
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        internal async static Task Craft(long userId, int savOrbId, int itemId, bool success)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            cmd.Parameters.Add("@savorb", DbType.Int32).Value = savOrbId;
                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;

                            cmd.CommandText = "update Inventory set Amount = Amount - 1 where UserID = @user and ItemID = @savorb";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "update Inventory " +
                                              "set Amount = Amount - (select UpgradeCost.Amount " +
                                              "from UpgradeCost where UpgradeCost.NeededItemID = Inventory.ItemID and UpgradeCost.UpgradeItemID = @item) " +
                                              "where UserID = @user and ItemID in (select NeededItemID from UpgradeCost where UpgradeCost.UpgradeItemID = @item)";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "delete from Inventory where UserID = @user and Amount = 0";
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "update Stats set Gold = Gold - (select Cost from Upgrades where ItemID = @item) where UserID = @user";
                            await cmd.ExecuteNonQueryAsync();

                            if (success)
                            {
                                cmd.CommandText = "select 1 from Inventory where UserID = @user and ItemID = @item";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Close();
                                        cmd.CommandText = "update Inventory set Amount = Amount + 1 where UserID = @user and ItemID = @item";
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    else
                                    {
                                        reader.Close();
                                        cmd.CommandText = "insert into Inventory (UserID, ItemID, Amount) values (@user, @item, 1)";
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
        internal async static Task SetLevel(long userId, byte level)
        {
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = "update Stats set " +
                                              "Level = (Level + 1), " +
                                              "Strength = 0, " +
                                              "Dexterity = 0, " +
                                              "Stamina = 0, " +
                                              "Luck = 0 " +
                                              "where UserID = @user";
                            cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                            await cmd.ExecuteNonQueryAsync();

                            cmd.CommandText = "update Stats set Level = @level where UserID = @user";
                            cmd.Parameters.Add("@level", DbType.Byte).Value = level;
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

        internal async static Task<string> SpawnItem(long userId, int itemId, int amount)
        {
            bool itemExists = false;
            using (SqlConnection conn = Helper.getConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tr;

                            cmd.CommandText = "select 1 from Items where ItemID = @item";
                            cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                            cmd.Parameters.Add("@amount", DbType.Int32).Value = amount;
                            
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows) itemExists = true;
                                reader.Close();
                            }

                            if (itemExists)
                            {
                                cmd.CommandText = "select 1 from Inventory where UserID = @user and ItemID = @item";
                                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Close();
                                        cmd.CommandText = "update Inventory set Amount = Amount + @amount where UserID = @user and ItemID = @item";
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    else
                                    {
                                        reader.Close();
                                        cmd.CommandText = "insert into Inventory (UserID, ItemID, Amount) values (@user, @item, @amount)";
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
