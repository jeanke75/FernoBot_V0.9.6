using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using DiscordBot.Helpers;

namespace DiscordBot.Modules.Administration
{
    class ServerAdministrationHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        public ServerAdministrationHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            Setup();
        }

        private void Setup()
        {
            service.CreateGroup("channel", cgb =>
            {
                cgb.CreateCommand("create")
                    .Description("Create a channel.")
                    .Parameter("ChannelName", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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

            service.CreateGroup("user", cgb =>
            {
                cgb.CreateCommand("kick")
                    .Description("Kick a user from the Server.")
                    .Parameter("User", ParameterType.Required)
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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

            service.CreateGroup("get", cgb =>
            {
                cgb.CreateCommand("users")
                    .Description("Users on the servers.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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
                    .AddCheck((command, user, channel) => !DiscordBot.paused)
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
    }
}
