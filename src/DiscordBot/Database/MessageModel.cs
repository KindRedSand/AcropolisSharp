using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class MessageModel
{
    /// <summary>
    ///     Discord message ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    /// <summary>
    ///     Sender User ID
    /// </summary>
    public ulong UserID { get; set; }

    /// <summary>
    ///     The guild ID where this message was send
    /// </summary>
    public ulong GuildID { get; set; }

    /// <summary>
    ///     The guild channel ID where this message was send
    /// </summary>
    public ulong ChannelID { get; set; }


    /// <summary>
    ///     Message text
    /// </summary>
    public string? Content { get; set; }

    public string[]? AttachmentsUrls { get; set; }

    public DateTime LogTime { get; set; }
}