#if NO_EF
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;

namespace DiscordBot.Database;

public class SQLiteDbConnector(
    string connectionString = "Data Source=base.db; Mode=ReadWriteCreate; FailIfMissing=False")
{
    private SQLiteConnection _connection = null!;


    private const string _config_select = 
        """
        SELECT "c"."Id", "c"."BanThreshold", "c"."LogChannel", "c"."MapChannel", "c"."NoMediaRoleId", "c"."SchematicChannel"
        FROM "Config" AS "c"
        WHERE "c"."Id" = $id
        LIMIT 1;
        """;
    public async Task<ConfigModel> GetGuildConfig(ulong guildId)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = _config_select;
        cmd.Parameters.AddWithValue("$id", guildId);
        var read = await cmd.ExecuteReaderAsync();
        if (await read.ReadAsync())
        {
            return new ConfigModel()
            {
                Id = read.Get<ulong>("Id"),
                LogChannel = read.Get<ulong>("LogChannel"),
                MapChannel = read.Get<ulong>("MapChannel"),
                SchematicChannel = read.Get<ulong>("SchematicChannel"),
                NoMediaRoleId = read.Get<ulong>("NoMediaRoleId"),
                BanThreshold = read.Get<ulong>("BanThreshold"),
            };
        }
        return new ConfigModel()
        {
            Id = guildId,
        };
    }
    
    
    private const string _config_update = 
        """
        INSERT OR REPLACE INTO Config(Id, BanThreshold, LogChannel, MapChannel, NoMediaRoleId, SchematicChannel)
        VALUES ($id, $ban, $log, $map, $nomedia, $scheme);
        """;
    public async Task UpdateGuildConfig(ConfigModel cfg)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = _config_update;
        cmd.Parameters.AddWithValue("$id", cfg.Id);
        cmd.Parameters.AddWithValue("$ban", cfg.BanThreshold);
        cmd.Parameters.AddWithValue("$log", cfg.LogChannel);
        cmd.Parameters.AddWithValue("$map", cfg.MapChannel);
        cmd.Parameters.AddWithValue("$nomedia", cfg.NoMediaRoleId);
        cmd.Parameters.AddWithValue("$scheme", cfg.SchematicChannel);
        await cmd.ExecuteNonQueryAsync();
    }

    
    private const string _warns_select = 
        """
        SELECT "w"."Id", "w"."ExpireTime", "w"."GuildID", "w"."IssueTime", "w"."ModeratorId", "w"."Summary", "w"."UserID"
        FROM "Warnings" AS "w"
        WHERE "w"."GuildID" = $gid AND "w"."UserID" = $uid
        """;
    public async Task<IEnumerable<WarningModel>> GetWarnings(ulong guildId, ulong userid)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = _warns_select;
        cmd.Parameters.AddWithValue("$gid", guildId);
        cmd.Parameters.AddWithValue("$uid", userid);
        var read = await cmd.ExecuteReaderAsync();
        var list = new List<WarningModel>();
        while (await read.ReadAsync())
        {
            list.Add(new WarningModel()
            {
                Id = read.Get<ulong>("Id"),
                GuildID = read.Get<ulong>("GuildID"),
                UserID = read.Get<ulong>("UserID"),
                Summary = read.Get<string>("Summary") ?? string.Empty,
                IssueTime = read.Get<long>("IssueTime"),
                ExpireTime = read.Get<long>("ExpireTime"),
                ModeratorId = read.Get<ulong>("Id"),
            });
        }
        var expired = list
            .Where(x => x.ExpireTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToImmutableArray();
        if (expired.Length > 0)
        {
            list.RemoveAll(x => expired.Contains(x));
            await _connection.BeginTransactionAsync();
            //TODO: Delete expired warnings!
        }

        return list;
    }
    
    
    private const string _warn_add = 
        """
        INSERT OR REPLACE INTO Warnings(Id, ExpireTime, GuildID, IssueTime, ModeratorId, Summary, UserID)
        VALUES ($id, $exp, $gid, $str, $mid, $text, $uid);
        """;
    public async Task AddWarn(WarningModel cfg)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = _warn_add;
        cmd.Parameters.AddWithValue("$id", cfg.Id);
        cmd.Parameters.AddWithValue("$exp", cfg.ExpireTime);
        cmd.Parameters.AddWithValue("$gid", cfg.GuildID);
        cmd.Parameters.AddWithValue("$str", cfg.IssueTime);
        cmd.Parameters.AddWithValue("$mid", cfg.ModeratorId);
        cmd.Parameters.AddWithValue("$text", cfg.Summary);
        cmd.Parameters.AddWithValue("$uid", cfg.UserID);
        await cmd.ExecuteNonQueryAsync();
    }
    
    
    private const string _warn_remove = 
        """
        DELETE FROM Warnings
        WHERE "Warnings"."Id" = $id;
        """;
    public async Task RemoveWarn(WarningModel cfg)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = _warn_remove;
        cmd.Parameters.AddWithValue("$id", cfg.Id);
        await cmd.ExecuteNonQueryAsync();
    }
    
    
    public async Task OpenConnection()
    {
        try
        {
            _connection = new SQLiteConnection(connectionString);
            await _connection.OpenAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task CloseConnection()
    {
        try
        {
            await _connection.CloseAsync();
            _connection = null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task PerformMigration()
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = _initialCommit;
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
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
            "BanThreshold" INTEGER NULL
        );

        CREATE TABLE IF NOT EXISTS "Messages" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Messages" PRIMARY KEY,
            "UserID" INTEGER NOT NULL,
            "GuildID" INTEGER NOT NULL,
            "ChannelID" INTEGER NOT NULL,
            "Content" TEXT NULL,
            "AttachmentsUrls" TEXT NULL,
            "LogTime" TEXT NOT NULL
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
            "IssueTime" INTEGER NOT NULL,
            "ExpireTime" INTEGER NOT NULL
        );

        COMMIT;
        """;
    //INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    //VALUES ('20240610061803_Initial', '8.0.6');
}

public static class SQLiteExtensions
{ 
    public static T? Get<T>(this DbDataReader reader, string fieldName)
    {
        var obj = reader.GetValue(fieldName);
        if (obj is DBNull)
            return default;
        return (T) Convert.ChangeType(reader.GetValue(fieldName), typeof(T), CultureInfo.InvariantCulture);
    }
}
#endif