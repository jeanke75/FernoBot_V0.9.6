using System;
using System.Configuration;
using Discord;
using DiscordBot.Modules;

namespace DiscordBot
{
    class DiscordBot
    {
        private readonly string _token = ConfigurationManager.AppSettings["TOKEN"];
        private readonly string _appName = ConfigurationManager.AppSettings["APP_NAME"];
        public static bool paused = false;

        private DiscordClient _client;

        public DiscordBot()
        {
            Login();

            new DiscordHandler(_client);
        }

        private void Login()
        {
            _client = new DiscordClient(x =>
            {
                x.AppName = _appName;
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = Log;
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(string.Format("[{0}] [{1}] {2}", e.Severity, e.Source, e.Message));
        }

        public void Run()
        {
            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(_token, TokenType.Bot); // bot-token
                _client.SetGame("!help");
            });
        }
    }
}
