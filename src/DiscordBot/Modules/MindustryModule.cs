using System.Text;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using DiscordBot.Database;
using Hjson;
using Playground.Mindustry;
using Playground.Mindustry.Blocks;
using SixLabors.ImageSharp;

namespace DiscordBot.Modules;

public class MindustryModule(BotDatabase db /*, DiscordSocketClient client*/, HttpClient http)
    : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Emoji cross = new("\u274c");
    private readonly Emoji tumbsDown = new("\ud83d\udc4e");
    private readonly Emoji tumbsUp = new("\ud83d\udc4d");

    [SlashCommand("schematic", "Опубликовать схему. Укажите только 1 аргумент!", runMode: RunMode.Async)]
    public async Task PostSchematic(string? base64 = null, Attachment? file = null)
    {
        await DeferAsync(true);

        var config = await db.GetNonTrackedConfig(Context.Guild.Id);
        if (config?.SchematicChannel == null)
        {
            await FollowupAsync("Schematic channel was not set. Please notify administrator about this!");
            return;
        }

        var emotes = JsonValue.Parse(config.ItemsEmoticons).Qo();
        MemoryStream? ms = null;
        try
        {
            if (file != null)
            {
                if (!file.Filename.EndsWith(".msch"))
                {
                    switch (Context.Interaction.UserLocale)
                    {
                        case "ru":
                            await FollowupAsync("Файл имеет неверный формат", ephemeral: true);
                            break;
                        default:
                            await FollowupAsync("File has invalid format", ephemeral: true);
                            break;
                    }

                    return;
                }

                ms = new MemoryStream();
                var resp = await http.GetAsync(file.Url);
                if (!resp.IsSuccessStatusCode) await FollowupAsync("Failed to download file");
                await resp.Content.CopyToAsync(ms);

                //ms = new MemoryStream(bytes);
            }
            else if (base64 != null)
            {
                ms = new MemoryStream(Convert.FromBase64String(base64));
            }
            else
            {
                switch (Context.Interaction.UserLocale)
                {
                    case "ru":
                        await FollowupAsync("Вам нужно либо прикрепить файл схемы, либо импортированую строку",
                            ephemeral: true);
                        break;
                    default:
                        await FollowupAsync("You forget to specify arguments", ephemeral: true);
                        break;
                }

                return;
            }

            ms.Seek(0, SeekOrigin.Begin);

            var scheme = Schematics.Read(ms, false);

            using var imgSource = SchematicDrawer.DrawSchemePreview(scheme);
            using var imgMs = new MemoryStream();
            await imgSource.SaveAsPngAsync(imgMs);
            ms.Seek(0, SeekOrigin.Begin);
            imgMs.Seek(0, SeekOrigin.Begin);

            var channel = Context.Guild.GetTextChannel(config.SchematicChannel.Value);
            var attachments = new FileAttachment[2];
            attachments[0] = new FileAttachment(imgMs, "preview.png");
            attachments[1] = new FileAttachment(ms, $"{scheme.tags["name"]}.msch");

            var prices = new Dictionary<string, int>();
            foreach (var tile in scheme.tiles)
            {
                var block = Blocks.blocks[tile.block.name];
                foreach (var itemPrice in block.Price)
                    if (prices.ContainsKey(itemPrice.name))
                        prices[itemPrice.name] += itemPrice.count;
                    else
                        prices.Add(itemPrice.name, itemPrice.count);
            }

            var emb = new EmbedBuilder();
            emb.WithAuthor(Context.User)
                .WithTitle(scheme.tags["name"])
                .WithColor(ConfigModule.EmbedColor)
                .WithDescription(formatPrice(prices, emotes))
                .WithFooter($"{scheme.width}x{scheme.height} | UserID: {Context.User.Id}")
                .WithImageUrl("attachment://preview.png");

            await addReactions(await channel.SendFilesAsync(attachments, embed: emb.Build()));

            emb = new EmbedBuilder();
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    emb.WithTitle($"Схема была опубликована в <#{config.SchematicChannel}>");
                    await FollowupAsync(embed: emb.Build(), ephemeral: true);
                    break;
                default:
                    emb.WithTitle($"Your schematic published in <#{config.SchematicChannel}> chanel");
                    await FollowupAsync(embed: emb.Build(), ephemeral: true);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    await FollowupAsync("Во время обработки схемы произошла ошибка", ephemeral: true);
                    break;
                default:
                    await FollowupAsync("Exception was risen while processing your schematic", ephemeral: true);
                    break;
            }
        }
        finally
        {
            if (ms != null)
                await ms.DisposeAsync();
        }
    }


    [SlashCommand("map", "Опубликовать карту", runMode: RunMode.Async)]
    public async Task PostMap(Attachment file)
    {
        await DeferAsync(true);

        var config = await db.GetNonTrackedConfig(Context.Guild.Id);
        if (config?.MapChannel == null)
        {
            await FollowupAsync("Map channel was not set. Please notify administrator about this!");
            return;
        }

        MemoryStream? ms = null;
        try
        {
            // if (file == null)
            // {
            //     switch (Context.Interaction.UserLocale)
            //     {
            //         case "ru":
            //             await FollowupAsync("Вам нужно прикрепить файл карты!",
            //                 ephemeral: true);
            //             break;
            //         default:
            //             await FollowupAsync("You forget to specify arguments", ephemeral: true);
            //             break;
            //     }
            //
            //     return;
            // }

            if (!file.Filename.EndsWith(".msav"))
            {
                switch (Context.Interaction.UserLocale)
                {
                    case "ru":
                        await FollowupAsync("Файл имеет неверный формат", ephemeral: true);
                        break;
                    default:
                        await FollowupAsync("File has invalid format", ephemeral: true);
                        break;
                }

                return;
            }

            ms = new MemoryStream();
            var resp = await http.GetAsync(file.Url);
            if (!resp.IsSuccessStatusCode) await FollowupAsync("Failed to download file");
            await resp.Content.CopyToAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var ctx = SaveIO.load(ms, false);
            if (ctx == null) throw new InvalidDataException("SaveIO returned null");
            //var scheme = Map.Read(ms, closeAfterRead:false);

            using var imgSource = ctx.GetMapImage(); //= SchematicDrawer.DrawSchemePreview(scheme);
            using var imgMs = new MemoryStream();
            await imgSource.SaveAsPngAsync(imgMs);
            ms.Seek(0, SeekOrigin.Begin);
            imgMs.Seek(0, SeekOrigin.Begin);

            var channel = Context.Guild.GetTextChannel(config.MapChannel.Value);
            var attachments = new FileAttachment[2];
            attachments[0] = new FileAttachment(imgMs, "preview.png");
            attachments[1] = new FileAttachment(ms, $"{ctx.Meta.mapName}.msav");


            var emb = new EmbedBuilder();
            emb.WithAuthor(Context.User)
                .WithTitle(ctx.Meta.mapName)
                .WithColor(ConfigModule.EmbedColor)
                .WithImageUrl("attachment://preview.png")
                .WithFooter($"{ctx.Size.X}x{ctx.Size.Y} | UserID: {Context.User.Id}");
            if (ctx.Meta.tags.TryGetValue("description", out string? tag))
                emb.WithDescription(tag);

            await addReactions(await channel.SendFilesAsync(attachments, embed: emb.Build()));

            emb = new EmbedBuilder();
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    emb.WithTitle($"Карта была опубликована в <#{config.MapChannel}>");
                    await FollowupAsync(embed: emb.Build(), ephemeral: true);
                    break;
                default:
                    emb.WithTitle($"Your map published in <#{config.MapChannel}> chanel");
                    await FollowupAsync(embed: emb.Build(), ephemeral: true);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            switch (Context.Interaction.UserLocale)
            {
                case "ru":
                    await FollowupAsync("Во время обработки карты произошла ошибка", ephemeral: true);
                    break;
                default:
                    await FollowupAsync("Exception was risen while processing your map", ephemeral: true);
                    break;
            }
        }
        finally
        {
            if (ms != null)
                await ms.DisposeAsync();
        }
    }

    private async Task addReactions(RestMessage msg)
    {
        await msg.AddReactionAsync(tumbsUp);
        await msg.AddReactionAsync(tumbsDown);
        await msg.AddReactionAsync(cross);
    }

    private string formatPrice(Dictionary<string, int> dictionary, JsonObject emotes)
    {
        var sb = new StringBuilder();
        sb.Append("Необходимые ресурсы:\n");
        var i = 0;
        foreach ((string name, int count) in dictionary)
        {
            if (emotes.ContainsKey(name)) sb.Append(emotes.Qstr(name)).Append(' ');
            switch (name)
            {
                case "copper":
                    sb.Append("Медь");
                    break;
                case "lead":
                    sb.Append("Свинец");
                    break;
                case "metaglass":
                    sb.Append("Метастекло");
                    break;
                case "graphite":
                    sb.Append("Графит");
                    break;
                case "sand":
                    sb.Append("Песок");
                    break;
                case "coal":
                    sb.Append("Уголь"); // ???
                    break;
                case "titanium":
                    sb.Append("Титан");
                    break;
                case "thorium":
                    sb.Append("Торий");
                    break;
                case "scrap":
                    sb.Append("Металлолом");
                    break;
                case "silicon":
                    sb.Append("Кремний");
                    break;
                case "plastanium":
                    sb.Append("Пластан");
                    break;
                case "phase-fabric":
                case "phaseFabric":
                    sb.Append("Фазовая ткань");
                    break;
                case "surge-alloy":
                case "surgeAlloy":
                    sb.Append("Кинетический сплав");
                    break;
                case "spore-pod":
                case "sporePod":
                    sb.Append("Споровой стручок"); // ???
                    break;
                case "blast-compound":
                case "blastCompound":
                    sb.Append("Взрывчатая смесь"); // ???
                    break;
                case "pyratite":
                    sb.Append("Пиратит");
                    break;
                case "beryllium":
                    sb.Append("Бериллий");
                    break;
                case "tungsten":
                    sb.Append("Вольфрам");
                    break;
                case "oxide":
                    sb.Append("Оксид");
                    break;
                case "carbide":
                    sb.Append("Карбид");
                    break;
                case "fissile-matter":
                case "fissileMatter":
                    sb.Append("fissile-matter"); // ???
                    break;
                case "dormant-cyst":
                case "dormantCyst":
                    sb.Append("dormant-cyst"); // ???
                    break;
                default:
                    sb.Append("oh-no");
                    break;
            }

            sb.Append(": ");
            sb.Append(count);
            i++;
            if (i != dictionary.Count)
                sb.Append(", ");
        }

        return sb.ToString();
    }
}