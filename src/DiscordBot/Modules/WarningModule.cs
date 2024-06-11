using System.Collections.Immutable;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Database;
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
        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                emb.WithDescription($"**Пользователь {user.Mention} предупрежден**\n" +
                                    $"Причина: {reason}\n" +
                                    $"Предупреждение #{warningsCount}");
                msg = await FollowupAsync(embed: emb.Build(), ephemeral: false);
                break;
            default:
                emb.WithDescription($"**User {user.Mention} warned**\n" +
                                    $"Reason: {reason}\n" +
                                    $"Warning #{warningsCount}");
                msg = await FollowupAsync(embed: emb.Build(), ephemeral: false);
                break;
        }

        warning.WarningUrl = $"https://discord.com/channels/{Context.Guild.Id}/{Context.Channel.Id}/{msg.Id}";

        await db.AddAsync(warning);
        await db.SaveChangesAsync();

        if (warningsCount > 1)
        {
            var cfg = await db.GetNonTrackedConfig(Context.Guild.Id);
            var channel = cfg?.LogChannel != null ? Context.Guild.GetTextChannel(cfg.LogChannel.Value) : null;
            try
            {
                await user.SetTimeOutAsync(warningsCount == 2 ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1));
                if (channel != null)
                    await channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithAuthor(user)
                        .WithDescription(
                            $"""
                             У пользователя уже {warningsCount} предупреждений!
                             Автоматически выдан тайм-аут на 1 {(warningsCount == 2 ? "час" : "день")}
                             """
                        ).Build());
            }
            catch (Exception)
            {
                if(channel != null)
                    await channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithAuthor(user)
                        .WithDescription(
                            $"""
                             У пользователя уже {warningsCount} предупреждений!
                             У бота недостаточно прав на выдачу тайм-аута, выдано только предупреждение
                             """
                        ).Build());
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
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    emb.WithDescription($"С пользователя {user.Mention} снатя роль <@{config.NoMediaRoleId}>");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
                default:
                    emb.WithDescription($"<@{config.NoMediaRoleId}> role was removed from {user.Mention}");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
            }
        }
        else
        {
            await user.AddRoleAsync(config.NoMediaRoleId.Value);
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    emb.WithDescription($"Пользователю {user.Mention} была выдана роль <@{config.NoMediaRoleId}>");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
                default:
                    emb.WithDescription($"<@{config.NoMediaRoleId}> now have {user.Mention} role");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
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
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    emb.WithDescription($"У пользователя {user.Mention} нет предупреждений");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
                default:
                    emb.WithDescription($"User {user.Mention} don't have any wrnings");
                    await FollowupAsync(embed: emb.Build(), ephemeral: false);
                    break;
            }

            return;
        }

        db.Remove(warning);
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                emb.WithDescription($"С пользователя {user.Mention} снято предупреждение");
                await FollowupAsync(embed: emb.Build(), ephemeral: false);
                break;
            default:
                emb.WithDescription($"From user {user.Mention} removed one warning");
                await FollowupAsync(embed: emb.Build(), ephemeral: false);
                break;
        }
    }


    [SlashCommand("wrn", "Посмотреть предупреждения для фикса", runMode: RunMode.Async)]
    public async Task mwr()
    {
        await MyWarnings();
    }

    [SlashCommand("my-warnings", "Посмотреть свои предупреждения", runMode: RunMode.Async)]
    public async Task MyWarnings()
    {
        await DeferAsync(true);

        var warnings = (await db.GetUserWarnings(Context.Guild.Id, Context.User.Id)).ToImmutableArray();

        var emb = new EmbedBuilder();
        emb.WithAuthor(Context.User)
            .WithColor(ConfigModule.EmbedColor);
        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                emb.WithTitle($"У вас {warnings.Length} предупреждений");
                if (warnings.Length > 0)
                {
                    var fields = new EmbedFieldBuilder[warnings.Length];
                    for (var i = 0; i < warnings.Length; i++)
                    {
                        // var sb = new StringBuilder();
                        // var delta = DateTimeOffset.FromUnixTimeSeconds(warnings[i].ExpireTime) - DateTimeOffset.UtcNow;
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Days} дней ");
                        // }
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Hours} часов");
                        // }

                        var user = await client.GetUserAsync(warnings[i].ModeratorId);
                        fields[i] = new EmbedFieldBuilder().WithName($"Предупреждение #{i + 1}")
                            .WithValue(
                                 $"""
                                 Выдана: {user.Mention}\
                                 Причина: {warnings[i].Summary}
                                 Ссылка: {warnings[i].WarningUrl}
                                 Истекает <t:{warnings[i].ExpireTime}:R>
                                 """)
                            .WithIsInline(true);
                    }

                    emb.WithFields(fields);
                }

                await FollowupAsync(embed: emb.Build(), ephemeral: true);
                break;
            default:
                emb.WithTitle($"You have {warnings.Length} warnings");
                if (warnings.Length > 0)
                {
                    var fields = new EmbedFieldBuilder[warnings.Length];
                    for (var i = 0; i < warnings.Length; i++)
                    {
                        // var sb = new StringBuilder();
                        // var delta = DateTimeOffset.FromUnixTimeSeconds(warnings[i].ExpireTime) - DateTimeOffset.UtcNow;
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Days} day(s) ");
                        // }
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Hours} hours");
                        // }

                        var user = await client.GetUserAsync(warnings[i].ModeratorId);
                        fields[i] = new EmbedFieldBuilder().WithName($"Warning #{i + 1}")
                            .WithValue(
                                $"""
                                Issued by: {user.Mention}
                                Reason: {warnings[i].Summary}
                                Link: {warnings[i].WarningUrl}
                                Expire <t:{warnings[i].ExpireTime}:R>
                                """)
                            .WithIsInline(true);
                    }

                    emb.WithFields(fields);
                }

                await FollowupAsync(embed: emb.Build(), ephemeral: true);
                break;
        }
    }

    [SlashCommand("warnings", "Показать предупреждения пользователя", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public async Task Warnings(IGuildUser user)
    {
        await DeferAsync();

        var warnings = (await db.GetUserWarnings(Context.Guild.Id, user.Id)).ToImmutableArray();

        var emb = new EmbedBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                emb.WithDescription($"У {user.Mention} {warnings.Length} предупреждений");
                if (warnings.Length > 0)
                {
                    var fields = new EmbedFieldBuilder[warnings.Length];
                    for (var i = 0; i < warnings.Length; i++)
                    {
                        // var sb = new StringBuilder();
                        // var delta = DateTimeOffset.FromUnixTimeSeconds(warnings[i].ExpireTime) - DateTimeOffset.UtcNow;
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Days} дней ");
                        // }
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Hours} часов");
                        // }

                        var mod = await client.GetUserAsync(warnings[i].ModeratorId);
                        fields[i] = new EmbedFieldBuilder().WithName($"Предупреждение #{i + 1}")
                            .WithValue(
                                 $"""
                                 Выдана: {mod.Mention}
                                 Причина: {warnings[i].Summary}
                                 Ссылка: {warnings[i].WarningUrl}
                                 Истекает <t:{warnings[i].ExpireTime}:R> 
                                 """)
                            .WithIsInline(true);
                    }

                    emb.WithFields(fields);
                }

                await FollowupAsync(embed: emb.Build(), ephemeral: true);
                break;
            default:
                emb.WithDescription($"User {user.Mention} has {warnings.Length} warnings");
                if (warnings.Length > 0)
                {
                    var fields = new EmbedFieldBuilder[warnings.Length];
                    for (var i = 0; i < warnings.Length; i++)
                    {
                        // var sb = new StringBuilder();
                        // var delta = DateTimeOffset.FromUnixTimeSeconds(warnings[i].ExpireTime) - DateTimeOffset.UtcNow;
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Days} day(s) ");
                        // }
                        // if (delta.Days > 0)
                        // {
                        //     sb.Append($"{delta.Hours} hours");
                        // }

                        var mod = await client.GetUserAsync(warnings[i].ModeratorId);
                        fields[i] = new EmbedFieldBuilder().WithName($"Warning #{i + 1}")
                            .WithValue(
                                 $"""
                                 Issued by: {mod.Mention}
                                 Reason: {warnings[i].Summary}
                                 Link: {warnings[i].WarningUrl}
                                 Expire <t:{warnings[i].ExpireTime}:R>
                                 """)
                            .WithIsInline(true);
                    }

                    emb.WithFields(fields);
                }

                await FollowupAsync(embed: emb.Build(), ephemeral: false);
                break;
        }
    }
}