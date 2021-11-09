using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Christmas_bot
{
    internal static class GiftHandle
    {
        public static char seperator = '^';

        public static void WriteGift(StreamWriter sw, ulong id, string gift) =>
            sw.WriteLine($"{id}{seperator}{gift}");

        public static void ReadGift(string s, out ulong id, out string gift)
        {
            var p = s.Split(seperator);
            id = ulong.Parse(p[0]);
            gift = p[1];
        }

        public static List<Tuple<ulong, string>> ReadGifts(DiscordGuild guild)
        {
            var gifts = new List<Tuple<ulong, string>>();

            var s = string.Empty;
            var path = PathHandle.GetGiftPath(guild);

            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
                while ((s = sr.ReadLine()) != null)
                {
                    ReadGift(s, out ulong id, out string gift);
                    gifts.Add(new Tuple<ulong, string>(id, gift));
                }

            return gifts;
        }

        public static void WriteGifts(DiscordGuild guild, List<Tuple<ulong, string>> gifts)
        {
            var path = PathHandle.GetGiftPath(guild);

            if (gifts.Count > 0)
                using (var sw = File.CreateText(path))
                    for (int i = 0; i < gifts.Count; i++)
                        WriteGift(sw, gifts[i].Item1, gifts[i].Item2);
        }
    }
}
