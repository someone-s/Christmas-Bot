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
    public class GiftCommands : BaseCommandModule
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
                        GiftHandle.AddGift(ctx.Guild, ctx.User, $"text:{filtered}");
                        break;
                    case "url":
                        await MessageHandle.SendSuccess(ctx.Channel,
                            message: $"{filtered} added by {ctx.User.Username}",
                            url: filtered).ConfigureAwait(false);
                        GiftHandle.AddGift(ctx.Guild, ctx.User, $"url:{filtered}");
                        break;
                    case "imageurl":
                        await MessageHandle.SendSuccess(ctx.Channel,
                            message: $"{filtered} added by {ctx.User.Username}",
                            url: filtered,
                            imageurl: filtered).ConfigureAwait(false);
                        GiftHandle.AddGift(ctx.Guild, ctx.User, $"image:{filtered}");
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

            var entries = GiftHandle.ReadGifts(ctx.Guild);
            for (int i = 0; i < entries.Count; i++)
            {
                if (i % 20 == 0 && i != 0)
                {
                    await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"success",
                        Color = DiscordColor.Green
                    };
                }
                var entry = entries[i];
                var user = entry.Item1;
                var gift = TextHandle.CleanText(entry.Item2);
                if (gift.StartsWith("text:"))
                    embed.AddField($"{i + 1}: {gift.Insert(5, " ")}", user.Username);
                else if (gift.StartsWith("url:"))
                    embed.AddField($"{i + 1}: {gift.Insert(4, " ")}", user.Username);
                else if (gift.StartsWith("image:"))
                    embed.AddField($"{i + 1}: {gift.Insert(6, " ")}", user.Username);
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

            if (GiftHandle.RemoveGift(ctx.Guild, index - 1))
                await MessageHandle.SendSuccess(ctx.Channel,
                    message: "gift removed from pool").ConfigureAwait(false);
            else
                await MessageHandle.SendError(ctx.Channel,
                    message: $"invalid operation").ConfigureAwait(false);

            await message.DeleteAsync();
        }

        [Command("gift-distribute")]
        [Aliases("gd", "dist-g")]
        [Description("distribute gifts to all user and remove them from gift pool")]
        public async Task DistributeGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var random = new Random();
            var gifts = GiftHandle.ReadGifts(ctx.Guild);
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
                    var gift = TextHandle.CleanText(gifts[i].Item2);

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

                GiftHandle.ClearGifts(ctx.Guild);
                if (!flip) GiftHandle.AddGifts(ctx.Guild, gifts.Where((t, i) => !used.Contains(i)).ToList());
            }

            await message.DeleteAsync();
        }

        [Command("gift-clear")]
        [Description("remove all gifts from gift pool")]
        public async Task ClearGift(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            GiftHandle.ClearGifts(ctx.Guild);

            await MessageHandle.SendSuccess(ctx.Channel,
                message: $"{ctx.User.Username} cleared gifts pool").ConfigureAwait(false);

            await message.DeleteAsync();
        }

    }
}
