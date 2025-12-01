using System.Collections.Immutable;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Database;

namespace DiscordBot.Modules;

[CommandContextType(InteractionContextType.Guild)]
public class PurgeModule(BotDatabase db) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("purge", "Удалить сообщения", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task Purge(int howMany, IGuildUser? user = null)
    {
        var messages = await Context.Channel.GetMessagesAsync(howMany).FlattenAsync();

        if (user == null)
            await RespondAsync($"Удаление {howMany} сообщений", ephemeral: true);
        else
            await RespondAsync($"Удаление сообщений от пользователя {user.Mention} среди последних {howMany} сообщений",
                ephemeral: true);
        ImmutableArray<IMessage> messagesToDelete;

        if (user != null)
            messagesToDelete = [..messages.Where(x => x.Author.Id == user.Id).Reverse()];
        else
            messagesToDelete = [..messages.Reverse()];

        await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messagesToDelete);
        var cfg = await db.GetNonTrackedConfig(Context.Guild.Id);

        if (cfg?.LogChannel == null)
            return;
        var logChannel = Context.Guild.GetChannel(cfg.LogChannel.Value) as SocketTextChannel;
        if (logChannel == null)
            return;
        foreach (var msg in messagesToDelete)
        {
            if (msg.Author.IsBot)
                continue;

            var emb = new EmbedBuilder();
            var sb = new StringBuilder();
            var usr = msg.Author;

            emb.WithAuthor(usr)
                .WithColor(ConfigModule.EmbedColor)
                .WithTitle($"Сообщение в канале <#{Context.Channel.Id}> было удалено:");
            sb.Append($"Модератор {Context.User.Mention} использовал /purge\n\n");
            sb.Append(msg.Content);
            var attachments = msg.Attachments.Select(x => x.Url).ToImmutableArray();
            if (msg.Attachments.Count > 0)
            {
                sb.Append('\n');
                emb.WithImageUrl(attachments[0]);
                for (var i = 0; i < attachments.Length; i++) sb.Append($"[file{i}]({attachments[i]})\n");
            }

            emb.WithDescription(sb.ToString());

            await logChannel.SendMessageAsync(embed: emb.Build());
        }
    }
    
    [SlashCommand("user-purge", "Удалить среди последних 200 сообщений от одного пользователя", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task UserPurge(IGuildUser user, bool kick = false)
    {
        var responded = false;
        if (kick)
        {
            if (user.GuildPermissions.KickMembers || user.GuildPermissions.BanMembers || user.GuildPermissions.ManageMessages||
                Context.Client.CurrentUser.Id == user.Id)
                kick = false;
            else if (!Context.Guild.GetUser(Context.Client.CurrentUser.Id).GuildPermissions.KickMembers)
            {
                await RespondAsync($"Удаление сообщений от пользователя {user.Mention} во всех каналах." +
                                   $"У бота нет прав на исключение пользователей!",
                    ephemeral: true);
                responded = true;
            }
            else
            {
                await user.KickAsync($"Пользователь был изгнан модератором {Context.User.Username} во время чистки сообщений.");
            }
        }
        
        if(!responded)
            await RespondAsync($"Удаление сообщений от пользователя {user.Mention} во всех каналах",
                ephemeral: true);

        foreach (var socketGuildChannel in Context.Guild.Channels.Where(x => x is ITextChannel))
        {
            var channel = (ITextChannel) socketGuildChannel;
            try
            {
                var messages = await channel.GetMessagesAsync(200).FlattenAsync();

                ImmutableArray<IMessage> messagesToDelete = [..messages.Where(x => x.Author.Id == user.Id &&
                    x.Timestamp.Date.AddDays(12) > DateTime.Now).Reverse()];

                if (!messagesToDelete.IsEmpty)
                    await channel.DeleteMessagesAsync(messagesToDelete);
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine($"Failed to receive messages for deletion from {channel.Name} because some of them are older than two weeks.");
                }
            }
            
        }
        
        var cfg = await db.GetNonTrackedConfig(Context.Guild.Id);

        if (cfg?.LogChannel == null)
            return;
        var logChannel = Context.Guild.GetChannel(cfg.LogChannel.Value) as SocketTextChannel;
        if (logChannel == null)
            return;
        
        var emb = new EmbedBuilder();
        var sb = new StringBuilder();
        emb.WithAuthor(user)
            .WithColor(ConfigModule.EmbedColor);
        sb.Append(
            $"Модератор {Context.User.Mention} использовал /user-purge на пользователе {user.DisplayName}({user.Mention})!\n\n");
        sb.Append(kick ? "Пользователь был изгнан с сервера." : "Пользователь не был автоматически изгнан.");
        emb.WithDescription(sb.ToString());
        await logChannel.SendMessageAsync(embed: emb.Build());
    }
}