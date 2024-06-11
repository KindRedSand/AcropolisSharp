using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class NoMediaModel
{
    /// <summary>
    /// Warning ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    /// <summary>
    /// Warned user ID
    /// </summary>
    public ulong UserID { get; set; }
    
    /// <summary>
    /// Mod who warned user
    /// </summary>
    public ulong ModeratorId { get; set; }

    /// <summary>
    /// The guild ID where this warning created
    /// </summary>
    public ulong GuildID { get; set; }
    
    /// <summary>
    /// Reason for warning
    /// </summary>
    public string Summary { get; set; }
    
    /// <summary>
    /// Reason for warning
    /// </summary>
    public long IssueTime { get; set; }
}