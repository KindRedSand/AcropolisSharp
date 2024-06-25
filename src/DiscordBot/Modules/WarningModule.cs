using System.Collections.Immutable;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Database;
using DiscordBot.Locale;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Modules;

[CommandContextType(InteractionContextType.Guild)]
public class WarningModule(BotDatabase db, DiscordSocketClient client) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("warn", "Выдать предупреждение", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public async Task WarnUser(IGuildUser user, string reason)
    {
        await DeferAsync();
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
            await FollowupAsync(embed: new EmbedBuilder()
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
        msg = await FollowupAsync(embed: emb.Build(), ephemeral: false);
                

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
                await user.SetTimeOutAsync(warningsCount == 2 ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1));
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


    [SlashCommand("media", "Отключить встраивание для пользователя", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public async Task NoMedia(IGuildUser user)
    {
        await DeferAsync();

        var config = await db.GetNonTrackedConfig(Context.Guild.Id);
        if (config?.NoMediaRoleId == null)
        {
            await FollowupAsync("No media роль не задана :SMOrc:", ephemeral: false);
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
           
            emb.WithDescription($"С пользователя {user.Mention} снатя роль <@{config.NoMediaRoleId}>");
            await FollowupAsync(embed: emb.Build(), ephemeral: false);
            
            if (config?.WarningLogChannel != null)
            {
                var channel = Context.Guild.GetTextChannel(config.WarningLogChannel.Value);
                if (channel != null)
                {
                    emb = new EmbedBuilder()
                        .WithAuthor(user)
                        .WithDescription($"Модератор {Context.User.Mention} __снял__ роль <@{config.NoMediaRoleId.Value}> с пользователя {user.Mention}");
                    await channel.SendMessageAsync(embed: emb.Build());
                }
            }
        }
        else
        {
            await user.AddRoleAsync(config.NoMediaRoleId.Value);
         
            emb.WithDescription($"Пользователю {user.Mention} была выдана роль <@{config.NoMediaRoleId}>");
            await FollowupAsync(embed: emb.Build(), ephemeral: false);
            
            if (config?.WarningLogChannel != null)
            {
                var channel = Context.Guild.GetTextChannel(config.WarningLogChannel.Value);
                if (channel != null)
                {
                    emb = new EmbedBuilder()
                        .WithAuthor(user)
                        .WithDescription($"Модератор {Context.User.Mention} выдал роль <@{config.NoMediaRoleId.Value}> пользователю {user.Mention}");
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


    [SlashCommand("unwarn", "Убрать предупреждение", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public async Task UnWarnUser(IGuildUser user)
    {
        await DeferAsync();
        var warning = await db.Warnings.OrderBy(x => x.IssueTime)
            .LastOrDefaultAsync(x => x.GuildID == Context.Guild.Id && x.UserID == user.Id);

        var emb = new EmbedBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        if (warning == null)
        {
            emb.WithDescription($"У пользователя {user.Mention} нет предупреждений");
            await FollowupAsync(embed: emb.Build(), ephemeral: false);

            return;
        }

        db.Remove(warning);
        await db.SaveChangesAsync();
        
        emb.WithDescription($"С пользователя {user.Mention} снято предупреждение");
        await FollowupAsync(embed: emb.Build(), ephemeral: false);
        
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
                    .WithDescription($"Модератор {Context.User.Mention} снял с пользователя {user.Mention} предупреждение");
                await channel.SendMessageAsync(embed: emb.Build());
            }
        }
    }

    [SlashCommand("my-warnings", "Посмотреть свои предупреждения", runMode: RunMode.Async)]
    public async Task MyWarnings()
    {
        await DeferAsync(true);

        var emb = await getUserWarningsEmbed(Context.Guild.Id, Context.User, Context.Interaction.UserLocale, false);
        
        emb.WithAuthor(Context.User)
            .WithColor(ConfigModule.EmbedColor);

        await FollowupAsync(embed: emb.Build(), ephemeral: true);
    }

    [SlashCommand("warnings", "Показать предупреждения пользователя", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public async Task Warnings(IGuildUser user)
    {
        await DeferAsync();

        var emb = await getUserWarningsEmbed(Context.Guild.Id, user, Context.Interaction.UserLocale, true);
        
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);

        await FollowupAsync(embed: emb.Build(), ephemeral: false);
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