BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "WarningsNew"
(
    "Id"          INTEGER NOT NULL
        CONSTRAINT "PK_Warnings" PRIMARY KEY AUTOINCREMENT,
    "UserID"      INTEGER NOT NULL,
    "ModeratorId" INTEGER NOT NULL,
    "GuildID"     INTEGER NOT NULL,
    "Summary"     TEXT    NOT NULL,
    "IssueTime"   INTEGER NOT NULL,
    "ExpireTime"  INTEGER NOT NULL
);

INSERT INTO "WarningsNew" ("UserID", "Summary", "ModeratorId") SELECT "userid", "reason", "by" FROM "warnings";

DROP TABLE "warnings";

ALTER TABLE "WarningsNew" RENAME TO "Warnings";

COMMIT;