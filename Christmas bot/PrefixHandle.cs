using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace Christmas_bot
{
    internal class PrefixHandle
    {
        private static Dictionary<DiscordGuild, List<string>> cache = new Dictionary<DiscordGuild, List<string>>();

        public static async Task LoadPrefix()
        {
            cache.Clear();

            foreach (var server in Directory.GetDirectories(PathHandle.GetServersPath()))
            {
                var name = Path.GetFileName(server);
                if (!ulong.TryParse(name, out var id)) continue;

                DiscordGuild guild;
                try
                {
                    guild = await Bot.Client.GetGuildAsync(id).ConfigureAwait(false);
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    continue;
                }
                catch (DSharpPlus.Exceptions.UnauthorizedException)
                {
                    continue;
                }

                var path = PathHandle.GetPrefixPath(guild);
                var json = File.ReadAllText(path);

                var list = JsonConvert.DeserializeObject<List<string>>(json);
                if (list is not null)
                    cache[guild] = list;
            }
        }

        public static void AddPrefix(DiscordGuild guild, string prefix)
        {
            if (!cache.ContainsKey(guild))
                cache[guild] = NewEntry();
            cache[guild].Add(prefix);

            Task.Run(delegate
            {
                var path = PathHandle.GetPrefixPath(guild);
                var json = File.ReadAllText(path);

                var list = JsonConvert.DeserializeObject<List<string>>(json);

                if (list == null) list = NewEntry();

                if (!list.Contains(prefix)) list.Add(prefix);

                json = JsonConvert.SerializeObject(list);
                File.WriteAllText(path, json);
            });
        }

        public static List<string> GetPrefixes(DiscordGuild guild)
        {
            if (cache.ContainsKey(guild))
                return cache[guild];
            else
                return NewEntry();
        }

        public static void RemovePrefix(DiscordGuild guild, string prefix)
        {
            if (!cache.ContainsKey(guild))
                cache[guild] = NewEntry();
            else if (cache[guild].Contains(prefix) && prefix != "?")
                cache[guild].Remove(prefix);

            Task.Run(delegate
            {
                var path = PathHandle.GetPrefixPath(guild);
                var json = File.ReadAllText(path);

                var list = JsonConvert.DeserializeObject<List<string>>(json);

                if (list == null) list = NewEntry();

                if (list.Contains(prefix) && prefix != "?") list.Remove(prefix);

                json = JsonConvert.SerializeObject(list);
                File.WriteAllText(path, json);
            });
        }

        public static async Task<int> HandlePrefix(DiscordMessage msg)
        {
            await Task.Run(() => 1 + 1);

            List<string> prefixes = new List<string> { "?" };
            if (cache.ContainsKey(msg.Channel.Guild))
                prefixes = cache[msg.Channel.Guild];

            int i = 0;
            bool flag = false;
            for (i = 0; i < prefixes.Count; i++)
                if (msg.Content.StartsWith(prefixes[i]))
                {
                    flag = true;
                    break;
                }

            if (flag)
                return prefixes[i].Length;
            else
                return -1;
        }

        public static List<string> NewEntry() => 
            new List<string> { "?"};
    }
}
