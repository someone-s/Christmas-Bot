using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Diagnostics;

using Christmas_bot.Commands;

namespace Christmas_bot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            using (var fs = new FileStream($"{PathHandle.GetRootPath()}{Path.DirectorySeparatorChar}config.json", FileMode.OpenOrCreate))
            {
                var json = await JsonDocument.ParseAsync(fs, new JsonDocumentOptions { AllowTrailingCommas = true }).ConfigureAwait(false);
                var root = json.RootElement;
                config.Token = root.GetProperty("token").GetString();
            }

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "?" },
                EnableDms = false,
                EnableMentionPrefix = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<InfoCommands>();
            Commands.RegisterCommands<GiftCommands>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
