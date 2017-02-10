using Discord;
using Discord.Commands;
using DiscordBot.Modules.Administration;
using DiscordBot.Modules.Events;
using DiscordBot.Modules.Games.RPG;
using DiscordBot.Modules.Info;
using DiscordBot.Modules.Social;

namespace DiscordBot.Modules
{
    class DiscordHandler
    {
        private readonly DiscordClient client;
        public DiscordHandler(DiscordClient client)
        {
            this.client = client;

            client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Private;
            });

            SetupCommandHandlers();
            SetupEventHandlers();
        }

        private void SetupCommandHandlers()
        {
            new BotHandler(client);
            new InfoHandler(client);
            new ServerAdministrationHandler(client);
            new SqlHandler(client);
            new SocialHandler(client);
            new RPGHandler(client);
        }

        private void SetupEventHandlers()
        {
            new UserEventHandler(client);
        }
    }
}
