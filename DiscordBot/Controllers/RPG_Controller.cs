using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Data;
using DiscordBot.Helpers;
using DiscordBot.Model;

namespace DiscordBot.Controllers
{
    static class RPG_Controller
    {
        internal enum Attribute { Strength, Dexterity, Stamina, Luck };

        #region Cooldowns
        internal async static Task<int> GetCooldown(User user, string command, int cooldown)
        {
            TimeSpan ts = DateTime.Now - await RPGDataHelper.GetTimeCommandUsed(user.Id, command);
            if (ts.TotalSeconds > cooldown)
            {
                return 0;
            }
            else
            {
                return (int)Math.Abs(Math.Round(ts.TotalSeconds) - cooldown);
            }
        }

        internal async static Task SetCooldown(User user, string command)
        {
            await RPGDataHelper.SetTimeCommandUsed(user.Id, command);
        }
        #endregion

        internal async static Task<string> Create(User user)
        {
            try
            {
                if (await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you've already started your adventure.";

                await RPGDataHelper.Create(user.Id);
            
                return $"{Helper.getDiscordDisplayName(user)}, your adventure has started. May the Divine spirits guide you on your adventures. (!help for a list of commands)";
            }
            catch (Exception ex)
            {
                return $"Failed to start adventure for {Helper.getDiscordDisplayName(user)} {ex.ToString()}";
            }
        }

        internal async static Task<string> Stats(User user)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)} hasn't started their adventure yet. Type !create to begin.";

