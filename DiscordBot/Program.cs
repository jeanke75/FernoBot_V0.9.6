using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Commands;
using DiscordBot.Controllers;
using DiscordBot.Helpers;

namespace DiscordBot
{
    public class Program
    {
        private readonly string _token = ConfigurationManager.AppSettings["TOKEN"];
        private readonly string _appName = ConfigurationManager.AppSettings["APP_NAME"];
        private static bool paused = false;

        public static void Main(string[] args)
        {
            new Program().Start();
        }

        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient(x =>
            {
                x.AppName = this._appName;
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = Log;
            });

            InitializeCommands();
            UserEventTriggers();

            // log all chat
            /*_client.MessageReceived += async (s, e) =>
            {on 
                if (!e.Message.IsAuthor)
                {
                    await Console.Out.WriteLineAsync(e.Message.User + ": " + e.Message.Text);
                }
            };*/

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(_token, TokenType.Bot); // bot-token
                _client.SetGame("!help");
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(string.Format("[{0}] [{1}] {2}", e.Severity, e.Source, e.Message));
        }

        public void UserEventTriggers()
        {
            _client.UserJoined += async (s, e) =>
            {
                // Create a Channel object by searching for a channel named '#log' on the server the ban occurred in.
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();
                // Send a message to the server's log channel, stating that a user was banned.
                await logChannel.SendMessage($"User Joined: {e.User.Name}");
            };

            _client.UserLeft += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Left: {e.User.Name}");
            };

