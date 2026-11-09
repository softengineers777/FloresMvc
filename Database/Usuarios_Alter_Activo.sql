-- Activo/Inactivo para Usuarios (y referencia para sincronizar con login.activo)
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).

ALTER TABLE "Usuarios"
    ADD COLUMN IF NOT EXISTS "Activo" BOOLEAN NOT NULL DEFAULT TRUE;

UPDATE "Usuarios" SET "Activo" = TRUE WHERE "Activo" IS NULL;
