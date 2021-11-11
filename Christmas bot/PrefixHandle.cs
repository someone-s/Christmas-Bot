using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Christmas_bot
{
    internal class PrefixHandle
    {
        public static void AddPrefix(DiscordGuild guild, string prefix)
        {
            var path = PathHandle.GetPrefixPath(guild);
            var json = File.ReadAllText(path);

            var list = JsonConvert.DeserializeObject<List<string>>(json);
            if (list == null)
                list = new List<string>();
            if (!list.Contains(prefix))
                list.Add(prefix);
            json = JsonConvert.SerializeObject(list);
            File.WriteAllText(path, json);
        }
        public static List<string> GetPrefixes(DiscordGuild guild)
        {
            var path = PathHandle.GetPrefixPath(guild);
            var json = File.ReadAllText(path);

            var list = JsonConvert.DeserializeObject<List<string>>(json);
            if (list == null)
                list = new List<string>();

            return list;
        }
        public static void RemovePrefix(DiscordGuild guild, string prefix)
        {
            var path = PathHandle.GetPrefixPath(guild);
            var json = File.ReadAllText(path);

            var list = JsonConvert.DeserializeObject<List<string>>(json);
            if (list == null)
                list = new List<string>();
            if (list.Contains(prefix) && prefix != "?")
                list.Remove(prefix);
            json = JsonConvert.SerializeObject(list);
            File.WriteAllText(path, json);
        }

        public static async Task<int> HandlePrefix(DiscordMessage msg)
        {
            await Task.Run(() => 1 + 1);

            var prefixes = GetPrefixes(msg.Channel.Guild);

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
    }
}
