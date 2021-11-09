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
    internal static class RoleHandle
    {
        public static bool IsOwner(DiscordGuild guild, DiscordUser user) =>
            user == guild.Owner;
        public static bool IsAdmin(DiscordGuild guild, DiscordUser user)
        {
            var admins = new List<ulong>();
            var path = PathHandle.GetAdminPath(guild);

            var s = string.Empty;
            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
                while ((s = sr.ReadLine()) != null)
                    admins.Add(ulong.Parse(s));

            return admins.Contains(user.Id);
        }
    }
}
