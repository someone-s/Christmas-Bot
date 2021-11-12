using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

using Christmas_bot.Commands;

namespace Christmas_bot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            if (AuthHandle.TryGetToken(out var token))
                config.Token = token;
            else
            {
                Console.WriteLine("error: Invalid Token File");
                await Task.Delay(10000);
                return;
            }

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                PrefixResolver = PrefixHandle.HandlePrefix,
                EnableDms = false,
                EnableMentionPrefix = true
            };


            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<InfoCommands>();
            Commands.RegisterCommands<GiftCommands>();
            Commands.RegisterCommands<PrefixCommands>();
            Commands.RegisterCommands<AdminCommands>();

            await PrefixHandle.LoadPrefix().ConfigureAwait(false);
            await GiftHandle.LoadGifts().ConfigureAwait(false);

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
