using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace Christmas_bot
{
    internal static class PathHandle
    {
        public static string GetPrefixPath(DiscordGuild guild) =>
            GetPrefixPath(guild.Id);
        public static string GetPrefixPath(ulong id)
        {
            var root = GetServerPath(id);
            var prefixes = $"{root}{Path.DirectorySeparatorChar}prefix.txt";
            if (!File.Exists(prefixes))
            {
                using (File.Create(prefixes))
                    Console.WriteLine($"{prefixes} created");
                var json = JsonConvert.SerializeObject(new List<string> { "?" });
                File.WriteAllText(prefixes, json);
            }
            return prefixes;
        }

        public static string GetAdminPath(DiscordGuild guild) =>
            GetAdminPath(guild.Id);
        public static string GetAdminPath(ulong id)
        {
            var root = GetServerPath(id);
            var admins = $"{root}{Path.DirectorySeparatorChar}admins.txt";
            if (!File.Exists(admins))
                using (File.Create(admins))
                    Console.WriteLine($"{admins} created");
            return admins;
        }

        public static string GetGiftPath(DiscordGuild guild) =>
            GetGiftPath(guild.Id);
        public static string GetGiftPath(ulong id)
        {
            var root = GetServerPath(id);
            var gifts = $"{root}{Path.DirectorySeparatorChar}gifts.txt";
            if (!File.Exists(gifts))
                using (File.Create(gifts))
                    Console.WriteLine($"{gifts} created");
            return gifts;
        }

        public static string GetRootPath()
        {
            var root = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}Christmas-Bot";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            return root;
        }

        public static string GetServersPath()
        {
            var path = $"{GetRootPath()}{Path.DirectorySeparatorChar}servers";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string GetServerPath(DiscordGuild guild)
        {
            var path = $"{GetServersPath()}{Path.DirectorySeparatorChar}{guild.Id}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string GetServerPath(ulong id)
        {
            var path = $"{GetServersPath()}{Path.DirectorySeparatorChar}{id}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string GetConfigPath()
        {
            var config = $"{GetRootPath()}{Path.DirectorySeparatorChar}config.json";
            if (!File.Exists(config))
                File.CreateText(config);
            return config;
        }
    }
}
