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
                    message: $"{ctx.User.Username} is not owner").ConfigureAwait(false);
            }
            else
            {
                RoleHandle.AddAdmin(ctx.Guild, newadmin);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{newadmin.Username} added as admin").ConfigureAwait(false);
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

            if (!RoleHandle.IsOwner(ctx.Guild, ctx.User))
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not owner");
            else
            {
                RoleHandle.RemoveAdmin(ctx.Guild, formeradmin);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{formeradmin.Username} relegated").ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("gift-admin-list")]
        [Aliases("gal", "list-a")]
        [Description("list bot admins")]
        public async Task ListAdmin(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };

            var hash = await RoleHandle.GetAdmins(ctx.Guild).ConfigureAwait(false);
            foreach (var user in hash) embed.AddField("admin", user.Username);

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
                    message: $"gift by {ctx.User.Username} is empty").ConfigureAwait(false);
            else
            {
                switch (type)
                {
                    case "text":
                        await MessageHandle.SendSuccess(ctx.Channel,
                            message: $"{filtered} added by {ctx.User.Username}").ConfigureAwait(false);
                        GiftHandle.WriteGift(ctx.Guild, ctx.User, $"text:{filtered}");
                        break;
                    case "url":
                        await MessageHandle.SendSuccess(ctx.Channel,
                            message: $"{filtered} added by {ctx.User.Username}",
                            url: filtered).ConfigureAwait(false);
                        GiftHandle.WriteGift(ctx.Guild, ctx.User, $"url:{filtered}");
                        break;
                    case "imageurl":
                        await MessageHandle.SendSuccess(ctx.Channel,
                            message: $"{filtered} added by {ctx.User.Username}",
                            url: filtered,
                            imageurl: filtered).ConfigureAwait(false);
                        GiftHandle.WriteGift(ctx.Guild, ctx.User, $"image:{filtered}");
                        break;
                    default:
                        await MessageHandle.SendError(ctx.Channel,
                            message: $"unsupported gift type {type}").ConfigureAwait(false);
                        break;
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

            var entries = await GiftHandle.ReadGifts(ctx.Guild).ConfigureAwait(false);
            entries.
                Select((t, i) => Tuple.Create(i, t)).ToList().
                ForEach(c => embed.AddField($"{c.Item1}: {c.Item2.Item2}", c.Item2.Item1.Username));

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
                    message: $"{ctx.User.Username} is not admin").ConfigureAwait(false);
            else
            {
                index--;

                var entries = await GiftHandle.ReadGifts(ctx.Guild).ConfigureAwait(false);

                if (entries.Count <= index && index < 0)
                    await MessageHandle.SendError(ctx.Channel,
                        message: $"index out of range (1-{entries.Count})").ConfigureAwait(false);
                else
                {
                    entries.RemoveAt(index);

                    File.Delete(PathHandle.GetGiftPath(ctx.Guild));
                    GiftHandle.WriteGifts(ctx.Guild, entries);

                    await MessageHandle.SendSuccess(ctx.Channel,
                        message: "gift removed from pool").ConfigureAwait(false);
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
                    message: $"{ctx.User.Username} is not admin").ConfigureAwait(false);
            else
            {
                var random = new Random();
                var gifts = await GiftHandle.ReadGifts(ctx.Guild).ConfigureAwait(false);
                var used = new HashSet<int>();
                var flip = false;

                if (gifts.Count == 0)
                    await MessageHandle.SendError(ctx.Channel,
                        message: "no presents avaliable").ConfigureAwait(false);
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

                        var sender = gifts[i].Item1;
                        var gift = gifts[i].Item2;

                        if (gift.StartsWith("text:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got `{gift.Remove(0, 5)}` from {sender.Mention}").ConfigureAwait(false);
                        else if (gift.StartsWith("url:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift.Remove(0, 4)).ConfigureAwait(false);
                        else if (gift.StartsWith("image:"))
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift.Remove(0, 6),
                                imageurl: gift.Remove(0, 6)).ConfigureAwait(false);

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
                    message: $"{ctx.User.Username} is not admin").ConfigureAwait(false);
            else
            {
                var path = PathHandle.GetGiftPath(ctx.Guild);
                File.Delete(path);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{ctx.User.Username} cleared gifts pool").ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

    }
}
