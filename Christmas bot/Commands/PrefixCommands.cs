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
    public class PrefixCommands : BaseCommandModule
    {
        [Command("prefix-add")]
        [Aliases("pa", "add-p")]
        [Description("add prefix")]
        public async Task AssignAdmin(CommandContext ctx,
            [Description("prefix to add")] string newprefix)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsAdmin(ctx.Guild, ctx.User))
            {
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not owner").ConfigureAwait(false);
            }
            else
            {
                PrefixHandle.AddPrefix(ctx.Guild, newprefix);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{newprefix} added as prefix").ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("prefix-remove")]
        [Aliases("pr", "rm-p")]
        [Description("remove prefix")]
        public async Task RelegateAdmin(CommandContext ctx,
            [Description("prefix to remove")] string formerprefix)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            if (!RoleHandle.IsAdmin(ctx.Guild, ctx.User))
                await MessageHandle.SendError(ctx.Channel,
                    message: $"{ctx.User.Username} is not owner");
            else
            {
                PrefixHandle.RemovePrefix(ctx.Guild, formerprefix);

                await MessageHandle.SendSuccess(ctx.Channel,
                    message: $"{formerprefix} removed").ConfigureAwait(false);
            }

            await message.DeleteAsync();
        }

        [Command("prefix-list")]
        [Aliases("pl", "listp")]
        [Description("list prefix")]
        public async Task ListAdmin(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };

            var prefixes = PrefixHandle.GetPrefixes(ctx.Guild);
            foreach (var prefix in prefixes) embed.AddField(prefix, "prefix");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);

            await message.DeleteAsync();
        }
    }
}
