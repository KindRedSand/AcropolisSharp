using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class StarMessage
{
    /// <summary>
    /// Discord message ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    /// <summary>
    /// Sender User ID
    /// </summary>
    public ulong UserID { get; set; }

    /// <summary>
    /// The guild ID where this message get stars
    /// </summary>
    public ulong GuildID { get; set; }

    /// <summary>
    /// ID related to message in Starboard
    /// </summary>
    public ulong? StarboardMessage { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    public string? Content { get; set; }

    public string? AttachmentUrl { get; set; }

    public int LastCount { get; set; } = 0;
    
    public bool OwnerReacted { get; set; } = false;
}