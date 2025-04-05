using Discord;
using Discord.Interactions;
using DiscordBot.Database;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Modules;

public class StarModule(BotDatabase db) : InteractionModuleBase<SocketInteractionContext>
{
    const string Star_Code = @"⭐";
    
    [CommandContextType(InteractionContextType.Guild)]

    [SlashCommand("star-channel", "Set specific channel as starboard", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetChannel(IChannel channel)
    {
        var cfg = await GetConfig(Context.Guild.Id);
        cfg.ChannelID = channel.Id;
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await RespondAsync($"Канал <#{channel.Id}> будет ипользоваться как доска почета", ephemeral: true);
                break;
            default:
                await RespondAsync($"You set channel <#{channel.Id}> as starboard channel", ephemeral: true);
                break;
        }
    }

    [CommandContextType(InteractionContextType.Guild)]
    [SlashCommand("star-count", "Set specific channel as starboard", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetCount([MinValue(1)]int count)
    {
        var cfg = await GetConfig(Context.Guild.Id);
        cfg.ReactionsThreshold = count;
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await RespondAsync($"Сообщения будут добавляться на доску почета по достежению {count} {Star_Code}", ephemeral: true);
                break;
            default:
                await RespondAsync($"Now messages will be sent to starboard once they recieve {count} {Star_Code}", ephemeral: true);
                break;
        }
    }

    
    async Task<StarConfig> GetConfig(ulong guildId)
    {
        var config = await db.StarConfig.FirstOrDefaultAsync(x => x.Id == guildId);
        if (config != null) return config;

        config = new StarConfig()
        {
            Id = guildId,
            ReactionsThreshold = 1,
        };
        
        await db.AddAsync(config);
        return config;
    }
}