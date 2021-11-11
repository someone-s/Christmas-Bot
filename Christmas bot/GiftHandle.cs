using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace Christmas_bot
{
    internal static class GiftHandle
    {
        public static void WriteGift(DiscordGuild guild, DiscordUser user, string gift)
        {
            var path = PathHandle.GetGiftPath(guild);
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
            if (list is null)
                list = new List<(ulong, string)>();

            list.Add((user.Id, gift));

            json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static async Task<List<(DiscordUser, string)>> ReadGifts(DiscordGuild guild)
        {
            var s = string.Empty;
            var path = PathHandle.GetGiftPath(guild);
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
            if (list is null)
                list = new List<(ulong, string)>();

            var gifts = new List<(DiscordUser, string)>();
            foreach (var tuple in list)
            {
                var user = await guild.GetMemberAsync(tuple.Item1).ConfigureAwait(false);
                gifts.Add((user, tuple.Item2));
            }
            return gifts;
        }

        public static void WriteGifts(DiscordGuild guild, List<(DiscordUser, string)> entries)
        {
            var path = PathHandle.GetGiftPath(guild);
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
            if (list is null)
                list = new List<(ulong, string)>();

            if (entries.Count <= 0)
                return;
            entries.ForEach(t => list.Add((t.Item1.Id, t.Item2)));

            json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
