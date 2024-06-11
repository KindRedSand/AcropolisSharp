using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Database;

public class BotDatabase : DbContext
{
    public DbSet<ConfigModel> Config { get; set; }
    // public DbSet<MessageModel> Messages { get; set; }
    
    public DbSet<WarningModel> Warnings { get; set; }
    
    public DbSet<NoMediaModel> NoMedia { get; set; }

    private readonly bool _isContextSet;
    /// <summary>
    /// Create a context and connect to local DB
    /// </summary>
    public BotDatabase()
    {
        _isContextSet = false;
    }

    /// <summary>
    /// Create a context configured to connect to a specific DB
    /// Probably you should adjust migrations to match your DB provider before using it
    /// </summary>
    public BotDatabase(DbContextOptions<BotDatabase> options) : base(options)
    {
        _isContextSet = true;
    }


    readonly string ConnectionString = string.Concat("Data Source=base.db");
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!_isContextSet)
            optionsBuilder.UseSqlite(ConnectionString);
    }


    public async Task<ConfigModel> GetOrCreateConfig(ulong guildId)
    {
        var config = await Config.FirstOrDefaultAsync(x => x.Id == guildId);
        if (config == null)
        {
            config = new ConfigModel()
            {
                Id = guildId,
            };
            await Config.AddAsync(config);
            await SaveChangesAsync();
            
            //Return tracked entity
            return await Config.FirstAsync(x => x.Id == guildId);
        }

        return config;
    }
    
    public async Task<ConfigModel?> GetNonTrackedConfig(ulong guildId)
    {
        return await Config.AsNoTracking().FirstOrDefaultAsync(x => x.Id == guildId);
    }

    public async Task<IEnumerable<WarningModel>> GetUserWarnings(ulong guildId, ulong userId)
    {
        var warningsList = await Warnings
            .Where(x => x.GuildID == guildId && x.UserID == userId).ToListAsync();

        var expired = warningsList
            .Where(x => x.ExpireTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToArray();
        
        if (expired.Length > 0)
        {
            warningsList.RemoveAll(x => expired.Contains(x));
            Warnings.RemoveRange(expired);
        }
        
        return warningsList;
    }
    

    internal async Task ApplyMigrations()
    {
        if (!await Database.CanConnectAsync())
        {
            Console.Write("Seems like db doesn't exist or configured. Execute initial migration... ");
            await Database.ExecuteSqlRawAsync(_initialCommit);
            await SaveChangesAsync();
            Console.WriteLine("Done!");
        }
    }
    
    private const string _initialCommit = 
        """
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
            "ProductVersion" TEXT NOT NULL
        );
        
        BEGIN TRANSACTION;
        
        CREATE TABLE IF NOT EXISTS "Config" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Config" PRIMARY KEY,
            "LogChannel" INTEGER NULL,
            "SchematicChannel" INTEGER NULL,
            "MapChannel" INTEGER NULL,
            "NoMediaRoleId" INTEGER NULL,
            "BanThreshold" INTEGER NULL,
            "ItemsEmoticons" TEXT NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS "NoMedia" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_NoMedia" PRIMARY KEY AUTOINCREMENT,
            "UserID" INTEGER NOT NULL,
            "ModeratorId" INTEGER NOT NULL,
            "GuildID" INTEGER NOT NULL,
            "Summary" TEXT NOT NULL,
            "IssueTime" INTEGER NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS "Warnings" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Warnings" PRIMARY KEY AUTOINCREMENT,
            "UserID" INTEGER NOT NULL,
            "ModeratorId" INTEGER NOT NULL,
            "GuildID" INTEGER NOT NULL,
            "Summary" TEXT NOT NULL,
            "WarningUrl" TEXT NOT NULL,
            "IssueTime" INTEGER NOT NULL,
            "ExpireTime" INTEGER NOT NULL
        );
        
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20240610061803_Initial', '8.0.6');
        
        COMMIT;
        """;
}
