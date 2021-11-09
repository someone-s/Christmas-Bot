using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Christmas_bot.Commands
{
    public class GiftCommands : BaseCommandModule
    {
        private char seperator = '^';
        private string GetAdminPath(ulong server)
        {
            var root = GetRootPath(server);
            var admins = $"{root}{Path.DirectorySeparatorChar}admins.txt";
            if (!File.Exists(admins))
                using (File.Create(admins))
                    Debug.WriteLine($"{admins} created");
            return admins;
        }
        private string GetGiftPath(ulong server)
        {
            var root = GetRootPath(server);
            var gifts = $"{root}{Path.DirectorySeparatorChar}gifts.txt";
            if (!File.Exists(gifts))
                using (File.Create(gifts))
                    Debug.WriteLine($"{gifts} created");
            return gifts;
        }
        private string GetRootPath(ulong server)
        {
            var root = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}servers{Path.DirectorySeparatorChar}{server}";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            return root;
        }

        [Command("gift-admin-add")]
        [Description("add gift admin")]
        public async Task AssignAdmin(CommandContext ctx, DiscordUser newadmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (ctx.User != ctx.Guild.Owner)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"error",
                    Color = DiscordColor.Red,
                    Description = $"{ctx.User.Username} is not owner"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            else
            {
                var path = GetAdminPath(ctx.Guild.Id);

                using (var sw = new StreamWriter(path, true))
                    sw.WriteLine(newadmin.Id);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"success",
                    Color = DiscordColor.Green,
                    Description = $"{newadmin.Username} added as admin"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("gift-admin-remove")]
        [Description("remove gift admin")]
        public async Task RelegateAdmin(CommandContext ctx, DiscordUser formeradmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (ctx.User != ctx.Guild.Owner)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"error",
                    Color = DiscordColor.Red,
                    Description = $"{ctx.User.Username} is not owner"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            else
            {
                var admins = new List<ulong>();
                var path = GetAdminPath(ctx.Guild.Id);

                var s = string.Empty;
                using (var fs = File.Open(path, FileMode.OpenOrCreate))
                using (var sr = new StreamReader(fs))
                    while ((s = sr.ReadLine()) != null)
                        admins.Add(ulong.Parse(s));

                admins.Remove(formeradmin.Id);

                File.Delete(path);
                using (var sw = File.CreateText(path))
                    foreach (var admin in admins)
                        sw.WriteLine(admin);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"success",
                    Color = DiscordColor.Green,
                    Description = $"{formeradmin.Username} relegated"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("gift-admin-list")]
        [Description("list bot admins")]
        public async Task ListAdmin(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var path = GetAdminPath(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };

            var s = string.Empty;
            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
                while ((s = sr.ReadLine()) != null)
                {
                    var admin = await ctx.Guild.GetMemberAsync(ulong.Parse(s));
                    embed.AddField("admin", admin.Username);
                }

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);

            await message.DeleteAsync();
        }

        [Command("gift-add")]
        [Description("add gift to pool [type: text, url, imageurl][string]")]
        public async Task AddGift(CommandContext ctx, string type, string gift)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var filtered = new string(gift.Where(c => c != '^').ToArray());

            if (filtered.Length == 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"error",
                    Color = DiscordColor.Red,
                    Description = $"gift by {ctx.User.Username} is empty",
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            else
            {
                var path = GetGiftPath(ctx.Guild.Id);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"success",
                    Color = DiscordColor.Green,
                };
                using (var sw = new StreamWriter(path, true))
                {
                    switch (type)
                    {
                        case "text":
                            embed.Description = $"{filtered} added by {ctx.User.Username}";
                            sw.WriteLine($"{ctx.User.Id}{seperator}text:{filtered}");
                            break;
                        case "url":
                            embed.Description = $"url:{filtered} added by {ctx.User.Username}";
                            sw.WriteLine($"{ctx.User.Id}{seperator}url:{filtered}");
                            break;
                        case "imageurl":
                            embed.Description = $"image:{filtered} added by {ctx.User.Username}";
                            sw.WriteLine($"{ctx.User.Id}{seperator}image:{filtered}");
                            break;
                        default:
                            break;
                    }
                }
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("gift-list")]
        [Description("list gifts in pool")]
        public async Task ListGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var path = GetGiftPath(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };

            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
            {
                var s = string.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    var p = s.Split(seperator);
                    var sender = await ctx.Guild.GetMemberAsync(ulong.Parse(p[0])).ConfigureAwait(false);
                    embed.AddField(p[1], sender.Username);
                }
            }

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);

            await message.DeleteAsync();
        }

        [Command("gift-distribute")]
        [Description("distribute gifts to all user and remove them from gift pool")]
        public async Task DistributeGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var admins = new List<ulong>();
            var path = GetAdminPath(ctx.Guild.Id);

            var s = string.Empty;
            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
                while ((s = sr.ReadLine()) != null)
                    admins.Add(ulong.Parse(s));

            if (!admins.Contains(ctx.User.Id))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"error",
                    Color = DiscordColor.Red,
                    Description = $"{ctx.User.Username} is not admin"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            else
            {
                var random = new Random();
                var gifts = new List<Tuple<ulong, string>>();
                var used = new HashSet<int>();
                var flip = false;

                path = GetGiftPath(ctx.Guild.Id);

                using (var fs = File.Open(path, FileMode.OpenOrCreate))
                using (var sr = new StreamReader(fs))
                    while ((s = sr.ReadLine()) != null)
                    {
                        var p = s.Split(seperator);
                        gifts.Add(new Tuple<ulong, string>(ulong.Parse(p[0]), p[1]));
                    }

                if (gifts.Count == 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"error",
                        Color = DiscordColor.Red,
                        Description = "no presents avaliable"
                    };
                    await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
                }
                else
                {
                    var guild = ctx.Guild;

                    foreach (var user in await guild.GetAllMembersAsync().ConfigureAwait(false))
                    {
                        if (used.Count == gifts.Count)
                        {
                            used.Clear();
                            flip = true;
                        }

                        int i = random.Next(0, gifts.Count);
                        while (used.Contains(i))
                            i = random.Next(0, gifts.Count);

                        var sender = await guild.GetMemberAsync(gifts[i].Item1).ConfigureAwait(false);
                        var gift = gifts[i].Item2;

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = $"presents",
                            Color = DiscordColor.Green
                        };
                        if (gift.StartsWith("text:"))
                        {
                            embed.Description = $"{user.Mention} got `{gift.Remove(0, 5)}` from {sender.Mention}";
                        }
                        else if (gift.StartsWith("url:"))
                        {
                            embed.Url = gift.Remove(0, 4);
                            embed.Description = $"{user.Mention} got this from {sender.Mention}";
                        }
                        else if (gift.StartsWith("image:"))
                        {
                            string actual = gift.Remove(0, 6);
                            if (actual.EndsWith(".png") ||
                                actual.EndsWith(".jpg"))
                                embed.ImageUrl = actual;
                            else
                                embed.Url = actual;
                            embed.Description = $"{user.Mention} got this from {sender.Mention}";
                        }
                        await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);

                        used.Add(i);
                    }

                    File.Delete(path);
                    if (!flip)
                        using (var sw = File.CreateText(path))
                            for (int i = 0; i < gifts.Count; i++)
                                if (!used.Contains(i))
                                    sw.WriteLine($"{gifts[i].Item1}{seperator}{gifts[i].Item2}");
                }
            }

            await message.DeleteAsync();
        }

        [Command("gift-clear")]
        [Description("remove all gifts from gift pool")]
        public async Task ClearGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var admins = new List<ulong>();
            var path = GetAdminPath(ctx.Guild.Id);

            var s = string.Empty;
            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
                while ((s = sr.ReadLine()) != null)
                    admins.Add(ulong.Parse(s));

            if (!admins.Contains(ctx.User.Id))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"error",
                    Color = DiscordColor.Red,
                    Description = $"{ctx.User.Username} is not admin"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            else
            {
                path = GetGiftPath(ctx.Guild.Id);
                File.Delete(path);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"success",
                    Color = DiscordColor.Green,
                    Description = $"{ctx.User.Username} cleared gifts pool"
                };
                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }
    }
}
