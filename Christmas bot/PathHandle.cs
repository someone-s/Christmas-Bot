using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace Christmas_bot
{
    internal static class PathHandle
    {
        public static string GetAdminPath(DiscordGuild guild)
        {
            var root = GetServerPath(guild);
            var admins = $"{root}{Path.DirectorySeparatorChar}admins.txt";
            if (!File.Exists(admins))
                using (File.Create(admins))
                    Debug.WriteLine($"{admins} created");
            return admins;
        }
        public static string GetGiftPath(DiscordGuild guild)
        {
            var root = GetServerPath(guild);
            var gifts = $"{root}{Path.DirectorySeparatorChar}gifts.txt";
            if (!File.Exists(gifts))
                using (File.Create(gifts))
                    Debug.WriteLine($"{gifts} created");
            return gifts;
        }
        public static string GetRootPath()
        {
            var root = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}Christmas-Bot";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            return root;
        }
        public static string GetServerPath(DiscordGuild guild)
        {
            var path = $"{GetRootPath()}{Path.DirectorySeparatorChar}servers{Path.DirectorySeparatorChar}{guild.Id}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
