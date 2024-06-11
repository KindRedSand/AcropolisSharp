﻿using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Database;

public class ConfigModel
{
    /// <summary>
    ///     Guild ID
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    /// <summary>
    ///     Log channel
    /// </summary>
    public ulong? LogChannel { get; set; }

    /// <summary>
    ///     Channel where to post schematics
    /// </summary>
    public ulong? SchematicChannel { get; set; }

    /// <summary>
    ///     Channel where to post maps
    /// </summary>
    public ulong? MapChannel { get; set; }

    /// <summary>
    ///     No media role ID
    /// </summary>
    public ulong? NoMediaRoleId { get; set; }

    /// <summary>
    ///     Warning ban threshold
    /// </summary>
    public ulong? BanThreshold { get; set; } = 4;

    /// <summary>
    ///     Items emoticons in json format
    /// </summary>
    public string ItemsEmoticons { get; set; } = "{}";
}