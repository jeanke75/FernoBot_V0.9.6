using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordBot.Modules.Info
{
    class InfoHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        private DateTime start;
        public InfoHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            start = DateTime.Now;
            Setup();
        }

        public void Setup()
        {
            service.CreateCommand("botinfo")
                    .Description("Provides info about the bot.")
                    .Do(async e =>
                    {
                        try
                        {
                            var servers = client.Servers;
                            int servercount = servers.Count();
                            long usercount = 0;
                            int channelcount = 0;
                            foreach (Server server in servers)
                            {
                                usercount = usercount + server.UserCount;
                                channelcount = channelcount + server.ChannelCount;
                            }

                            TimeSpan uptime = DateTime.Now.Subtract(start);

                            string message = "```ini" + Environment.NewLine +
                                             "Bot info:" + Environment.NewLine +
                                            $"[Uptime]   {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s" + Environment.NewLine +
                                            $"[Servers]  {servercount}" + Environment.NewLine +
                                            $"[Channels] {channelcount}" + Environment.NewLine +
                                            $"[Users]    {usercount}" + Environment.NewLine +
                                            $"[Status]   {(DiscordBot.paused ? "Paused" : "Active")}" +
                                             "```";

                            await e.Channel.SendMessage(message);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage(ex.Message);
                        }
                    });

            service.CreateCommand("serverinfo")
                    .Description("Provides info about the server.")
                    .Do(async e =>
                    {
                        try
                        {
                            List<string> roles = e.Server.Roles.Where(x => !x.IsEveryone).Select(x => x.Name).ToList();
                            string message = "```ini" + Environment.NewLine +
                                             "Server info:" + Environment.NewLine +
                                            $"[Name]            {e.Server.Name}" + Environment.NewLine +
                                            $"[Owner]           {e.Server.Owner}" + Environment.NewLine +
                                            $"[User Count]      {e.Server.UserCount}" + Environment.NewLine +
                                            $"[Channel Count]   {e.Server.ChannelCount}" + Environment.NewLine +
                                            $"[Default Channel] #{e.Server.DefaultChannel}" + Environment.NewLine +
                                            $"[Role Count]      {roles.Count}" + Environment.NewLine +
                                            $"[Roles]           {string.Join(", ", roles)}" + Environment.NewLine +
                                            $"[Creation date]   {e.Server.Owner.JoinedAt} ({(DateTime.Now - e.Server.Owner.JoinedAt).Days} days ago)" +
                                             "```";

                            await e.Channel.SendMessage(message);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage(ex.Message);
                        }
                    });
        }
    }
}
