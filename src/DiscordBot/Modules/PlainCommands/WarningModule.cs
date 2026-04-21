using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Database;
using DiscordBot.Locale;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using RunMode = Discord.Commands.RunMode;

namespace DiscordBot.Modules.PlainCommands;

public class WarningModule(BotDatabase db, DiscordSocketClient client) : ModuleBase<SocketCommandContext>
{
    [Command("warn", RunMode = RunMode.Async)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task WarnUser(IUser user, [Remainder] string reason)
    {
        //await DeferAsync();
        var warning = new WarningModel
        {
            Summary = reason,
            IssueTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            GuildID = Context.Guild.Id,
            ModeratorId = Context.User.Id,
            UserID = user.Id
        };

        int warningsCount = (await db.GetUserWarnings(Context.Guild.Id, user.Id)).Count() + 1;

        if (warningsCount > 4 + 1)
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(ConfigModule.EmbedColor)
                .WithTitle("**У пользователя максимальное количество предупреждений!**")
                .Build());
            return;
        }

        var emb = new EmbedBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        IUserMessage msg;
        emb.WithDescription(
            $"""
             **Пользователь {user.Mention} предупрежден**
             Причина: {reason}
             Предупреждение #{warningsCount}
             """);
        msg = await ReplyAsync(embed: emb.Build());
                

        warning.WarningUrl = $"https://discord.com/channels/{Context.Guild.Id}/{Context.Channel.Id}/{msg.Id}";

        await db.AddAsync(warning);
        await db.SaveChangesAsync();
        
