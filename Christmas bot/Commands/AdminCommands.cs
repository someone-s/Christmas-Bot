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
    public class AdminCommands : BaseCommandModule
    {
        [Command("gift-admin-add")]
        [Aliases("gaa", "add-a")]
        [Description("add gift admin")]
        public async Task AssignAdmin(CommandContext ctx,
            [Description("user to add")] DiscordUser newadmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            RoleHandle.AddAdmin(ctx.Guild, newadmin);

            await MessageHandle.SendSuccess(ctx.Channel,
                message: $"{newadmin.Username} added as admin").ConfigureAwait(false);

            await message.DeleteAsync();
        }

        [Command("gift-admin-remove")]
        [Aliases("gar", "rm-a")]
        [Description("remove gift admin")]
        public async Task RelegateAdmin(CommandContext ctx,
            [Description("user to remove")] DiscordUser formeradmin)
        {
            var message = await ctx.Channel.SendMessageAsync("processing...").ConfigureAwait(false);

            RoleHandle.RemoveAdmin(ctx.Guild, formeradmin);

            await MessageHandle.SendSuccess(ctx.Channel,
                message: $"{formeradmin.Username} relegated").ConfigureAwait(false);

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
}
