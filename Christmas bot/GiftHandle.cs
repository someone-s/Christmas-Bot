using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Christmas_bot
{
    internal static class GiftHandle
    {
        public static Dictionary<DiscordGuild, List<(DiscordUser, string)>> cache = new Dictionary<DiscordGuild, List<(DiscordUser, string)>>();

        public static async Task LoadGifts()
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

                var path = PathHandle.GetGiftPath(guild);
                var json = File.ReadAllText(path);
                var entries = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
                if (entries is null) entries = new List<(ulong, string)>();

                if (!cache.ContainsKey(guild))
                    cache[guild] = new List<(DiscordUser, string)>();
                foreach (var entry in entries)
                {
                    DiscordUser user;
                    try
                    {
                        user = await guild.GetMemberAsync(entry.Item1);
                    }
                    catch (DSharpPlus.Exceptions.NotFoundException)
                    {
                        continue;
                    }
                    var gift = entry.Item2;
                    cache[guild].Add((user, gift));
                }
            }
        }

        public static void AddGift(DiscordGuild guild, DiscordUser user, string gift)
        {
            if (!cache.ContainsKey(guild))
                cache[guild] = new List<(DiscordUser, string)>();
            cache[guild].Add((user, gift));

            Task.Run(delegate
            {
                var path = PathHandle.GetGiftPath(guild);
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
                if (list is null)
                    list = new List<(ulong, string)>();

                list.Add((user.Id, gift));

                json = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(path, json);
            });
        }

        public static bool RemoveGift(DiscordGuild guild, int index)
        {
            var entries = ReadGifts(guild);

            if (entries.Count <= index || index < 0) return false;

            entries.RemoveAt(index);

            File.Delete(PathHandle.GetGiftPath(guild));
            cache.Remove(guild);
            AddGifts(guild, entries);

            return true;
        }

        public static List<(DiscordUser, string)> ReadGifts(DiscordGuild guild)
        {
            if (cache.ContainsKey(guild))
                return cache[guild];
            else
                return new List<(DiscordUser, string)>();
        }

        public static void AddGifts(DiscordGuild guild, List<(DiscordUser, string)> entries)
        {
            if (!cache.ContainsKey(guild))
                cache[guild] = new List<(DiscordUser, string)>();
            foreach (var entry in entries)
            {
                var user = entry.Item1;
                var gift = entry.Item2;
                cache[guild].Add((user, gift));
            }

            Task.Run(delegate
            {
                var path = PathHandle.GetGiftPath(guild);
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
                if (list is null) list = new List<(ulong, string)>();

                if (entries.Count <= 0)
                    return;
                foreach (var entry in entries)
                {
                    var user = entry.Item1;
                    var gift = entry.Item2;
                    list.Add((user.Id, gift));
                }

                json = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(path, json);
            });
        }

        public static void ClearGifts(DiscordGuild guild)
        {
            var path = PathHandle.GetGiftPath(guild);

            File.Delete(path);
            if (cache.ContainsKey(guild))
                cache.Remove(guild);
        }
    }
}
