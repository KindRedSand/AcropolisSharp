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
        
        if(user == null)
            await RespondAsync($"Удаление {howMany} сообщений", ephemeral: true);
        else
            await RespondAsync($"Удаление сообщений от пользователя {user.Mention} среди последних {howMany} сообщений", ephemeral: true);
        ImmutableArray<IMessage> messagesToDelete;
        
        if (user != null)
            messagesToDelete = [..messages.Where(x => x.Author.Id == user.Id).Reverse()];
        else
            messagesToDelete = [..messages.Reverse()];
        
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messagesToDelete);
        var cfg = await db.GetNonTrackedConfig(Context.Guild.Id);
        
        if(cfg?.LogChannel == null)
            return;
        var logChannel = Context.Guild.GetChannel(cfg.LogChannel.Value) as SocketTextChannel;
        if(logChannel == null)
            return;
        foreach (var msg in messagesToDelete)
        {
            if(msg.Author.IsBot)
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
                for (int i = 0; i < attachments.Length; i++)
                {
                    sb.Append($"[file{i}]({attachments[i]})\n");
                }
            }
            emb.WithDescription(sb.ToString());
    
            await logChannel.SendMessageAsync(embed: emb.Build());
        }
    }
}