using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class StarConfig
{
    /// <summary>
    /// Guild ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    /// <summary>
    /// Starboard channel
    /// </summary>
    public ulong? ChannelID { get; set; }

    /// <summary>
    /// This many stars required for message to be added
    /// </summary>
    public int ReactionsThreshold { get; set; }
}