            Stats stats = await RPGDataHelper.GetUserInfo(user);
            if (stats != null)
            {
                return stats.ToString();
            }
            else
            {
                return $"{Helper.getDiscordDisplayName(user)}'s stats could not be retreived.";
            }
        }

        internal async static Task<string> AssignStats(User user, Attribute type, string amount)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            int pointsToAssign;
            if (!int.TryParse(amount, out pointsToAssign)) return $"{Helper.getDiscordDisplayName(user)} tried to assign an invalid amount of attribute points.";
            if (pointsToAssign <= 0) return $"{Helper.getDiscordDisplayName(user)} tried to assign an invalid amount of attribute points.";

            string attribute;
            if (type == Attribute.Strength) {
                attribute = "Strength";
            } else if (type == Attribute.Dexterity) {
                attribute = "Dexterity";
            } else if (type == Attribute.Stamina) {
                attribute = "Stamina";
            } else if (type == Attribute.Luck) {
                attribute = "Luck";
            } else {
                attribute = "InvalidAttributeType";
            }

            // check if enough free attributes are available
            int pointsAvailable = await RPGDataHelper.GetUnusedAttributePoints(user.Id);
            if (pointsAvailable == 0)
            {
                return $"{Helper.getDiscordDisplayName(user)} tried to assign {pointsToAssign} attribute points to {attribute}, but doesn't have any available.";
            }
            else if (pointsAvailable < pointsToAssign)
            {
                return $"{Helper.getDiscordDisplayName(user)} tried to assign {pointsToAssign} attribute points to {attribute}, but only has {pointsAvailable} available.";
            }

            // assign attributes
            if (type == Attribute.Strength)
            {
                await RPGDataHelper.AssignAttributePointsToStrength(user.Id, pointsToAssign);
            }
            else if (type == Attribute.Dexterity)
            {
                await RPGDataHelper.AssignAttributePointsToDexterity(user.Id, pointsToAssign);
            }
            else if (type == Attribute.Stamina)
            {
                await RPGDataHelper.AssignAttributePointsToStamina(user.Id, pointsToAssign);
            }
            else if (type == Attribute.Luck)
            {
                await RPGDataHelper.AssignAttributePointsToLuck(user.Id, pointsToAssign);
            }
                
            return $"{Helper.getDiscordDisplayName(user)} assigned {pointsToAssign} attribute points to {attribute}.";
        }

        internal async static Task<string> Inventory(User user, string pageparam)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            int page = 0;
            int.TryParse(pageparam, out page);

            Tuple<List<InventoryItem>, int, int> result = await RPGDataHelper.GetInventory(user.Id, page);

            string header = $"+------ {Helper.getDiscordDisplayName(user)}'s Inventory (Page {result.Item2}/{result.Item3}) ------+" + Environment.NewLine;

            string s = "```diff" + Environment.NewLine + header;

            foreach (InventoryItem item in result.Item1)
            {
                s += $"| {item.name} x {item.amount}" + Environment.NewLine;
            }

            result = null;

            s += "+";
            for (int i = 0; i < header.Length - 2; i++) s += "-";

            return s + "+```";
        }

        internal async static Task<string> Equip(User user, string itemparam)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            Item item = null;
            ulong id = 0;
            if (!itemparam.Contains("+") && ulong.TryParse(itemparam, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id.";
                item = await RPGDataHelper.GetItemInfoByID(id);

                if (item == null) return $"{Helper.getDiscordDisplayName(user)} no item was found with that ID.";
            }
            else
            {
                List<Item> result = await RPGDataHelper.GetItemInfoByName(itemparam);
                if (result.Count > 1)
                {
                    if (result.Count > 25) return $"{result.Count} items found with a similar name. Please provide more info.";

                    string s = $"{result.Count} items found with a similar name:" + Environment.NewLine;
                    foreach (Item i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                if (result.Count == 1)
                {
                    item = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} no item was found with that name.";
                }
            }

            //check if item in inventory
            if (await RPGDataHelper.GetAmountOfItemOwned(user.Id, (ulong)item.id) == 0) return $"{Helper.getDiscordDisplayName(user)}, you don't have this item.";

            Stats stats = await RPGDataHelper.GetUserInfo(user);

            // check if it can be equipped
            if (!item.type.Equals("W") && !item.type.Equals("A")) return $"{Helper.getDiscordDisplayName(user)}, this item can't be equipped.";

            if (stats.equipment.isItemEquipped(item)) return $"{Helper.getDiscordDisplayName(user)}, this item is already equipped.";

            if (stats.level < item.level) return $"{Helper.getDiscordDisplayName(user)}, you're level doesn't meet the requirements.";

            await RPGDataHelper.EquipItem(user.Id, item.id, (item.type.Equals("A") ? ((Armor)item).subtype : item.type));

            return $"{Helper.getDiscordDisplayName(user)}, the {item.name} was succesfully equipped.";
        }

        internal async static Task<string> Unequip(User user, bool weapon, bool helmet, bool upper, bool pants, bool boots, bool gauntlets, bool mantle, bool shield)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            await RPGDataHelper.UnequipItems(user.Id, weapon, helmet, upper, pants, boots, gauntlets, mantle, shield);

            return $"{Helper.getDiscordDisplayName(user)}, the item was succesfully unequipped.";
        }

        internal async static Task<string> Donate(User caller, User target, string amount)
        {
            if (!await RPGDataHelper.UserStartedAdventure(caller.Id)) return $"{Helper.getDiscordDisplayName(caller)}, you haven't started your adventure yet. Type !create to begin.";
            if (caller == target) return $"{Helper.getDiscordDisplayName(caller)}, you can't donate to yourself";
            if (!await RPGDataHelper.UserStartedAdventure(target.Id)) return $"{Helper.getDiscordDisplayName(caller)}, this person hasn't started their adventure yet.";

            int goldToDonate = 0;
            if (!int.TryParse(amount, out goldToDonate) || goldToDonate <= 0) return $"{Helper.getDiscordDisplayName(caller)} tried to donate an invalid amount of gold.";

            if (await RPGDataHelper.GetGold(caller.Id) < goldToDonate) return $"{Helper.getDiscordDisplayName(caller)}, you don't have this much gold.";

            await RPGDataHelper.Donate(caller.Id, target.Id, goldToDonate);

            return $"{Helper.getDiscordDisplayName(caller)} donated {goldToDonate} gold to {Helper.getDiscordDisplayName(target)}";
        }

        internal async static Task<string> ItemInfo(User user, string param)
        {
            Item item = null;
            ulong id = 0;
            if (!param.Contains("+") && ulong.TryParse(param, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id.";
                item = await RPGDataHelper.GetItemInfoByID(id);

                if (item == null) return $"{Helper.getDiscordDisplayName(user)} no item was found with that ID.";
            }
            else
            {
                List<Item> result = await RPGDataHelper.GetItemInfoByName(param);
                if (result.Count > 1) {
                    if (result.Count > 25) return $"{result.Count} items found with a similar name. Please provide more info.";

                    string s = $"{result.Count} items found with a similar name:" + Environment.NewLine;
                    foreach(Item i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                if (result.Count == 1)
                {
                    item = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} no item was found with that name.";
                }
            }

            return item.ToString();
        }

        internal async static Task<string> MonsterInfo(User user, string param)
        {
            Monster monster = null;
            ulong id = 0;
            if (ulong.TryParse(param, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id.";
                monster = await RPGDataHelper.GetMonsterInfoByID(id);

                if (monster == null) return $"{Helper.getDiscordDisplayName(user)} no monster was found with that ID.";
            }
            else
            {
                List<Monster> result = await RPGDataHelper.GetMonsterInfoByName(param);
                
                if (result.Count > 1)
                {
                    if (result.Count > 25) return $"{result.Count} monsters found with a similar name. Please provide more info.";
                    
                    string s = $"{result.Count} monsters found with a similar name" + Environment.NewLine;
                    foreach (Monster i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                else if (result.Count == 1)
                {
                    monster = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} no monster was found with that name.";
                }
            }

            return monster.ToString();
        }

        internal async static Task<string> ShopInfo(string param)
        {
            int page = 0;
            int.TryParse(param, out page);
            //if (page <= 0) page = 1;
            Tuple<List<Item>, int, int> result = await RPGDataHelper.GetShopInfo(page);

            string header = $"+------ Shop (Page {result.Item2}/{result.Item3}) ------+";

            string s = "```diff" + Environment.NewLine +
                       header + Environment.NewLine;

            foreach (Item item in result.Item1)
            {
                s += $"| {item.name} {(item.level > 0 ? "[Lvl." + item.level + "] " : "")}- {item.valueBuy}G" + Environment.NewLine;
            }

            result = null;

            s += "+";

            for (int i = 0; i < header.Length - 2; i++) s += "-";

            return s + "+```";
        }

        internal async static Task<string> ShopBuy(User user, string amountparam, string itemparam)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            int amount = 0;
            if (!int.TryParse(amountparam, out amount) || amount <= 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid amount.";

            Item item = null;
            ulong id = 0;
            if (!itemparam.Contains("+") && ulong.TryParse(itemparam, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id. shouldn't occur";
                item = await RPGDataHelper.GetShopItemByID(id);

                if (item == null) return $"{Helper.getDiscordDisplayName(user)} the store doesn't sell an item with that ID.";
            }
            else
            {
                List<Item> result = await RPGDataHelper.GetShopItemByName(itemparam);
                if (result.Count > 1)
                {
                    if (result.Count > 25) return $"{result.Count} items found with a similar name. Please provide more info.";

                    string s = $"{result.Count} items found with a similar name:" + Environment.NewLine;
                    foreach (Item i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                if (result.Count == 1)
                {
                    item = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} the store doesn't sell an item with that name.";
                }
            }

            int price = amount * item.valueBuy;
            if (price > await RPGDataHelper.GetGold(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you don't have enough gold.";
            await RPGDataHelper.ShopBuy(user.Id, amount, item.id);

            return $"{Helper.getDiscordDisplayName(user)} bought {amount}x {item.name} for {price} gold.";
        }

        internal async static Task<string> ShopSell(User user, string amountparam, string itemparam)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            int amount = 0;
            if (!int.TryParse(amountparam, out amount) || amount <= 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid amount.";

            Item item = null;
            ulong id = 0;
            if (!itemparam.Contains("+") && ulong.TryParse(itemparam, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id.";
                item = await RPGDataHelper.GetItemInfoByID(id);

                if (item == null) return $"{Helper.getDiscordDisplayName(user)} no item was found with that ID.";
            }
            else
            {
                List<Item> result = await RPGDataHelper.GetItemInfoByName(itemparam);
                if (result.Count > 1)
                {
                    if (result.Count > 25) return $"{result.Count} items found with a similar name. Please provide more info.";

                    string s = $"{result.Count} items found with a similar name:" + Environment.NewLine;
                    foreach (Item i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                if (result.Count == 1)
                {
                    item = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} no item was found with that name.";
                }
            }

            int amountInventory = await RPGDataHelper.GetAmountOfItemOwned(user.Id, (ulong)item.id);
            if ((await RPGDataHelper.GetUserInfo(user)).equipment.isItemEquipped(item)) amountInventory--;
            if (amountInventory < amount) return $"{Helper.getDiscordDisplayName(user)} you don't have {amount} {item.name}.";

            int price = amount * item.valueSell;
            if (price > await RPGDataHelper.GetGold(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you don't have enough gold.";
            await RPGDataHelper.ShopSell(user.Id, amount, item.id);

            return $"{Helper.getDiscordDisplayName(user)} sold {amount}x {item.name} for {price} gold.";
        }

        internal async static Task<string> Attack(User user, string monsterID)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            Stats stats = await RPGDataHelper.GetUserInfo(user);
            Monster monster = await RPGDataHelper.GetFight(user.Id);
            if (monster == null)
            {
                ulong id = 0;
                if (!ulong.TryParse(monsterID, out id)) return $"{Helper.getDiscordDisplayName(user)}, you must provide a valid monster id.";
                monster = await RPGDataHelper.GetMonsterInfoByID(id);

                if (monster == null) return $"{Helper.getDiscordDisplayName(user)}, the monster you want to fight does not exist.";

                await RPGDataHelper.CreateFight(user, monster);
                return await BattleMonster(user, stats, monster, true);
            }
            else
            {
                return await BattleMonster(user, stats, monster, false);
            }
        }

        private async static Task<string> BattleMonster(User user, Stats player, Monster monster, bool isNewFight)
        {
            string s = "```diff" + Environment.NewLine +
                      $"+------ {player.name}'s fight ------+" + Environment.NewLine;
            
            // regen mob hp
            if (!isNewFight && monster.health < monster.healthMax && monster.healthRegen > 0)
            {
                int beforeRegen = monster.health;
                monster.health += monster.healthRegen;
                s += $"+ {monster.name} regenerated {monster.health - beforeRegen} health" + Environment.NewLine;
            }

            // monster deals damage
            int userDef = player.defense;
            int monsterAttack = monster.damageMinimum;
            bool monsterCritical = false;
            if (userDef < monster.defenseNoCritical)
            {
                if (userDef < monster.defenseMinimum)
                {
                    double dmgMultiplier = monster.defenseMinimum / userDef;
                    monsterAttack = (int)Math.Round(Math.Pow(dmgMultiplier, 3) * monsterAttack);
                }
                // check if critical hit
                if (Helper.rng.Next(0, 100) < 33)
                {
                    monsterCritical = true;
                    monsterAttack *= 2;
                }
            }

            player.health -= monsterAttack;
            s += $"- {monster.name} {(monsterCritical ? "crit" : "hit")} for {monsterAttack} hp damage" + Environment.NewLine;

            // deal damage if still alive
            if (player.health > 0)
            {
                int userAttack = Helper.rng.Next(player.attackMin, player.attackMax);
                bool userCritical = Helper.rng.Next(0, 100) < player.critical;
                if (userCritical)
                {
                    userAttack *= 2;
                }

                monster.health -= userAttack;
                s += $"+ {player.name} {(userCritical ? "crit" : "hit")} for {userAttack} hp damage" + Environment.NewLine;

                if (monster.health <= 0)
                {
                    s += $"+ You have {player.health}/{player.healthMax} health left" + Environment.NewLine +
                         $"+ {monster.name} has been defeated by you!" + Environment.NewLine;

                    int goldDrop = Helper.rng.Next(monster.goldMinimum, monster.goldMaximum + 1);
                    player.gold += goldDrop;

                    // add exp and check level up
                    player.experience += monster.experience;
                    if (player.experience >= player.experienceNeeded)
                    {
                        s += $"+ You got {goldDrop} gold and {monster.experience} experience (LEVEL UP)" + Environment.NewLine;
                    }
                    else
                    {
                        s += $"+ You got {goldDrop} gold and {monster.experience} experience" + Environment.NewLine;
                    }

                    while (player.experience >= player.experienceNeeded)
                    {
                        player.experience -= player.experienceNeeded;
                        player.level++;
                    }

                    //TODO get dropped item(s)
                    List<Drop> drops = await RPGDataHelper.GetKilledMonsterDrops(monster.id);
                    if (drops.Count > 0)
                    {
                        s += $"+ Items:";
                        foreach (Drop drop in drops) s += $" {drop.name} x{drop.amount},";
                        s = s.Substring(0, s.Length - 1) + Environment.NewLine; // remove last comma and add newline
                    }
                    await RPGDataHelper.BattleFinished(user, player, monster, drops);
                }
                else
                {
                    s += $"+ You have {player.health}/{player.healthMax} health left" + Environment.NewLine +
                         $"+ {monster.name} has {monster.health}/{monster.healthMax} health left" + Environment.NewLine;
                    await RPGDataHelper.BattleFinished(user, player, monster, null);
                }
            }
            else
            {
                player.health = player.healthMax;
                int expLost = (int)Math.Round(player.experienceNeeded * 0.1);
                int goldLost = (int)Math.Round(player.gold * 0.1);
                player.experience = (player.experience - expLost >= 0 ? player.experience - expLost : 0);
                player.gold -= goldLost;
                s += $"- You have been killed and lost {goldLost} gold and {expLost} experience!" + Environment.NewLine;

                await RPGDataHelper.BattleFinished(user, player, monster, null);
            }

            s += "+-------";

            for (int i = 0; i < player.name.Length; i++) s += "-";

            return s + "---------------+```";
        }

        internal async static Task<string> Heal(User user)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            Stats stats = await RPGDataHelper.GetUserInfo(user);

            int health = stats.health;
            int healthMax = stats.healthMax;
            if (health >= healthMax) return $"{Helper.getDiscordDisplayName(user)}, you're already fully healed.";

            Potion potion = await RPGDataHelper.GetBestPotionInInventory(user.Id);

            if (potion == null) return $"{Helper.getDiscordDisplayName(user)}, you don't have any potions.";

            stats.health += potion.heal;
            if (stats.health > healthMax) stats.health = healthMax;

            await RPGDataHelper.Heal(user.Id, stats.health, potion.id);

            return $"{Helper.getDiscordDisplayName(user)}, you've used a {potion.name} and recovered {stats.health - health} health. ({stats.health}/{stats.healthMax})";
        }

        internal async static Task<string> Craft(User user, string itemparam, int type)
        {
            if (!await RPGDataHelper.UserStartedAdventure(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you haven't started your adventure yet. Type !create to begin.";

            Item item = null;
            ulong id = 0;
            if (!itemparam.Contains("+") && ulong.TryParse(itemparam, out id))
            {
                if (id < 0) return $"{Helper.getDiscordDisplayName(user)} please provide a valid id.";
                item = await RPGDataHelper.GetItemInfoByID(id);

                if (item == null) return $"{Helper.getDiscordDisplayName(user)} no item was found with that ID.";
            }
            else
            {
                List<Item> result = await RPGDataHelper.GetItemInfoByName(itemparam);
                if (result.Count > 1)
                {
                    if (result.Count > 25) return $"{result.Count} items found with a similar name. Please provide more info.";

                    string s = $"{result.Count} items found with a similar name:" + Environment.NewLine;
                    foreach (Item i in result)
                    {
                        s += "`" + i.name + "`, ";
                    }
                    return s.Substring(0, s.Length - 2);
                }
                if (result.Count == 1)
                {
                    item = result[0];
                }
                else
                {
                    return $"{Helper.getDiscordDisplayName(user)} no item was found with that name.";
                }
            }

            //check if item can be crafted and get required items to craft
            Tuple<List<Tuple<Item, long, long>>, int, int> craftInfo = await RPGDataHelper.GetRequirementsCraftedItem(user.Id, item.id);

            if (craftInfo == null) return $"{Helper.getDiscordDisplayName(user)}, this item can't be crafted.";

            //check if corresponding saviour orb is in inventory
            long savOrbId = 0;
            if (type != 0)
            {
                //get item id
                List<Item> result = await RPGDataHelper.GetItemInfoByName("Saviour Orb L" + type);
                if (result.Count > 1)
                {
                    return "problem with saviour orb names!!";
                }
                if (result.Count == 1)
                {
                    savOrbId = result[0].id;
                    if (await RPGDataHelper.GetAmountOfItemOwned(user.Id, (ulong)savOrbId) < 1) return $"{Helper.getDiscordDisplayName(user)}, you need to have a Saviour Orb L{type} to do this.";
                }
                else
                {
                    return "problem 2 with saviour orb names!!";
                }
            }

            //check if all the items are owned
            string missingItems = "";
            string usedItems = "";
            foreach(Tuple<Item, long, long> req in craftInfo.Item1)
            {
                usedItems += string.Format("{0} x{1}, ", req.Item1.name, req.Item2);
                if (req.Item2 > req.Item3) missingItems += string.Format("{0} x{1}, ", req.Item1.name, req.Item2 - req.Item3);
            }

            if (!missingItems.Equals("")) return $"{Helper.getDiscordDisplayName(user)}, you are missing some items: {missingItems.Substring(0,missingItems.Length-2)}";

            //check if enough gold is owned
            if (craftInfo.Item2 > await RPGDataHelper.GetGold(user.Id)) return $"{Helper.getDiscordDisplayName(user)}, you don't have enough gold.";

            string status = $"{Helper.getDiscordDisplayName(user)}'s attempt to craft {item.name} ";

            //check if upgrade is a success
            int x = Helper.rng.Next(0, 100);
            bool success = x < craftInfo.Item3;
            
            if (success)
            {
                status += "was a `SUCCESS`!";
                await Console.Out.WriteLineAsync(x.ToString());
            }
            else
            {
                int y = -1;
                if (type == 1)
                {
                    y = Helper.rng.Next(0, 100);
                    success = y < 10;
                }
                else if (type == 2)
                {
                    y = Helper.rng.Next(0, 100);
                    success = y < 20;
                }
                else if (type == 3)
                {
                    y = Helper.rng.Next(0, 100);
                    success = y < 40;
                }
                else if (type == 4)
                {
                    y = Helper.rng.Next(0, 100);
                    success = y < 60;
                }
                else if (type == 5)
                {
                    y = Helper.rng.Next(0, 100);
                    success = y < 80;
                }

                if (success)
                {
                    status += "was a `SUCCESS` because of the Saviour Orb!";
                    await Console.Out.WriteLineAsync(x.ToString() + " " + y.ToString());
                }
                else
                {
                    if (type == 0)
                    {
                        status += "`FAILED`.";
                        await Console.Out.WriteLineAsync(x.ToString());
                    }
                    else
                    {
                        status += "`FAILED`, even with the Saviour Orb.";
                        await Console.Out.WriteLineAsync(x.ToString() + " " + y.ToString());
                    }
                }
                
            }

            await RPGDataHelper.Craft(user.Id, savOrbId, item.id, success);

            if (type == 0)
            {
                return status + Environment.NewLine + $"{usedItems.Substring(0, usedItems.Length - 2)} and {craftInfo.Item2} gold was used.";
            }
            else
            {
                return status + Environment.NewLine + $"1x Saviour Orb L{type}, {usedItems.Substring(0, usedItems.Length - 2)} and {craftInfo.Item2} gold was used.";
            }
        }
    }
}

