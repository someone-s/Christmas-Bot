using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

#nullable enable
namespace Christmas_bot
{
    internal static class MessageHandle
    {
        public static async Task SendSuccess(DiscordChannel channel, 
            string? message = null, 
            string? imageurl = null, 
            string? url = null,
            string? alternatetitle = null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"success",
                Color = DiscordColor.Green
            };
            if (alternatetitle is not null)
                embed.Title = alternatetitle;
            if (message is not null)
                embed.Description = message;
            if (imageurl is not null && TextHandle.isEmbedableImage(imageurl))
                embed.ImageUrl = imageurl;
            if (url is not null)
                embed.Url = url;
            await channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        public static async Task SendError(DiscordChannel channel, string? message = null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"error",
                Color = DiscordColor.Red
            };
            if (message is not null)
                embed.Description = message;
            await channel.SendMessageAsync(embed).ConfigureAwait(false);
        }
    }
}
