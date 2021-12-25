using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Christmas_bot.Commands
{
    public partial class GiftCommands : BaseCommandModule
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
                var isvalid = TextHandle.isValidUrl(filtered);

                if (type == "text")
                {
                    await MessageHandle.SendSuccess(ctx.Channel,
                        message: $"{filtered} added by {ctx.User.Username}").ConfigureAwait(false);
                    GiftHandle.AddGift(ctx.Guild, ctx.User, $"text:{filtered}");
                }
                else if (isvalid &&
                    type == "url")
                {
                    await MessageHandle.SendSuccess(ctx.Channel,
                               message: $"{filtered} added by {ctx.User.Username}",
                               url: filtered).ConfigureAwait(false);
                    GiftHandle.AddGift(ctx.Guild, ctx.User, $"url:{filtered}");
                }
                else if (isvalid &&
                    type == "imageurl")
                {
                    await MessageHandle.SendSuccess(ctx.Channel,
                        message: $"{filtered} added by {ctx.User.Username}",
                        url: filtered,
                        imageurl: filtered).ConfigureAwait(false);
                    GiftHandle.AddGift(ctx.Guild, ctx.User, $"image:{filtered}");
                }
                else
                {
                    await MessageHandle.SendError(ctx.Channel,
                        message: $"unsupported gift type {type}").ConfigureAwait(false);
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

            var buttonb = new DiscordButtonComponent
            (
                style: ButtonStyle.Primary,
                customId: $"{ctx.Guild.Id}-{ctx.Channel.Id}-{DateTime.UnixEpoch}-B",
                label: "",
                emoji: new DiscordComponentEmoji("⬅️")
            );

            var buttonf = new DiscordButtonComponent
            (
                style: ButtonStyle.Primary,
                customId: $"{ctx.Guild.Id}-{ctx.Channel.Id}-{DateTime.UnixEpoch}-F",
                label: "",
                emoji: new DiscordComponentEmoji("➡️")
            );

            var entries = GiftHandle.ReadGifts(ctx.Guild);
            for (int i = 0; i < 10; i++)
            {
                if (i >= entries.Count) break;

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

            embed.WithFooter($"1/{MathF.Ceiling(entries.Count / 10f)}");

            var output = new DiscordMessageBuilder();
            output.AddEmbed(embed);
            output.AddComponents(buttonb, buttonf);

            await ctx.Channel.SendMessageAsync(output).ConfigureAwait(false);

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
                if (GiftHandle.RemoveGift(ctx.Guild, index - 1))
                    await MessageHandle.SendSuccess(ctx.Channel,
                        message: "gift removed from pool").ConfigureAwait(false);
                else
                    await MessageHandle.SendError(ctx.Channel,
                        message: $"invalid operation").ConfigureAwait(false);
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
                        var original = TextHandle.CleanText(gifts[i].Item2);

                        var gift = string.Empty;
                        if (original.StartsWith("text:")) gift = original.Remove(0, 5);
                        else if (original.StartsWith("url:")) gift = original.Remove(0, 4);
                        else if (original.StartsWith("image:")) gift = original.Remove(0, 6);

                        var isvalid = TextHandle.isValidUrl(gift);

                        if (original.StartsWith("text:") || !isvalid)
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got `{gift}` from {sender.Mention}").ConfigureAwait(false);
                        else if (original.StartsWith("url:") && isvalid)
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift).ConfigureAwait(false);
                        else if (original.StartsWith("image:") && isvalid)
                            await MessageHandle.SendSuccess(ctx.Channel,
                                alternatetitle: "present",
                                message: $"{user.Mention} got this from {sender.Mention}",
                                url: gift,
                                imageurl: gift).ConfigureAwait(false);

                        used.Add(i);
                    }

                    GiftHandle.ClearGifts(ctx.Guild);
                    if (!flip) GiftHandle.AddGifts(ctx.Guild, gifts.Where((t, i) => !used.Contains(i)).ToList());
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
                GiftHandle.ClearGifts(ctx.Guild);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{ctx.User.Username} cleared gifts pool").ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }
    }

    public partial class GiftCommands
    {
        public static async Task ListGiftInteraction(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            if (args.Interaction.Data.ComponentType != ComponentType.Button) return;

            if (args.Message.Embeds.Count < 1) return;

            var embed = new DiscordEmbedBuilder(args.Message.Embeds[0]);
            var footer = embed.Footer.Text;

            if (footer == null) return;
            if (!footer.Contains('/')) return;

            var items = footer.Split('/');

            if (items.Count() < 1) return;
            if (!int.TryParse(items[0], out var index)) return;
            index -= 1;

            Console.WriteLine(args.Interaction.Data.CustomId);

            if (args.Interaction.Data.CustomId.EndsWith('F')) index += 1;
            else if (args.Interaction.Data.CustomId.EndsWith('B')) index -= 1;
            else return;

            embed.RemoveFieldRange(0, embed.Fields.Count);

            var entries = GiftHandle.ReadGifts(args.Guild);

            index = Math.Clamp(index, 0, entries.Count / 10);

            for (int i = 0; i < 10; i++)
            {
                var t = i + index * 10;
                if (t >= entries.Count) break;

                var entry = entries[t];
                var user = entry.Item1;
                var gift = TextHandle.CleanText(entry.Item2);
                if (gift.StartsWith("text:"))
                    embed.AddField($"{t + 1}: {gift.Insert(5, " ")}", user.Username);
                else if (gift.StartsWith("url:"))
                    embed.AddField($"{t + 1}: {gift.Insert(4, " ")}", user.Username);
                else if (gift.StartsWith("image:"))
                    embed.AddField($"{t + 1}: {gift.Insert(6, " ")}", user.Username);
            }

            embed.WithFooter($"{index + 1}/{MathF.Ceiling(entries.Count / 10f)}");

            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage).ConfigureAwait(false);
            await args.Message.ModifyAsync(new Optional<DiscordEmbed>(embed)).ConfigureAwait(false);
        }
    }
}
