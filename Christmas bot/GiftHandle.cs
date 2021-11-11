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
        public static async Task WriteGift(DiscordGuild guild, DiscordUser user, string gift)
        {
            try
            {
                await guild.GetMemberAsync(user.Id).ConfigureAwait(false);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                return;
            }
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
                DiscordUser user;
                try
                {
                    user = await guild.GetMemberAsync(tuple.Item1).ConfigureAwait(false);
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    continue;
                }
                gifts.Add((user, tuple.Item2));
            }
            return gifts;
        }

        public static async Task WriteGifts(DiscordGuild guild, List<(DiscordUser, string)> entries)
        {
            var path = PathHandle.GetGiftPath(guild);
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<(ulong, string)>>(json);
            if (list is null)
                list = new List<(ulong, string)>();

            if (entries.Count <= 0)
                return;
            foreach (var entry in entries)
            {
                var user = entry.Item1;
                var gift = entry.Item2;
                try
                {
                    await guild.GetMemberAsync(user.Id).ConfigureAwait(false);
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    continue;
                }
                list.Add((user.Id, gift));
            }

            json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
