using System;
using Discord;
using Discord.Commands;
using DiscordBot.Helpers;
using DiscordBot.Modules.Games.RPG.Controller;

namespace DiscordBot.Modules.Games.RPG
{
    class RPGHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        public RPGHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            Setup();
        }

        private void Setup()
        {
            service.CreateCommand("start")
                    .Alias(new string[] { "create", "begin" })
                    .Description("Start your adventure.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            await e.Channel.SendMessage(await RPG_Controller.Create(e.Message.User));
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage(ex.Message);
                        }
                    });

            service.CreateCommand("stats")
                .Description("Check your stats.")
                .Parameter("Person", ParameterType.Optional)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        int cd = await RPG_Controller.GetCooldown(e.Message.User, "stats", 15);
                        if (cd > 0)
                        {
                            await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                        }
                        else
                        {
                            User user = Helper.getUserFromMention(e.Server, e.GetArg("Person"));
                            await e.Channel.SendMessage(await RPG_Controller.Stats((user != null ? user : e.Message.User)));
                            await RPG_Controller.SetCooldown(e.Message.User, "stats");
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"```{ex.ToString()}```");
                    }
                });

            service.CreateGroup("assign", cgb =>
            {
                cgb.CreateCommand("strength")
                    .Alias(new string[] { "str" })
                    .Description("Assign an amount of stat points to Strength.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "assign", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.CharacterAttribute.Strength, e.GetArg("Amount")));
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("dexterity")
                    .Alias(new string[] { "dex" })
                    .Description("Assign an amount of stat points to Dexterity.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "assign", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.CharacterAttribute.Dexterity, e.GetArg("Amount")));
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("stamina")
                    .Alias(new string[] { "sta", "stam" })
                    .Description("Assign an amount of stat points to Stamina")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "assign", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.CharacterAttribute.Stamina, e.GetArg("Amount")));
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("luck")
                    .Alias(new string[] { "lck" })
                    .Description("Assign an amount of stat points to Luck.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "assign", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.CharacterAttribute.Luck, e.GetArg("Amount")));
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            service.CreateCommand("inventory")
                .Alias(new string[] { "inv", "bag" })
                .Description("Show a list of all the items in your inventory.")
                .Parameter("Page", ParameterType.Optional)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        int cd = await RPG_Controller.GetCooldown(e.Message.User, "inventory", 15);
                        if (cd > 0)
                        {
                            await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                        }
                        else
                        {
                            await e.Channel.SendMessage(await RPG_Controller.Inventory(e.Message.User, e.GetArg("Page")));
                            await RPG_Controller.SetCooldown(e.Message.User, "inventory");
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed retrieve inventory for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            service.CreateCommand("equip")
                .Description("Equip a certain item from your inventory using the ID.")
                .Parameter("Item", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        if (!e.GetArg("Item").Trim().Equals(""))
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Equip(e.Message.User, e.GetArg("Item").Trim()));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to equip an item on {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            service.CreateGroup("unequip", cgb =>
            {
                cgb.CreateCommand("all")
                    .Description("Unequip all the items currently equipped.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, true, true, true, true, true, true, true, true));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip items for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("weapon")
                    .Description("Unequip the item currently equipped in the weapon slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, true, false, false, false, false, false, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip weapon for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("helmet")
                    .Alias(new string[] { "helm", "head", "hat" })
                    .Description("Unequip the item currently equipped in the helmet slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, true, false, false, false, false, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip helmet for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("upper")
                    .Alias(new string[] { "body", "chest", "top" })
                    .Description("Unequip the item currently equipped in the upper slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, true, false, false, false, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip upper for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("pants")
                    .Alias(new string[] { "legs", "trousers" })
                    .Description("Unequip the item currently equipped in the pants slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, true, false, false, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip pants for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("boots")
                    .Alias(new string[] { "feet", "shoes" })
                    .Description("Unequip the item currently equipped in the upper slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, true, false, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip boots for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("gauntlets")
                    .Alias(new string[] { "gloves", "hands" })
                    .Description("Unequip the item currently equipped in the gauntlets slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, true, false, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip gauntlets for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("mantle")
                    .Alias(new string[] { "cape" })
                    .Description("Unequip the item currently equipped in the mantle slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, false, true, false));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip mantle for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("shield")
                    .Description("Unequip the item currently equipped in the shield slot.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "equip", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, false, false, true));
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip shield for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            service.CreateCommand("donate")
                .Alias(new string[] { "give" })
                .Description("Share some of your wealth with others.")
                .Parameter("Person", ParameterType.Required)
                .Parameter("Amount", ParameterType.Required)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        int cd = await RPG_Controller.GetCooldown(e.Message.User, "donate", 10);
                        if (cd > 0)
                        {
                            await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                        }
                        else
                        {
                            await e.Channel.SendMessage(await RPG_Controller.Donate(e.Message.User, Helper.getUserFromMention(e.Server, e.GetArg("Person")), e.GetArg("Amount")));
                            await RPG_Controller.SetCooldown(e.Message.User, "donate");
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} failed to donate gold ```{ex.ToString()}```");
                    }
                });

            service.CreateCommand("item")
                .Alias(new string[] { "itm" })
                .Description("Get info about a certain item using the ID.")
                .Parameter("Item", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        if (!e.GetArg("Item").Trim().Equals(""))
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "info", 15);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.ItemInfo(e.Message.User, e.GetArg("Item").Trim()));
                                await RPG_Controller.SetCooldown(e.Message.User, "info");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to retrieve info about an item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            service.CreateCommand("monster")
                .Alias(new string[] { "mob" })
                .Description("Get info about a certain monster using the ID.")
                .Parameter("Monster", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        if (!e.GetArg("Monster").Trim().Equals(""))
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "info", 15);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.MonsterInfo(e.Message.User, e.GetArg("Monster").Trim()));
                                await RPG_Controller.SetCooldown(e.Message.User, "info");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to retrieve info about a monster for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            service.CreateGroup("shop", cgb =>
            {
                cgb.CreateCommand("list")
                    .Alias(new string[] { "lst", "info" })
                    .Description("Get a list of the items available in the store.")
                    .Parameter("Page", ParameterType.Optional)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            int cd = await RPG_Controller.GetCooldown(e.Message.User, "shop", 10);
                            if (cd > 0)
                            {
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(await RPG_Controller.ShopInfo(e.GetArg("Page")));
                                await RPG_Controller.SetCooldown(e.Message.User, "shop");
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to retrieve the items in the store for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("buy")
                    .Description("Buy an item from the shop.")
                    .Parameter("Amount", ParameterType.Required)
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "shop", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.ShopBuy(e.Message.User, e.GetArg("Amount"), e.GetArg("Item").Trim()));
                                    await RPG_Controller.SetCooldown(e.Message.User, "shop");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to buy an item from the store for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("sell")
                    .Description("Sell an item to the store. This CAN'T be undone.")
                    .Parameter("Amount", ParameterType.Required)
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "shop", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.ShopSell(e.Message.User, e.GetArg("Amount"), e.GetArg("Item").Trim()));
                                    await RPG_Controller.SetCooldown(e.Message.User, "shop");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to sell an item to the store for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            service.CreateCommand("attack")
                .Alias(new string[] { "att", "atk", "fight" })
                .Description("Attack a new monster or continue your current fight.")
                .Parameter("Monster", ParameterType.Optional)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        int cd = await RPG_Controller.GetCooldown(e.Message.User, "attack", 10);
                        if (cd > 0)
                        {
                            await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                        }
                        else
                        {
                            await e.Channel.SendMessage(await RPG_Controller.Attack(e.Message.User, e.GetArg("Monster")));
                            await RPG_Controller.SetCooldown(e.Message.User, "attack");
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}'s attack failed ```{ex.ToString()}```");
                    }
                });

            service.CreateCommand("heal")
                .Description("Recover health by using the strongest health potion in your inventory.")
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        int cd = await RPG_Controller.GetCooldown(e.Message.User, "heal", 30);
                        if (cd > 0)
                        {
                            await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                        }
                        else
                        {
                            await e.Channel.SendMessage(await RPG_Controller.Heal(e.Message.User));
                            await RPG_Controller.SetCooldown(e.Message.User, "heal");
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} failed to heal ```{ex.ToString()}```");
                    }
                });

            service.CreateGroup("craft", cgb =>
            {
                cgb.CreateCommand()
                    .Description("Craft an item. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 0));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("L1")
                    .Description("Craft an item using a Saviour Orb L1. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 1));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("L2")
                    .Description("Craft an item using a Saviour Orb L2. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 2));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("L3")
                    .Description("Craft an item using a Saviour Orb L3. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 3));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("L4")
                    .Description("Craft an item using a Saviour Orb L4. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 4));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("L5")
                    .Description("Craft an item using a Saviour Orb L5. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (!e.GetArg("Item").Trim().Equals(""))
                            {
                                int cd = await RPG_Controller.GetCooldown(e.Message.User, "craft", 10);
                                if (cd > 0)
                                {
                                    await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}, {cd} seconds before you can use this command again.");
                                }
                                else
                                {
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 5));
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to craft item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            #region test only commands
            service.CreateCommand("level")
                .Description("Set level of the user to the level specified. (testing only!)")
                .Parameter("Person", ParameterType.Required)
                .Parameter("Level", ParameterType.Required)
                .AddCheck((command, user, channel) => !DiscordBot.paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    try
                    {
                        await e.Channel.SendMessage(await RPG_Controller.SetUserLevel(Helper.getUserFromMention(e.Server, e.GetArg("Person")), e.GetArg("Level")));
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} set level failed {ex.ToString()}");
                    }
                });

            service.CreateCommand("spawn")
                .Description("Spawn an item in your inventory. (testing only!)")
                .Parameter("Person", ParameterType.Required)
                .Parameter("Item", ParameterType.Required)
                .Parameter("Amount", ParameterType.Required)
                .AddCheck((command, user, channel) => !DiscordBot.paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    try
                    {
                        await e.Channel.SendMessage(await RPG_Controller.SpawnItemForUser(e.Message.User, Helper.getUserFromMention(e.Server, e.GetArg("Person")), e.GetArg("Item"), e.GetArg("Amount")));
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} item spawning failed {ex.ToString()}");
                    }
                });
            #endregion
        }
    }
}