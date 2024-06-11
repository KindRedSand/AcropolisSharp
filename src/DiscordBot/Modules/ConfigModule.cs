using Discord;
using Discord.Interactions;
using DiscordBot.Database;
using Hjson;

namespace DiscordBot.Modules;

[CommandContextType(InteractionContextType.Guild)]
public class ConfigModule(BotDatabase db /*, DiscordSocketClient client*/)
    : InteractionModuleBase<SocketInteractionContext>
{
    static ConfigModule()
    {
        EmbedColor = new Color(255, 211, 127);
    }

    public static Color EmbedColor { get; private set; }

    [SlashCommand("cfg-log-channel", "Set specific channel for logging", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetLogChannel(ITextChannel channel)
    {
        await DeferAsync(true);
        var cfg = await db.GetOrCreateConfig(Context.Guild.Id);
        cfg.LogChannel = channel.Id;
        db.Update(cfg);
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await FollowupAsync($"Канал <#{channel.Id}> будет ипользоваться для логов", ephemeral: true);
                break;
            default:
                await FollowupAsync($"You set channel <#{channel.Id}> as log channel", ephemeral: true);
                break;
        }
    }

    [SlashCommand("cfg-map-channel", "Set specific channel map posting", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetMapChannel(ITextChannel channel)
    {
        await DeferAsync(true);
        var cfg = await db.GetOrCreateConfig(Context.Guild.Id);
        cfg.MapChannel = channel.Id;
        db.Update(cfg);
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await FollowupAsync($"Канал <#{channel.Id}> будет ипользоваться для публикаций карт", ephemeral: true);
                break;
            default:
                await FollowupAsync($"You set channel <#{channel.Id}> map posting", ephemeral: true);
                break;
        }
    }


    [SlashCommand("cfg-schemas-channel", "Set specific channel map posting", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetSchematicChannel(ITextChannel channel)
    {
        await DeferAsync(true);
        var cfg = await db.GetOrCreateConfig(Context.Guild.Id);
        cfg.SchematicChannel = channel.Id;
        db.Update(cfg);
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await FollowupAsync($"Канал <#{channel.Id}> будет ипользоваться для публикаций схем", ephemeral: true);
                break;
            default:
                await FollowupAsync($"You set channel <#{channel.Id}> schematic posting", ephemeral: true);
                break;
        }
    }


    [SlashCommand("cfg-no-media", "Set No Media role", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetNoMediaRole(IRole role)
    {
        await DeferAsync(true);
        var cfg = await db.GetOrCreateConfig(Context.Guild.Id);
        cfg.NoMediaRoleId = role.Id;
        db.Update(cfg);
        await db.SaveChangesAsync();

        switch (Context.Interaction.UserLocale)
        {
            case "ru":
                await FollowupAsync($"Роль {role.Mention} будет ипользоваться для /media", ephemeral: true);
                break;
            default:
                await FollowupAsync($"Role {role.Mention} will be used for /media", ephemeral: true);
                break;
        }
    }


    [SlashCommand("cfg-items-emotes", "Set schematic price emotes", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task SetItemEmotes(string copper, string lead, string metaglass,
        string graphite, string titanium, string thorium, string scrap,
        string silicon, string plastanium, string phaseFabric, string surgeAlloy,
        string beryllium, string tungsten, string oxide, string carbide)
    {
        await DeferAsync(true);
        var cfg = await db.GetOrCreateConfig(Context.Guild.Id);

        var json = new JsonObject();
        json.Add(new KeyValuePair<string, JsonValue>("lead", lead));
        json.Add(new KeyValuePair<string, JsonValue>("metaglass", metaglass));
        json.Add(new KeyValuePair<string, JsonValue>("graphite", graphite));
        json.Add(new KeyValuePair<string, JsonValue>("titanium", titanium));
        json.Add(new KeyValuePair<string, JsonValue>("thorium", thorium));
        json.Add(new KeyValuePair<string, JsonValue>("scrap", scrap));
        json.Add(new KeyValuePair<string, JsonValue>("silicon", silicon));
        json.Add(new KeyValuePair<string, JsonValue>("copper", copper));
        json.Add(new KeyValuePair<string, JsonValue>("plastanium", plastanium));
        json.Add(new KeyValuePair<string, JsonValue>("phaseFabric", phaseFabric));
        json.Add(new KeyValuePair<string, JsonValue>("surgeAlloy", surgeAlloy));
        json.Add(new KeyValuePair<string, JsonValue>("beryllium", beryllium));
        json.Add(new KeyValuePair<string, JsonValue>("tungsten", tungsten));
        json.Add(new KeyValuePair<string, JsonValue>("oxide", oxide));
        json.Add(new KeyValuePair<string, JsonValue>("carbide", carbide));

        cfg.ItemsEmoticons = json.ToString();
        db.Update(cfg);
        await db.SaveChangesAsync();

        await FollowupAsync("Emotes updated", ephemeral: true);
    }
}