using System;
using System.Threading;
using Discord;
using Discord.Commands;

namespace DiscordBot.Modules.Administration
{
    class BotHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        public BotHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            Setup();
        }

        private void Setup()
        {
            service.CreateCommand("ping")
                .Description("Will respond with pong if the bot is up.")
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    try
                    {
                        await e.Channel.SendMessage("Pong");
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage(ex.ToString());
                    }
                });
            
            service.CreateCommand("pause")
                    .Description("The bot will stop responding to commands untill unpaused.")
                    .AddCheck((command, user, channel) => !DiscordBot.paused && user.Id == 140470317440040960)
                    .Hide()
                    .Do(async e =>
                    {
                        DiscordBot.paused = true;
                        await e.Channel.SendMessage("The bot has been paused and will stop responding to commands!");
                    });

            service.CreateCommand("unpause")
                .Description("The bot will respond to commands again.")
                .AddCheck((command, user, channel) => DiscordBot.paused && user.Id == 140470317440040960)
                .Hide()
                .Do(async e =>
                {
                    DiscordBot.paused = false;
                    await e.Channel.SendMessage("The bot has been unpaused and will respond to commands again.");
                });

            service.CreateCommand("stop")
                .Description("Stop the program")
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Hide()
                .Do(e =>
                {
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                });
        }
    }
}