            _client.UserBanned += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Banned: {e.User.Name}");
            };

            _client.UserUnbanned += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Unbanned: {e.User.Name}");
            };
        }

        public void InitializeCommands()
        {
            _client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Private;
            });

            _client.GetService<CommandService>().CreateCommand("ping")
                .Description("Will respond with pong if the bot is up.")
                .AddCheck((command, user, channel) => !paused)
                .Do(async e =>
                {
                    try
                    {
                        await e.Channel.SendMessage("Pong");
                        _client.Log.Debug("test", null);
                    }
                    catch(Exception ex)
                    {
                        await e.Channel.SendMessage(ex.ToString());
                    }
                });

            CreateModerationCommands();
            CreateSocialCommands();
            CreateRPGCommands();
        }

        private void CreateModerationCommands()
        {
            _client.GetService<CommandService>().CreateGroup("channel", cgb =>
            {
                cgb.CreateCommand("create")
                    .Description("Create a channel.")
                    .Parameter("ChannelName", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.ManageChannels)
                            {
                                await e.Server.CreateChannel($"{e.GetArg("ChannelName")}", ChannelType.Text);
                                await e.Channel.SendMessage($"{e.User.Name} created the channel {e.GetArg("ChannelName")}");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name} you don't have the permission to do this.");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync(ex.Message);
                        }
                    });

                cgb.CreateCommand("delete")
                    .Description("Delete a channel.")
                    .Parameter("ChannelName", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.ManageChannels)
                            {
                                await e.Server.FindChannels($"{e.GetArg("ChannelName")}", ChannelType.Text).FirstOrDefault().Delete();
                                await e.Channel.SendMessage($"{e.User.Name} deleted the channel {e.GetArg("ChannelName")}");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name} you don't have the permission to do this.");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync(ex.Message);
                        }
                    });
            });

            _client.GetService<CommandService>().CreateGroup("user", cgb =>
            {
                cgb.CreateCommand("kick")
                    .Description("Kick a user from the Server.")
                    .Parameter("User", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.KickMembers)
                            {
                                User user = null;
                                try
                                {
                                    user = e.Server.FindUsers(e.GetArg("User")).First();
                                }
                                catch (InvalidOperationException)
                                {
                                    await e.Channel.SendMessage($"Couldn't kick user {e.GetArg("User")} (not found).");
                                    return;
                                }

                                if (user == null) await e.Channel.SendMessage($"Couldn't kick user {e.GetArg("User")} (not found).");
                                await user.Kick();
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(user)} was kicked from the server!");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name} you don't have the permission to kick.");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync(ex.Message);
                        }
                    });

                cgb.CreateCommand("unban")        //controleren op user ID
                    .Description("Unban a user from the Server.")
                    .Parameter("User", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.BanMembers)
                            {
                                User userToUnban = null;
                                try
                                {
                                    userToUnban = e.Server.FindUsers(e.GetArg("User")).First();
                                }
                                catch (InvalidOperationException)
                                {
                                    await e.Channel.SendMessage($"Couldn't unban user {e.GetArg("User")} (not found).");
                                    return;
                                }

                                var bannedUsers = await e.Server.GetBans();

                                try
                                {
                                    var user = bannedUsers.Single(x => x == userToUnban);
                                }
                                catch (InvalidOperationException)
                                {
                                    await e.Channel.SendMessage($"User {e.GetArg("User")} isn't banned.");
                                    return;
                                }
                                
                                await e.Server.Unban(userToUnban.Id);
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(userToUnban)} was unbanned from the server!");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name} you don't have the permission to unban.");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync(ex.Message);
                        }
                    });

                cgb.CreateCommand("ban")
                    .Description("Ban a user from the Server.")
                    .Parameter("User", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.BanMembers)
                            {
                                User user = null;
                                try
                                {
                                    user = e.Server.FindUsers(e.GetArg("User")).First();
                                }
                                catch (InvalidOperationException)
                                {
                                    await e.Channel.SendMessage($"Couldn't ban user {e.GetArg("User")} (not found).");
                                    return;
                                }
                                if (user == null) await e.Channel.SendMessage($"Couldn't ban user {e.GetArg("User")} (not found).");
                                await e.Server.Ban(user, 0);
                                await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(user)} was banned from the server!");
                            }
                            else
                            {
                                await e.Channel.SendMessage($"{e.User.Name} you don't have the permission to ban.");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync(ex.Message);
                        }
                    });
            });

            _client.GetService<CommandService>().CreateGroup("get", cgb =>
            {
                cgb.CreateCommand("users")
                    .Description("Users on the servers.")
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        IEnumerable<User> users = e.Server.Users;
                        string s = "```----- USERS -----";
                        foreach (User u in users)
                        {
                            s += Environment.NewLine + u.ToString();
                        }
                        s += Environment.NewLine + "-----------------```";
                        await e.Channel.SendMessage(s);
                    });

                cgb.CreateCommand("banned")
                    .Description("Banned users on the servers.")
                    .AddCheck((command, user, channel) => !paused)
                    .Do(async e =>
                    {
                        var users = await e.Server.GetBans();
                        string s = "```----- BANNED USERS -----";
                        foreach (User u in users)
                        {
                            s += Environment.NewLine + u.Id;
                        }
                        s += Environment.NewLine + "-----------------```";
                        await e.Channel.SendMessage(s);
                    });
            });
        }

        private void CreateSocialCommands()
        {
            _client.GetService<CommandService>().CreateCommand("hug")
                .Description("Give someone a big hug.")
                .Parameter("Person", ParameterType.Required)
                .AddCheck((command, user, channel) => !paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} hugs {e.GetArg("Person")}");
                });

            _client.GetService<CommandService>().CreateCommand("kill")
                .Description("Kill yourself or someone else in one of many random ways.")
                .Parameter("Person", ParameterType.Optional)
                .AddCheck((command, user, channel) => !paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(Social_Controller.Kill(e.User, e.GetArg("Person")));
                });

            _client.GetService<CommandService>().CreateCommand("dice")
                .Alias(new string[] { "rnd", "random", "roll" })
                .Description($"Generate a random number between 2 values.```!roll -> 1 - 6{Environment.NewLine}!roll <n> -> 1 - <n>{Environment.NewLine}!roll <n1> <n2> -> <n1> - <n2>```")
                .Parameter("Number", ParameterType.Optional)
                .Parameter("Number2", ParameterType.Optional)
                .AddCheck((command, user, channel) => !paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(Social_Controller.Dice(e.User, e.GetArg("Number"), e.GetArg("Number2")));
                });
        }

        private void CreateRPGCommands()
        {
            _client.GetService<CommandService>().CreateCommand("start")
                .Alias(new string[] { "create", "begin" })
                .Description("Start your adventure.")
                .AddCheck((command, user, channel) => !paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(await RPG_Controller.Create(e.Message.User));
                });
            
            _client.GetService<CommandService>().CreateCommand("stats")
                .Description("Check your stats.")
                .Parameter("Person", ParameterType.Optional)
                .AddCheck((command, user, channel) => !paused)
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
                            await RPG_Controller.SetCooldown(e.Message.User, "stats");
                            User user = Helper.getUserFromMention(e.Server, e.GetArg("Person"));
                            await e.Channel.SendMessage(await RPG_Controller.Stats((user != null ? user : e.Message.User)));
                        }
                    }
                    catch(Exception ex)
                    {
                        await e.Channel.SendMessage($"```{ex.ToString()}```");
                    }
                });

            _client.GetService<CommandService>().CreateGroup("assign", cgb =>
            {
                cgb.CreateCommand("strength")
                    .Alias(new string[] { "str" })
                    .Description("Assign an amount of stat points to Strength.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.Attribute.Strength, e.GetArg("Amount")));
                            }
                        }
                        catch(Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("dexterity")
                    .Alias(new string[] { "dex" })
                    .Description("Assign an amount of stat points to Dexterity.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.Attribute.Dexterity, e.GetArg("Amount")));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.Attribute.Stamina, e.GetArg("Amount")));
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
                
                cgb.CreateCommand("sense")
                    .Alias(new string[] { "sns" })
                    .Description("Assign an amount of stat points to Sense.")
                    .Parameter("Amount", ParameterType.Required)
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "assign");
                                await e.Channel.SendMessage(await RPG_Controller.AssignStats(e.Message.User, RPG_Controller.Attribute.Luck, e.GetArg("Amount")));
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to assign attributes for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            _client.GetService<CommandService>().CreateCommand("inventory")
                .Alias(new string[] { "inv", "bag" })
                .Description("Show a list of all the items in your inventory.")
                .Parameter("Page", ParameterType.Optional)
                .AddCheck((command, user, channel) => !paused)
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
                            await RPG_Controller.SetCooldown(e.Message.User, "inventory");
                            await e.Channel.SendMessage(await RPG_Controller.Inventory(e.Message.User, e.GetArg("Page")));
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed retrieve inventory for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });
            
            _client.GetService<CommandService>().CreateCommand("equip")
                .Description("Equip a certain item from your inventory using the ID.")
                .Parameter("Item", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Equip(e.Message.User, e.GetArg("Item").Trim()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to equip an item on {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            _client.GetService<CommandService>().CreateGroup("unequip", cgb =>
            {
                cgb.CreateCommand("all")
                    .Description("Unequip all the items currently equipped.")
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, true, true, true, true, true, true, true, true));
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip items for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("weapon")
                    .Description("Unequip the item currently equipped in the weapon slot.")
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, true, false, false, false, false, false, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, true, false, false, false, false, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, true, false, false, false, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, true, false, false, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, true, false, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, true, false, false));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, false, true, false));
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip mantle for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });

                cgb.CreateCommand("shield")
                    .Description("Unequip the item currently equipped in the shield slot.")
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "equip");
                                await e.Channel.SendMessage(await RPG_Controller.Unequip(e.Message.User, false, false, false, false, false, false, false, true));
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to unequip shield for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            _client.GetService<CommandService>().CreateCommand("donate")
                .Alias(new string[] { "give" })
                .Description("Share some of your wealth with others.")
                .Parameter("Person", ParameterType.Required)
                .Parameter("Amount", ParameterType.Required)
                .AddCheck((command, user, channel) => !paused)
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
                            await RPG_Controller.SetCooldown(e.Message.User, "donate");
                            await e.Channel.SendMessage(await RPG_Controller.Donate(e.Message.User, Helper.getUserFromMention(e.Server, e.GetArg("Person")), e.GetArg("Amount")));
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} failed to donate gold ```{ex.ToString()}```");
                    }
                });
            
            _client.GetService<CommandService>().CreateCommand("item")
                .Alias(new string[] { "itm" })
                .Description("Get info about a certain item using the ID.")
                .Parameter("Item", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "info");
                                await e.Channel.SendMessage(await RPG_Controller.ItemInfo(e.Message.User, e.GetArg("Item").Trim()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to retrieve info about an item for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });
            
            _client.GetService<CommandService>().CreateCommand("monster")
                .Alias(new string[] { "mob" })
                .Description("Get info about a certain monster using the ID.")
                .Parameter("Monster", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "info");
                                await e.Channel.SendMessage(await RPG_Controller.MonsterInfo(e.Message.User, e.GetArg("Monster").Trim()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"Failed to retrieve info about a monster for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                    }
                });

            _client.GetService<CommandService>().CreateGroup("shop", cgb =>
            {
                cgb.CreateCommand("list")
                    .Alias(new string[] { "lst", "info" })
                    .Description("Get a list of the items available in the store.")
                    .Parameter("Page", ParameterType.Optional)
                    .AddCheck((command, user, channel) => !paused)
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
                                await RPG_Controller.SetCooldown(e.Message.User, "shop");
                                await e.Channel.SendMessage(await RPG_Controller.ShopInfo(e.GetArg("Page")));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "shop");
                                    await e.Channel.SendMessage(await RPG_Controller.ShopBuy(e.Message.User, e.GetArg("Amount"), e.GetArg("Item").Trim()));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "shop");
                                    await e.Channel.SendMessage(await RPG_Controller.ShopSell(e.Message.User, e.GetArg("Amount"), e.GetArg("Item").Trim()));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage($"Failed to sell an item to the store for {Helper.getDiscordDisplayName(e.Message.User)} ```{ex.ToString()}```");
                        }
                    });
            });

            _client.GetService<CommandService>().CreateCommand("attack")
                .Alias(new string[] { "att", "atk", "fight" })
                .Description("Attack a new monster or continue your current fight.")
                .Parameter("Monster", ParameterType.Optional)
                .AddCheck((command, user, channel) => !paused)
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
                            await RPG_Controller.SetCooldown(e.Message.User, "attack");
                            await e.Channel.SendMessage(await RPG_Controller.Attack(e.Message.User, e.GetArg("Monster")));
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)}'s attack failed ```{ex.ToString()}```");
                    }
                });

            _client.GetService<CommandService>().CreateCommand("heal")
                .Description("Recover health by using the strongest health potion in your inventory.")
                .AddCheck((command, user, channel) => !paused)
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
                            await RPG_Controller.SetCooldown(e.Message.User, "heal");
                            await e.Channel.SendMessage(await RPG_Controller.Heal(e.Message.User));
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} failed to heal ```{ex.ToString()}```");
                    }
                });

            _client.GetService<CommandService>().CreateGroup("craft", cgb =>
            {
                cgb.CreateCommand()
                    .Description("Craft an item. Be carefull, because if it fails you lose all the items and gold required.")
                    .Parameter("Item", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 0));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 1));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 2));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 3));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 4));
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
                    .AddCheck((command, user, channel) => !paused)
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
                                    await RPG_Controller.SetCooldown(e.Message.User, "craft");
                                    await e.Channel.SendMessage(await RPG_Controller.Craft(e.Message.User, e.GetArg("Item").Trim(), 5));
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
            _client.GetService<CommandService>().CreateCommand("level")
                .Description("Set level of the user to the level specified. (testing only!)")
                .Parameter("Person", ParameterType.Required)
                .Parameter("Level", ParameterType.Required)
                .AddCheck((command, user, channel) => !paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    try
                    {
                        await Data.RPGDataHelper.setLevel(Helper.getUserFromMention(e.Server, e.GetArg("Person")).Id, int.Parse(e.GetArg("Level")));
                        await e.Channel.SendMessage($"{e.GetArg("Person")} set to level {int.Parse(e.GetArg("Level"))}");
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} set level failed {ex.ToString()}");
                    }
               });

            _client.GetService<CommandService>().CreateCommand("spawn")
                .Description("Spawn an item in your inventory. (testing only!)")
                .Parameter("Item", ParameterType.Required)
                .Parameter("Amount", ParameterType.Required)
                .AddCheck((command, user, channel) => !paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    try
                    {
                        await e.Channel.SendMessage(e.Message.User.Nickname + await Data.RPGDataHelper.SpawnItem(e.Message.User.Id, long.Parse(e.GetArg("Item")), int.Parse(e.GetArg("Amount"))));
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"{Helper.getDiscordDisplayName(e.Message.User)} item spawning failed {ex.ToString()}");
                    }
                });

            _client.GetService<CommandService>().CreateCommand("pause")
                .Description("The bot will stop responding to commands untill unpaused.")
                .AddCheck((command, user, channel) => !paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    paused = true;
                    await e.Channel.SendMessage("The bot has been paused and will stop responding to commands!");
                });
            
            _client.GetService<CommandService>().CreateCommand("unpause")
                .Description("The bot will respond to commands again.")
                .AddCheck((command, user, channel) => paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    paused = false;
                    await e.Channel.SendMessage("The bot has been unpaused and will respond to commands again.");
                });

            _client.GetService<CommandService>().CreateCommand("stop")
                .Description("Stop the program")
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Hide()
                .Do(e =>
                {
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                });
            #endregion

            _client.GetService<CommandService>().CreateCommand("sql")
                .Parameter("SQL", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Hide()
                .Do(async (e) =>
                {
                    if (!e.GetArg("SQL").Trim().Equals(""))
                    {
                        await e.Channel.SendMessage(e.GetArg("SQL").Trim());
                        try
                        {
                            using (System.Data.SqlClient.SqlConnection conn = Helper.getConnection())
                            {
                                await conn.OpenAsync();
                                using (System.Data.SqlClient.SqlTransaction tr = conn.BeginTransaction())
                                {
                                    using (System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.Transaction = tr;

                                        cmd.CommandText = e.GetArg("SQL").Trim();
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    tr.Commit();
                                }
                            }
                            await e.Channel.SendMessage("success");
                        }
                        catch (Exception ex)
                        {
                            e.Channel.SendMessage(ex.ToString());
                        }
                    }
                });

            _client.GetService<CommandService>().CreateCommand("tables")
                .Hide()
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Do(async (e) =>
                {
                    try
                    {
                        using (System.Data.SqlClient.SqlConnection conn = Helper.getConnection())
                        {
                            await conn.OpenAsync();
                            System.Data.DataTable schemaDataTable = conn.GetSchema("Tables");
                            string colums = "";
                            foreach (System.Data.DataColumn column in schemaDataTable.Columns)
                            {
                                colums += column.ColumnName + "\t";
                            }
                            await e.Channel.SendMessage(colums);
                            foreach (System.Data.DataRow row in schemaDataTable.Rows)
                            {
                                string rows = "";
                                foreach (object value in row.ItemArray)
                                {
                                    rows += value.ToString() + "\t";
                                }
                                await e.Channel.SendMessage(rows);
                            }
                            await e.Channel.SendMessage("-----done-----");
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Channel.SendMessage(ex.ToString());
                    }
                });
        }
    }
}
