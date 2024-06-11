using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class WarningModel
{
    [NotMapped] private long _issueTime;

    /// <summary>
    ///     Warning ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    /// <summary>
    ///     Warned user ID
    /// </summary>
    public ulong UserID { get; set; }

    /// <summary>
    ///     Mod who warned user
    /// </summary>
    public ulong ModeratorId { get; set; }

    /// <summary>
    ///     The guild ID where this warning created
    /// </summary>
    public ulong GuildID { get; set; }

    /// <summary>
    ///     Reason for warning
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    ///     Link to warning message
    /// </summary>
    public string WarningUrl { get; set; }

    /// <summary>
    ///     Date of issue
    /// </summary>
    public long IssueTime
    {
        get => _issueTime;
        set
        {
            _issueTime = value;
            ExpireTime = _issueTime + 30 * 24 * 60 * 60;
        }
    }

    /// <summary>
    ///     Warning expire time
    /// </summary>
    // [NotMapped]//We can calculate this in place
    public long ExpireTime { get; set; }
}