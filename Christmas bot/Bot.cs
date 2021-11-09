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
    public partial class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                Token = "0OTA2ODczNTIzMDg2MTcyMjIx.YYe9yA.MToSIHY7UXTszJ1Md7oBcbxJFVU",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            using (var fs = new FileStream($"{PathHandle.GetRootPath()}{Path.DirectorySeparatorChar}config.json", FileMode.OpenOrCreate))
            {
                var json = await JsonDocument.ParseAsync(fs, new JsonDocumentOptions { AllowTrailingCommas = true }).ConfigureAwait(false);
                var root = json.RootElement;
                string t;
                config.Token = (t = root.GetProperty("token").GetString());
                Console.WriteLine(t);
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
    public partial class Bot
    {
        private async Task<string> GetJson(string path)
        {
            var json = string.Empty;
            using (var fs = File.OpenRead(path))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            return json;
        }
    }
}
