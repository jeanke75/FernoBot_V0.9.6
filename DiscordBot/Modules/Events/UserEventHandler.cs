using System.Linq;
using Discord;

namespace DiscordBot.Modules.Events
{
    class UserEventHandler
    {
        private readonly DiscordClient client;
        public UserEventHandler(DiscordClient client)
        {
            this.client = client;
            Setup();
        }

        private void Setup()
        {
            client.UserJoined += async (s, e) =>
            {
                // Create a Channel object by searching for a channel named '#log' on the server the ban occurred in.
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();
                // Send a message to the server's log channel, stating that a user was banned.
                await logChannel.SendMessage($"User Joined: {e.User.Name}");
            };

            client.UserLeft += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Left: {e.User.Name}");
            };

            client.UserBanned += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Banned: {e.User.Name}");
            };

            client.UserUnbanned += async (s, e) =>
            {
                var logChannel = e.Server.FindChannels("log").FirstOrDefault();

                await logChannel.SendMessage($"User Unbanned: {e.User.Name}");
            };
        }
    }
}