        var cfg = await db.GetNonTrackedConfig(Context.Guild.Id);
        var channel = cfg?.WarningLogChannel != null ? Context.Guild.GetTextChannel(cfg.WarningLogChannel.Value) : null;
        emb = await getUserWarningsEmbed(Context.Guild.Id, user, "ru", true);
        emb.WithAuthor(user);
        try
        {
            if(warningsCount > 1)
            {
                if(user is IGuildUser guildUser)
                    await guildUser.SetTimeOutAsync(warningsCount == 2 ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1));
            }
            if (channel != null)
            {
                emb.Description = $"""
                                   Модератором {Context.User.Mention} было выдано предупреждение пользователю {user.Mention}
                                   
                                   У пользователя {(warningsCount > 1 ? "уже " : string.Empty)}{LocaleHelper.Warnings(warningsCount)}!{(warningsCount > 1 ? $"\nАвтоматически выдан тайм-аут на 1 {(warningsCount == 2 ? "час" : "день")}" : string.Empty)}
                                   """;
                await channel.SendMessageAsync(embed: emb.Build());
            }
        }
        catch (Exception)
        {
            if (channel != null)
            {
                emb.Description = $"""
                                   Модератором {Context.User.Mention} было выдано предупреждение пользователю {user.Mention}
                                   
                                   У пользователя {(warningsCount > 1 ? "уже " : string.Empty)}{warningsCount} предупреждений!
                                   У бота недостаточно прав на выдачу тайм-аута, выдано только предупреждение
                                   """;
                await channel.SendMessageAsync(embed: emb.Build());
            }
        }
    }


    [Command("media", RunMode = RunMode.Async)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task NoMedia(IGuildUser user)
    {
        //await DeferAsync();

        var config = await db.GetNonTrackedConfig(Context.Guild.Id);
        if (config?.NoMediaRoleId == null)
        {
            await ReplyAsync("No media роль не задана :SMOrc:");
            return;
        }

        // Did we ever need db entry for this?
        var noMedia = await db.NoMedia.FirstOrDefaultAsync(x =>
            x.UserID == user.Id && x.GuildID == Context.Guild.Id);
        bool hasRole = user.RoleIds.Any(x => x == config.NoMediaRoleId);
        var emb = new EmbedBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        if (hasRole)
        {
            if (noMedia != null)
            {
                db.Remove(noMedia);
                await db.SaveChangesAsync();
            }

            await user.RemoveRoleAsync(config.NoMediaRoleId.Value);
           
            emb.WithDescription($"С пользователя {user.Mention} снята роль <@&{config.NoMediaRoleId}>");
            await ReplyAsync(embed: emb.Build());
            
            if (config?.WarningLogChannel != null)
            {
                var channel = Context.Guild.GetTextChannel(config.WarningLogChannel.Value);
                if (channel != null)
                {
                    emb = new EmbedBuilder()
                        .WithColor(ConfigModule.EmbedColor)
                        .WithAuthor(user)
                        .WithDescription($"Модератор {Context.User.Mention} __снял__ роль <@&{config.NoMediaRoleId.Value}> с пользователя {user.Mention}");
                    await channel.SendMessageAsync(embed: emb.Build());
                }
            }
        }
        else
        {
            await user.AddRoleAsync(config.NoMediaRoleId.Value);
         
            emb.WithDescription($"Пользователю {user.Mention} была выдана роль <@&{config.NoMediaRoleId}>");
            await ReplyAsync(embed: emb.Build());
            
            if (config?.WarningLogChannel != null)
            {
                var channel = Context.Guild.GetTextChannel(config.WarningLogChannel.Value);
                if (channel != null)
                {
                    emb = new EmbedBuilder()
                        .WithColor(ConfigModule.EmbedColor)
                        .WithAuthor(user)
                        .WithDescription($"Модератор {Context.User.Mention} выдал роль <@&{config.NoMediaRoleId.Value}> пользователю {user.Mention}");
                    await channel.SendMessageAsync(embed: emb.Build());
                }
            }
        }

        noMedia = new NoMediaModel
        {
            // Summary = reason,
            IssueTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            GuildID = Context.Guild.Id,
            ModeratorId = Context.User.Id,
            UserID = user.Id
        };
        await db.AddAsync(noMedia);
        await db.SaveChangesAsync();
    }


    [Command("unwarn", RunMode = RunMode.Async)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnWarnUser(IGuildUser user, int warnNumber = 1)
    {
        //await DeferAsync();
        if (warnNumber is < 1 or > 5)
        {
            try
            {
                var m = await ReplyAsync("warnNumber must be between 1 and 5.");
                await Task.Delay(TimeSpan.FromSeconds(10));
                await m.DeleteAsync();
            }
            catch
            {
                //ignored
            }
            return;
        }
        
        var warnings = db.Warnings.OrderBy(x => x.IssueTime)
            .Where(x => x.GuildID == Context.Guild.Id && x.UserID == user.Id)
            .ToImmutableArray();

        var emb = new EmbedBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        
        switch (warnings.Length)
        {
            case 0:
                emb.WithDescription($"У пользователя {user.Mention} нет предупреждений");
                await ReplyAsync(embed: emb.Build());
                return;
            case > 1 when warnNumber > warnings.Length:
                emb.WithDescription($"У пользователя {user.Mention} нет предупреждения под номером {warnNumber}. Всего у него {warnings.Length} предупреждений");
                await ReplyAsync(embed: emb.Build());
                return;
        }
        
        var warning = warnings[warnNumber - 1];

        db.Remove(warning);
        await db.SaveChangesAsync();
        
        emb.WithDescription($"С пользователя {user.Mention} снято предупреждение #{warnNumber}");
        await ReplyAsync(embed: emb.Build());
        
        if (user.TimedOutUntil != null)
        {
            await user.RemoveTimeOutAsync();
        }

        var config = await db.GetNonTrackedConfig(Context.Guild.Id);
        if (config?.WarningLogChannel != null)
        {
            var channel = Context.Guild.GetTextChannel(config.WarningLogChannel.Value);
            if (channel != null)
            {
                emb = new EmbedBuilder()
                    .WithColor(ConfigModule.EmbedColor)
                    .WithAuthor(user)
                    .WithDescription($"Модератор {Context.User.Mention} снял с пользователя {user.Mention} предупреждение #{warnNumber}");
                await channel.SendMessageAsync(embed: emb.Build());
            }
        }
    }

    // [Command("my-warnings", "Посмотреть свои предупреждения", runMode: RunMode.Async)]
    // public async Task MyWarnings()
    // {
    //     await DeferAsync(true);
    //
    //     var emb = await getUserWarningsEmbed(Context.Guild.Id, Context.User, Context.Interaction.UserLocale, false);
    //     
    //     emb.WithAuthor(Context.User)
    //         .WithColor(ConfigModule.EmbedColor);
    //
    //     await ReplyAsync(embed: emb.Build(), ephemeral: true);
    // }

    [Command("warnings",RunMode = RunMode.Async)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task Warnings(IGuildUser user)
    {
        //await DeferAsync();

        var emb = await getUserWarningsEmbed(Context.Guild.Id, user, "ru-RU", true);
        
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);

        await ReplyAsync(embed: emb.Build());
    }

    private async Task<EmbedBuilder> getUserWarningsEmbed(ulong guildId, IUser user, string userLocale,
        bool useMention = false)
    {
        var emb = new EmbedBuilder()
            .WithColor(ConfigModule.EmbedColor);
        var warnings = (await db.GetUserWarnings(guildId, user.Id)).ToImmutableArray();
        
        emb.WithDescription($"У {(useMention ? user.Mention : "вас")} {LocaleHelper.Warnings(warnings.Length)}");
        if (warnings.Length > 0)
        {
            var fields = new EmbedFieldBuilder[warnings.Length];
            for (var i = 0; i < warnings.Length; i++)
            {
                var mod = await client.GetUserAsync(warnings[i].ModeratorId);
                fields[i] = new EmbedFieldBuilder().WithName($"Предупреждение #{i + 1}")
                    .WithValue(
                        $"""
                         Выдано: {mod.Mention}
                         Причина: {warnings[i].Summary}
                         Ссылка: {warnings[i].WarningUrl}
                         Истекает <t:{warnings[i].ExpireTime}:R>
                         """)
                    .WithIsInline(true);
            }

            emb.WithFields(fields);
        }
        return emb;
    }
}