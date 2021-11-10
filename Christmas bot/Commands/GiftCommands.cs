using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Christmas_bot.Commands
{
    public partial class GiftCommands : BaseCommandModule
    {
        [Command("gift-admin-add")]
        [Aliases("gaa", "add-a")]
        [Description("add gift admin")]
        public async Task AssignAdmin(CommandContext ctx, 
            [Description("user to add")] DiscordUser newadmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsOwner(ctx.Guild, ctx.User))
            {
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not owner");
            }
            else
            {
                var path = PathHandle.GetAdminPath(ctx.Guild);

                using (var sw = new StreamWriter(path, true))
                    sw.WriteLine(newadmin.Id);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{newadmin.Username} added as admin");
            }

            await message.DeleteAsync();
        }

        [Command("gift-admin-remove")]
        [Aliases("gar", "rm-a")]
        [Description("remove gift admin")]
        public async Task RelegateAdmin(CommandContext ctx, 
            [Description("user to remove")] DiscordUser formeradmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (ctx.User != ctx.Guild.Owner)
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not owner");
            else
            {
                var admins = new List<ulong>();
                var path = PathHandle.GetAdminPath(ctx.Guild);

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

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{formeradmin.Username} relegated");
            }

            await message.DeleteAsync();
        }

        [Command("gift-admin-list")]
        [Aliases("gal", "list-a")]
        [Description("list bot admins")]
        public async Task ListAdmin(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var path = PathHandle.GetAdminPath(ctx.Guild);
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
    }

    public partial class GiftCommands
    {

        [Command("gift-add")]
        [Aliases("ga", "add-g")]
        [Description("add gift to pool")]
        public async Task AddGift(CommandContext ctx,
            [Description("either text, url, imageurl; use quotation to include spaces")] string type,
            [Description("gift in text form")] string gift)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var filtered = TextHandle.CleanText(gift);

            if (filtered.Length == 0)
                await MessageHandle.SendError(ctx.Channel,
                    message: $"gift by {ctx.User.Username} is empty");
            else
            {
                var path = PathHandle.GetGiftPath(ctx.Guild);

                using (var sw = new StreamWriter(path, true))
                {
                    switch (type)
                    {
                        case "text":
                            await MessageHandle.SendSuccess(ctx.Channel, 
                                message: $"{filtered} added by {ctx.User.Username}");
                            GiftHandle.WriteGift(sw, ctx.User.Id, $"text:{filtered}");
                            break;
                        case "url":
                            await MessageHandle.SendSuccess(ctx.Channel,
                                message: $"{filtered} added by {ctx.User.Username}",
                                url: filtered);
                            GiftHandle.WriteGift(sw, ctx.User.Id, $"url:{filtered}");
                            break;
                        case "imageurl":
                            await MessageHandle.SendSuccess(ctx.Channel,
                                message: $"{filtered} added by {ctx.User.Username}",
                                url: filtered,
                                imageurl: filtered);
                            GiftHandle.WriteGift(sw, ctx.User.Id, $"image:{filtered}");
                            break;
                        default:
                            await MessageHandle.SendError(ctx.Channel,
                                message: $"unsupported gift type {type}");
                            break;
                    }
                }
            }

            await message.DeleteAsync();
        }

        [Command("gift-list")]
        [Aliases("gl", "ls-g")]
        [Description("list gifts in pool")]
        public async Task ListGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var path = PathHandle.GetGiftPath(ctx.Guild);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };

            using (var fs = File.Open(path, FileMode.OpenOrCreate))
            using (var sr = new StreamReader(fs))
            {
                var s = string.Empty;
                var i = 1;
                while ((s = sr.ReadLine()) != null)
                {
                    GiftHandle.ReadGift(s, out ulong id, out string gift);
                    var sender = await ctx.Guild.GetMemberAsync(id).ConfigureAwait(false);
                    embed.AddField($"{i}: {gift}", sender.Username);

                    i++;
                }
            }

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);

            await message.DeleteAsync();
        }

        [Command("gift-remove")]
        [Aliases("gr", "rm-g")]
        [Description("remove gift pool")]
        public async Task RemoveGift(CommandContext ctx,
            [Description("index of gift to be removed")] int index)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsAdmin(ctx.Guild, ctx.User))
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not admin");
            else
            {
                var gifts = GiftHandle.ReadGifts(ctx.Guild);

                if (gifts.Count < index && index - 1 < 0)
                    await MessageHandle.SendError(ctx.Channel,
                        message: $"index out of range (1-{gifts.Count})");
                else
                {
                    gifts.RemoveAt(index - 1);

                    File.Delete(PathHandle.GetGiftPath(ctx.Guild));
                    GiftHandle.WriteGifts(ctx.Guild, gifts);

                    await MessageHandle.SendSuccess(ctx.Channel,
                        message: "gift removed from pool");
                }
            }

            await message.DeleteAsync();
        }

        [Command("gift-distribute")]
        [Aliases("gd", "dist-g")]
        [Description("distribute gifts to all user and remove them from gift pool")]
        public async Task DistributeGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsAdmin(ctx.Guild, ctx.User))
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not admin");
            else
            {
                var random = new Random();
                var gifts = GiftHandle.ReadGifts(ctx.Guild);
                var used = new HashSet<int>();
                var flip = false;

                if (gifts.Count == 0)
                    await MessageHandle.SendError(ctx.Channel,
                        message: "no presents avaliable");
                else
                {
                    foreach (var user in await ctx.Guild.GetAllMembersAsync().ConfigureAwait(false))
                    {
                        if (used.Count == gifts.Count)
                        {
                            used.Clear();
                            flip = true;
                        }

                        int i = random.Next(0, gifts.Count);
                        while (used.Contains(i))
                            i = random.Next(0, gifts.Count);

                        var sender = await ctx.Guild.GetMemberAsync(gifts[i].Item1).ConfigureAwait(false);
                        var gift = gifts[i].Item2;

                        if (gift.StartsWith("text:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got `{gift.Remove(0, 5)}` from {sender.Mention}");
                        else if (gift.StartsWith("url:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift.Remove(0, 4));
                        else if (gift.StartsWith("image:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift.Remove(0, 6),
                                imageurl: gift.Remove(0, 6));

                        used.Add(i);
                    }

                    var path = PathHandle.GetGiftPath(ctx.Guild);

                    File.Delete(path);
                    if (!flip)
                        GiftHandle.WriteGifts(ctx.Guild, gifts.Where((t, i) => !used.Contains(i)).ToList());
                }
            }

            await message.DeleteAsync();
        }

        [Command("gift-clear")]
        [Description("remove all gifts from gift pool")]
        public async Task ClearGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsAdmin(ctx.Guild, ctx.User))
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not admin");
            else
            {
                var path = PathHandle.GetGiftPath(ctx.Guild);
                File.Delete(path);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{ctx.User.Username} cleared gifts pool");
            }

            await message.DeleteAsync();
        }

    }
}
