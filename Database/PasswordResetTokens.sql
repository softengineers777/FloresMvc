-- =====================================================================
-- PasswordResetTokens: tokens para recuperación de contraseña (1 uso, con vencimiento)
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- =====================================================================

CREATE TABLE IF NOT EXISTS "PasswordResetTokens" (
    "IdToken"      SERIAL PRIMARY KEY,
    "UserId"       integer NOT NULL,
    "TokenHash"    VARCHAR(128) NOT NULL,
    "ExpiresAt"    TIMESTAMP NOT NULL,
    "UsedAt"       TIMESTAMP,
    "CreatedAt"    TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_passwordresettokens_tokenhash
    ON "PasswordResetTokens" ("TokenHash");

CREATE INDEX IF NOT EXISTS ix_passwordresettokens_userid
    ON "PasswordResetTokens" ("UserId");
