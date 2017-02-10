using System;
using Discord;
using Discord.Commands;
using DiscordBot.Modules.Social.Controller;

namespace DiscordBot.Modules.Social
{
    class SocialHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        public SocialHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            Setup();
        }

        private void Setup()
        {
            service.CreateCommand("hug")
                .Description("Give someone a big hug.")
                .Parameter("Person", ParameterType.Required)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} hugs {e.GetArg("Person")}");
                });

            service.CreateCommand("kill")
                .Description("Kill yourself or someone else in one of many random ways.")
                .Parameter("Person", ParameterType.Optional)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(Social_Controller.Kill(e.User, e.GetArg("Person")));
                });

            service.CreateCommand("dice")
                .Alias(new string[] { "rnd", "random", "roll" })
                .Description($"Generate a random number between 2 values.```!roll -> 1 - 6{Environment.NewLine}!roll <n> -> 1 - <n>{Environment.NewLine}!roll <n1> <n2> -> <n1> - <n2>```")
                .Parameter("Number", ParameterType.Optional)
                .Parameter("Number2", ParameterType.Optional)
                .AddCheck((command, user, channel) => !DiscordBot.paused)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(Social_Controller.Dice(e.User, e.GetArg("Number"), e.GetArg("Number2")));
                });
        }
    }
}
