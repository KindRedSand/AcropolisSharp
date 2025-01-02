using System.Globalization;
using System.Reflection;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Database;
using DiscordBot.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Playground.Mindustry.Blocks;

//Host independent culture
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;


var messages = new MessageModel?[1000];
var messagesIndex = 0;


string oatoken;

if (!File.Exists(".secret") ||
    string.IsNullOrEmpty(oatoken = File.ReadAllText(".secret").Trim()))
{
    Console.WriteLine("Please, fill .secret file before launching this bot!");
    File.Open(".secret", FileMode.OpenOrCreate).Close();
    Console.ReadKey();
    return;
}

//Initialise Parser
Blocks.Load();
Items.Load();

// var db = new BotDatabase();
// await db.Database.EnsureCreatedAsync();


var client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds |
                     GatewayIntents.GuildEmojis | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages |
                     GatewayIntents.MessageContent | GatewayIntents.DirectMessageReactions |
                     GatewayIntents.GuildBans | GatewayIntents.GuildMembers,
    UseInteractionSnowflakeDate = false //Sometimes give false positive System.TimeoutException
});

var commands = new CommandService();
var interaction = new InteractionService(client);
IReadOnlyCollection<RestGlobalCommand> globalCommands = null!;
interaction.Log += Log;


var cts = new CancellationTokenSource();

AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
{
    Console.WriteLine("Exiting...");
    await client.StopAsync();
    //Release main thread
    cts.Cancel();
};

var services = new ServiceCollection()
    .AddSingleton(client)
    .AddSingleton(interaction)
    .AddSingleton(commands)
    // .AddSingleton<SQLiteDbConnector>()
    .AddSingleton<HttpClient>()
    .AddDbContext<BotDatabase>(x => { x.UseSqlite("Data Source=base.db"); })
    .BuildServiceProvider();


// var db = services.GetService<BotDatabase>()!;
// await db.ApplyMigrations();

var db = services.GetService<BotDatabase>()!;
await db.Database.EnsureCreatedAsync();
await db.ApplyMigrations();

client.MessageReceived += OnMessage;
client.MessageUpdated += OnEdit;
client.MessageDeleted += OnDelete;
client.ReactionAdded += OnReaction;
client.ReactionRemoved += OnReactionRemoved;
client.UserJoined += OnUserJoin;
client.Log += Log;

bool clearGuildCommands = false;
ulong guildId = 0;
//Skip commands registration in order to clear guild commands
if (args.Length <= 1 || args[0] != "-rgc" || !ulong.TryParse(args[1], out guildId))
{
    await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), services);
}
else
    clearGuildCommands = true;


client.InteractionCreated += async x =>
{
    var context = new SocketInteractionContext(client, x);

    // Execute the incoming command.
    _ = await interaction.ExecuteCommandAsync(context, services);
};
client.Ready += async () =>
{
    if (clearGuildCommands)
    {
        await interaction.RegisterCommandsToGuildAsync(guildId);
        Console.WriteLine("Guild commands now should be removed. Exiting...");
        cts.Cancel();
    }
    else
    {
        globalCommands = await interaction.RegisterCommandsGloballyAsync();
        await client.SetActivityAsync(new Game("ping for help", ActivityType.Watching));
    }
};


await client.LoginAsync(TokenType.Bot, oatoken);
await client.StartAsync();

//Just wait until exit
while (!cts.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(60)))
{
    //Periodic tasks

    //Moved to db.GetUserWarnings();
    //It should automatically clean up any expired warnings
    // var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    // var expiredWarnings = db.Warnings
    //     .Where(x => x.ExpireTime < currentTime).ToImmutableArray();
    // if(expiredWarnings.Length > 0)
    // {
    //     db.Warnings.RemoveRange(expiredWarnings);
    //     await db.SaveChangesAsync();
    // }
}


async Task OnReaction(Cacheable<IUserMessage, ulong> cmsg, Cacheable<IMessageChannel, ulong> cchannel,
    SocketReaction reaction)
{
    if (!reaction.User.IsSpecified)
        return;
    var msg = (RestUserMessage)(reaction.Message.IsSpecified ? reaction.Message.Value : await reaction.Channel.GetMessageAsync(cmsg.Id));
    
    switch (reaction.Emote.Name)
    {
        case "\u274c":
            await OnMapDeletion(msg, reaction);
            return;
        case "\u2b50":
            await OnStar(msg, reaction);
            break;
    }
}

