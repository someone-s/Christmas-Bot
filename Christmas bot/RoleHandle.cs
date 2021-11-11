using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Christmas_bot
{
    internal static class RoleHandle
    {
        public static bool IsOwner(DiscordGuild guild, DiscordUser user) =>
            user == guild.Owner;
        public static bool IsAdmin(DiscordGuild guild, DiscordUser user)
        {
            var path = PathHandle.GetAdminPath(guild);
            var json = File.ReadAllText(path);

            var hash = JsonConvert.DeserializeObject<List<ulong>>(json);
            if (hash == null)
                hash = new List<ulong>();

            return hash.Contains(user.Id);
        }

        public static void AddAdmin(DiscordGuild guild, DiscordUser user)
        {
            var path = PathHandle.GetAdminPath(guild);
            var json = File.ReadAllText(path);

            var hash = JsonConvert.DeserializeObject<List<ulong>>(json);
            if (hash == null)
                hash = new List<ulong>();
            hash.Add(user.Id);
            json = JsonConvert.SerializeObject(hash);
            File.WriteAllText(path, json);
        }
        public static async Task<List<DiscordUser>> GetAdmins(DiscordGuild guild)
        {
            var path = PathHandle.GetAdminPath(guild);
            var json = File.ReadAllText(path);

            var hash = JsonConvert.DeserializeObject<List<ulong>>(json);
            if (hash == null)
                hash = new List<ulong>();
            var output = new List<DiscordUser>();
            foreach (var id in hash) 
            {
                var user = await guild.GetMemberAsync(id);
                output.Add(user);
            }
            return output;
        }
        public static void RemoveAdmin(DiscordGuild guild, DiscordUser user)
        {
            var path = PathHandle.GetAdminPath(guild);
            var json = File.ReadAllText(path);

            var hash = JsonConvert.DeserializeObject<List<ulong>>(json);
            if (hash == null)
                hash = new List<ulong>();
            hash.Remove(user.Id);
            json = JsonConvert.SerializeObject(hash);
            File.WriteAllText(path, json);
        }
    }
}
