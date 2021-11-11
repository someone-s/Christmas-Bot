using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Christmas_bot.Commands
{
    public class InfoCommands : BaseCommandModule
    {
        [Command("version")]
        [Aliases("v")]
        [Description("get the bot version")]
        public async Task GetInfo(CommandContext ctx)
        {
            var v = Assembly.GetEntryAssembly().GetName().Version;

            await ctx.Channel.SendMessageAsync(v.ToString()).ConfigureAwait(false);
        }
    }
}