async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cmsg, Cacheable<IMessageChannel, ulong> cchannel,
    SocketReaction reaction)
{
    if (reaction.Channel is not SocketGuildChannel gchan)
        return;
    
    if (reaction.Emote.Name == "\u2b50")
    {
        if (!reaction.User.IsSpecified)
            return;
        var msg = (RestUserMessage)(reaction.Message.IsSpecified ? reaction.Message.Value : await reaction.Channel.GetMessageAsync(cmsg.Id));
        
        var rCount =  msg.Reactions.FirstOrDefault(x => x.Key.Name == "\u2b50").Value.ReactionCount;
        

        var cfg = await db.GetNonTrackedStarConfig(gchan.Guild.Id);
        if (cfg?.ChannelID is null)
            return;
        
        var msgEntry = await db.StarMessage.FirstOrDefaultAsync(x => x.Id == msg.Id);
        if (msgEntry == null || msgEntry.LastCount == rCount || msgEntry.StarboardMessage == null)
            return;

        if (msg.Author.Id == reaction.UserId)
            msgEntry.OwnerReacted = false;

        if (msgEntry.OwnerReacted)
            rCount -= 1;
        
        msgEntry.LastCount = rCount;
    
        //Here are no way we can have ChannelID as null, but Roslyn still complain
        var starboardCh = ((IMessageChannel) client.GetChannel(cfg.ChannelID ?? 0));
        
        if (rCount <= Math.Max(cfg.ReactionsThreshold - 4, 0))
        {
            if(msgEntry.StarboardMessage != null && msgEntry.StarboardMessage != 0)
                await starboardCh.DeleteMessageAsync(msgEntry.StarboardMessage ?? 0);
            msgEntry.StarboardMessage = null;
            //Force update cache
            db.StarMessage.Update(msgEntry);
            db.SaveChanges();
            return;
        }
    }
}

async Task OnMapDeletion(RestUserMessage msg, SocketReaction reaction)
{
    //Only check reactions on own messages
    if (msg.Author.Id != client.CurrentUser.Id || reaction.UserId == client.CurrentUser.Id)
        return;
    
    if (msg.Embeds.Count > 0)
    {
        var emb = msg.Embeds.First();
        if (emb.Footer?.Text.Contains("UserID:") ?? false)
        {
            string sid = emb.Footer.Value.Text.Split("UserID: ").Last();
            if (!ulong.TryParse(sid, out ulong id))
                return;
            if (reaction.User.Value.Id == id) await msg.DeleteAsync();
            else await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
        }
    }
}

