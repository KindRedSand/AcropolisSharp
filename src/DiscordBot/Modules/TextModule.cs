using Discord;
using Discord.Interactions;
using Hjson;

namespace DiscordBot.Modules;

[CommandContextType(InteractionContextType.Guild)]
[RequireUserPermission(ChannelPermission.ManageChannels)]
public class TextModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string _exampleEmbedPayload =
        """
        ```hjson
        {
            "title": "Test"
            "description":
            '''
            Lorem ipsum dolor sit amet,
            consectetur adipiscing elit,
            sed do eiusmod tempor incididunt
            ut labore et dolore magna aliqua.
            Ut enim ad minim veniam,
            quis nostrud exercitation ullamco
            laboris nisi ut aliquip ex ea
            commodo consequat. Duis aute irure
            dolor in reprehenderit in voluptate
            velit esse cillum dolore eu fugiat
            nulla pariatur.
            '''
            "color": "10521600"
            "image": "https://media.discordapp.net/attachments/966951414830092288/1246974420606783588/image20240603-21216-2mo30o.png?ex=6668e2ed&is=6667916d&hm=73b79f4fe111f6698ee5ffab9e62a26cd92c458fd267cc7e1d2109880b175680&=&format=webp&quality=lossless&width=193&height=440"
            "thumbnail": "https://media.discordapp.net/attachments/966951414830092288/1246974420606783588/image20240603-21216-2mo30o.png?ex=6668e2ed&is=6667916d&hm=73b79f4fe111f6698ee5ffab9e62a26cd92c458fd267cc7e1d2109880b175680&=&format=webp&quality=lossless&width=193&height=440"
            "footer": {
                text: "made with hjson"
                "icon_url": "https://media.discordapp.net/attachments/966951369145745469/1248024978964746351/image20240605-21216-3vdt7s.png?ex=6668c0d6&is=66676f56&hm=0cb8ec89dd3bedd9a26ff5a09d3b475e98e670e6267a9c7fff52a44117e08d74&=&format=webp&quality=lossless&width=40&height=48"
                }
            "timestamp": "1718098283"
            "author": "872763849554677801"
            "fields": [
                {
                    "name": "test1"
                    "value": "value test 1"
                    "inline": "true"
                },
                {
                    "name": "test2"
                    "value": "value 2 test"
                    "inline": "true"
                },
                {
                    "name": "3test3"
                    "value": "3 value 2 test"
                    "inline": "false"
                }
            ]
        }
        ```
        """;

    [SlashCommand("say", "AI will capture the world", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task Say(string text)
    {
        await DeferAsync(true);
        await Context.Channel.SendMessageAsync(text);
        await FollowupAsync("Done");
    }

    [SlashCommand("embed", "Parse hjson and send embed message", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task Embed(string hjson, bool getExample = false)
    {
        if (getExample)
        {
            await RespondAsync(_exampleEmbedPayload, ephemeral: true);
            return;
        }

        await DeferAsync(true);
        try
        {
            var json = HjsonValue.Parse(hjson).Qo();
            var emb = new EmbedBuilder();

            if (json.ContainsKey("color"))
            {
                if (uint.TryParse(json.Qstr("color"), out uint color)) emb.WithColor(new Color(color));
            }
            else
            {
                emb.WithColor(ConfigModule.EmbedColor);
            }

            if (json.ContainsKey("timestamp"))
                if (long.TryParse(json.Qstr("timestamp"), out long time))
                    emb.WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(time));

            if (json.ContainsKey("author"))
                if (ulong.TryParse(json.Qstr("author"), out ulong uid))
                {
                    var user = Context.Guild.GetUser(uid);
                    if (user != null)
                        emb.WithAuthor(user);
                }

            if (json.ContainsKey("title"))
                emb.WithTitle(json.Qstr("title"));
            if (json.ContainsKey("description"))
                emb.WithDescription(json.Qstr("description"));
            if (json.ContainsKey("footer"))
            {
                var footer = json.Qo("footer");
                if (footer.ContainsKey("icon_url"))
                    emb.WithFooter(footer.Qstr("text"), footer.Qstr("icon_url"));
                else
                    emb.WithFooter(footer.Qstr("text"));
            }

            if (json.ContainsKey("url"))
                emb.WithFooter(json.Qstr("url"));

            if (json.ContainsKey("image"))
                emb.WithImageUrl(json.Qstr("image"));
            if (json.ContainsKey("thumbnail"))
                emb.WithThumbnailUrl(json.Qstr("thumbnail"));

            if (json.ContainsKey("fields"))
            {
                var arr = json.Qa("fields");
                var fields = new List<EmbedFieldBuilder>(arr.Count);
                foreach (var entr in arr)
                {
                    var item = entr.Qo();
                    var field = new EmbedFieldBuilder();
                    if (item.ContainsKey("name"))
                        field.WithName(item.Qstr("name"));
                    if (item.ContainsKey("value"))
                        field.WithValue(item.Qstr("value"));
                    if (item.ContainsKey("inline"))
                    {
                        string? strVal = item.Qstr("inline");
                        field.WithIsInline(strVal.ToLower() is "true" or "yes" or "1");
                    }

                    fields.Add(field);
                }

                emb.WithFields(fields);
            }

            await Context.Channel.SendMessageAsync(embed: emb.Build());
            await FollowupAsync("Готово");
        }
        catch (Exception e)
        {
            await FollowupAsync($"При обработке запроса произошла ошибка:\n{e.Message}");
        }
    }
}