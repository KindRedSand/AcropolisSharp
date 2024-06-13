CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory"
(
    "MigrationId"    TEXT NOT NULL
        CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "Config"
(
    "Id"               INTEGER NOT NULL
        CONSTRAINT "PK_Config" PRIMARY KEY,
    "LogChannel"       INTEGER NULL,
    "SchematicChannel" INTEGER NULL,
    "MapChannel"       INTEGER NULL,
    "NoMediaRoleId"    INTEGER NULL,
    "BanThreshold"     INTEGER NULL,
    "ItemsEmoticons"   TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS "NoMedia"
(
    "Id"          INTEGER NOT NULL
        CONSTRAINT "PK_NoMedia" PRIMARY KEY AUTOINCREMENT,
    "UserID"      INTEGER NOT NULL,
    "ModeratorId" INTEGER NOT NULL,
    "GuildID"     INTEGER NOT NULL,
    "Summary"     TEXT    NOT NULL,
    "IssueTime"   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "Warnings"
(
    "Id"          INTEGER NOT NULL
        CONSTRAINT "PK_Warnings" PRIMARY KEY AUTOINCREMENT,
    "UserID"      INTEGER NOT NULL,
    "ModeratorId" INTEGER NOT NULL,
    "GuildID"     INTEGER NOT NULL,
    "Summary"     TEXT    NOT NULL,
    "WarningUrl"  TEXT    NOT NULL,
    "IssueTime"   INTEGER NOT NULL,
    "ExpireTime"  INTEGER NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240610061803_Initial', '8.0.6');

COMMIT;