async Task OnStar(RestUserMessage msg, SocketReaction reaction)
{
    if (msg.Author.Id == client.CurrentUser.Id /*|| msg.Author.Id == reaction.UserId*/)
        return;
    if (msg.Channel is not SocketGuildChannel gchan)
        return;
    
    var rCount =  msg.Reactions.FirstOrDefault(x => x.Key.Name == "\u2b50").Value.ReactionCount;
    
    var cfg = await db.GetNonTrackedStarConfig(gchan.Guild.Id);
    if (cfg?.ChannelID is null)
        return;
    
    var (msgEntry, exist) = await db.GetStarMessageEntry(msg.Id, msg);
    
    if (msgEntry.LastCount == rCount)
        return;
    
    if (msg.Author.Id == reaction.UserId)
        msgEntry.OwnerReacted = true;

    if (msgEntry.OwnerReacted)
        rCount -= 1;
    
    msgEntry.LastCount = rCount;
    
    //Here are no way we can have ChannelID as null, but Roslyn still complain
    var starboardCh = ((IMessageChannel) client.GetChannel(cfg.ChannelID ?? 0));

    //We reached threshold and we don't have printed msg
    if (msgEntry.StarboardMessage is null && rCount >= cfg.ReactionsThreshold)
    {
        var emb = new EmbedBuilder().WithAuthor(msg.Author).WithColor(Color.LightOrange)
            .WithDescription(msg.Content);

        // if (!string.IsNullOrEmpty(msgEntry.AttachmentUrl))
        //     emb = emb.WithImageUrl(msgEntry.AttachmentUrl);
        if (msg.Attachments.Count > 0)
        {
            foreach (var attachment in msg.Attachments.Where(x => x.ContentType.StartsWith("image/") || x.ContentType.StartsWith("video/")))
            {
                emb = emb.WithImageUrl(attachment.Url);
                if(attachment.ContentType.StartsWith("image/"))
                    emb = emb.WithImageUrl(attachment.Url);
                if (attachment.ContentType.StartsWith("video/"))
                    emb = emb.AddField("Video", attachment.Url);
            }
        }
        if (msg.Embeds.Count > 0)
        {
            foreach (var embed in msg.Embeds.Where(x => x.Type is EmbedType.Image or EmbedType.Video))
            {
                if(embed.Image.HasValue)
                    emb = emb.WithImageUrl(embed.Image.Value.Url);
                if (embed.Video.HasValue)
                    emb = emb.AddField("Video", embed.Video.Value.Url);
            }
        }
        //We can rely on discord embeds here 
        // if (msg.Content.StartsWith("https://"))
        // {
        //     try
        //     {
        //         //Weird way to validate this, but atm i already have headache
        //         var uri = new Uri(msg.Content);
        //         emb = emb.WithImageUrl(msg.Content);
        //     }
        //     catch (Exception)
        //     {
        //         // ignored
        //     }
        // }
        
        var r = await starboardCh.SendMessageAsync($"{rCount} \u2b50 {msg.GetJumpUrl()}",embed: emb.Build());
        msgEntry.StarboardMessage = r.Id;

        if (!exist)
            await db.AddAsync(msgEntry);
        else
            db.Update(msgEntry);
        await db.SaveChangesAsync();
        return;
    }

    try
    {
        if (rCount <= Math.Max(cfg.ReactionsThreshold - 4, 0))
        {
            if(msgEntry.StarboardMessage != null && msgEntry.StarboardMessage != 0)
                await starboardCh.DeleteMessageAsync(msgEntry.StarboardMessage ?? 0);
            
            msgEntry.StarboardMessage = null;
            
            //Force update cache
            if (!exist)
                await db.AddAsync(msgEntry);
            else
                db.Update(msgEntry);
            await db.SaveChangesAsync();
            return;
        }
        if(msgEntry.StarboardMessage is null or 0)
            return;

        var rest = (RestUserMessage)await starboardCh.GetMessageAsync(msgEntry.StarboardMessage ?? 0);
        await rest.ModifyAsync(x => x.Content = $"{rCount} \u2b50 {msg.GetJumpUrl()}");
    }
    catch (Exception e)
    {
        Console.WriteLine( new LogMessage(LogSeverity.Error, "Reactions", "It appears what or starboard channel are changed, or original message get deleted." +
                                                                          " This entry will be purged from the database", e));
        db.StarMessage.Remove(msgEntry);
        await db.SaveChangesAsync();
    }
    await db.SaveChangesAsync();
}

//Reassign NoMedia role upon rejoin
async Task OnUserJoin(SocketGuildUser user)
{
    var entry = await db.NoMedia.AsNoTracking()
        .FirstOrDefaultAsync(x => x.GuildID == user.Guild.Id && x.UserID == user.Id);
    if (entry == null)
        return;
    
    var config = await db.GetNonTrackedConfig(user.Guild.Id);
    if (config == null)
    {
        //TODO: Properly log invalid states
        Console.WriteLine(
            $"Found NoMedia entry for user {user.Id} in guild {user.Guild.Name} but guild don't have config entry!");
        return;
    }

    if (config.NoMediaRoleId == null)
        return;
    var role = user.Guild.GetRole(config.NoMediaRoleId.Value);
    if (role == null)
        return;
    await user.AddRoleAsync(role);
}

#region Message Logging

async Task OnMessage(SocketMessage msg)
{
    if (msg.Author.IsBot || //We don't want to log others bots garbage
        msg.Channel is not SocketGuildChannel channel)
        return;

    if (msg is SocketUserMessage umsg)
    {
        if (umsg.Content.Trim()
            .StartsWith(client.CurrentUser.Mention)) 
            if (msg.Author is SocketGuildUser user)
            {
                var emb = new EmbedBuilder()
                    .WithColor(ConfigModule.EmbedColor)
                    .WithTitle("Помощь | Список доступных команд")
                    .WithFooter("Это временное сообщение, иcчезающее через 30 секунд");
                var sb = new StringBuilder();
                foreach (var cmd in globalCommands)
                {
                    if (cmd.Name == "wrn") // Skip this alias
                        continue;

                    if (cmd.DefaultMemberPermissions.RawValue == 0 ||
                        (cmd.DefaultMemberPermissions.RawValue & user.GuildPermissions.RawValue) > 0ul)
                        sb.Append($"</{cmd.Name}:{cmd.Id}> - {cmd.Description}\n");
                }

                var field = new EmbedFieldBuilder()
                    .WithName("Нажмите на команду, чтобы воспользоваться ей")
                    .WithValue(sb.ToString());
                emb.WithFields(field);
                var rest = await msg.Channel.SendMessageAsync(embed: emb.Build());
                _ = Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    try
                    {
                        rest.DeleteAsync();
                    }
                    catch (Exception)
                    {
                        // In case someone delete this message before us
                    }
                });
            }
    }

    var messageModel = new MessageModel
    {
        Id = msg.Id,
        UserID = msg.Author.Id,
        Content = msg.Content,
        AttachmentsUrls = msg.Attachments.Select(x => x.Url).ToArray(),
        ChannelID = channel.Id,
        GuildID = channel.Guild.Id,
        LogTime = DateTime.UtcNow
    };

    messages[messagesIndex++] = messageModel;
    messagesIndex %= messages.Length;
}

async Task OnEdit(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel ichannel)
{
    if (msg.Author.IsBot || //We don't want to log others bots garbage
        msg.Channel is not SocketTextChannel channel)
        return;

    var messageModel =
        messages.FirstOrDefault(x => x?.Id == msg.Id);

    if (messageModel == null)
    {
        messageModel = new MessageModel
        {
            Id = msg.Id,
            UserID = msg.Author.Id,
            Content = msg.Content,
            AttachmentsUrls = msg.Attachments.Select(x => x.Url).ToArray(),
            ChannelID = channel.Id,
            GuildID = channel.Guild.Id,
            LogTime = DateTime.UtcNow
        };

        messages[messagesIndex++] = messageModel;
        messagesIndex %= messages.Length;
        return;
    }

    var config = await db.GetNonTrackedConfig(channel.Guild.Id);
    if (config?.LogChannel == null)
        return;

    var logChannel = channel.Guild.GetTextChannel(config.LogChannel.Value);
    if (logChannel != null)
    {
        var emb = new EmbedBuilder();
        var sb = new StringBuilder();
        emb.WithAuthor(msg.Author)
            .WithColor(ConfigModule.EmbedColor)
            .WithTitle(
                $"Сообщение в канале <#{channel.Id}> было изменено: https://discord.com/channels/{channel.Guild.Id}/{channel.Id}/{msg.Id}");

        sb.Append("**До:**\n");
        sb.Append(messageModel.Content);
        sb.Append("\n\n**После:**\n");
        sb.Append(msg.Content);

        if (messageModel.AttachmentsUrls?.Length > 0)
        {
            sb.Append('\n');
            emb.WithImageUrl(messageModel.AttachmentsUrls[0]);
            for (var i = 0; i < messageModel.AttachmentsUrls.Length; i++)
                sb.Append($"[file{i}]({messageModel.AttachmentsUrls[i]})\n");
        }

        emb.WithDescription(sb.ToString());

        await logChannel.SendMessageAsync(embed: emb.Build());
    }

    messageModel.Content = msg.Content;
    messageModel.AttachmentsUrls = msg.Attachments.Select(x => x.Url).ToArray();
    // db.Messages.Update(messageModel);
    // await db.SaveChangesAsync();
}

async Task OnDelete(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> cchannel)
{
    var messageModel =
        messages.FirstOrDefault(x => x?.Id == cache.Id);

    if (messageModel == null) return;

    var config = await db.GetNonTrackedConfig(messageModel.GuildID);
    if (config?.LogChannel == null)
        return;
    if (await client.GetChannelAsync(config.LogChannel.Value) is SocketTextChannel logChannel)
    {
        var emb = new EmbedBuilder();
        var sb = new StringBuilder();
        var usr = client.GetUser(messageModel.UserID);

        emb.WithAuthor(usr)
            .WithColor(ConfigModule.EmbedColor)
            .WithTitle($"Сообщение в канале <#{cchannel.Id}> было удалено:");

        sb.Append(messageModel.Content);

        if (messageModel.AttachmentsUrls?.Length > 0)
        {
            sb.Append('\n');
            emb.WithImageUrl(messageModel.AttachmentsUrls[0]);
            for (var i = 0; i < messageModel.AttachmentsUrls.Length; i++)
                sb.Append($"[file{i}]({messageModel.AttachmentsUrls[i]})\n");
        }

        emb.WithDescription(sb.ToString());

        await logChannel.SendMessageAsync(embed: emb.Build());
    }
}

#endregion

Task Log(LogMessage msg)
{
    return Task.Run(() => Console.WriteLine(msg.ToString()));
}