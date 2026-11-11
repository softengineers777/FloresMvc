-- Agrega la columna Firma a la tabla DeclaracionConflictoInteres (para firma del trabajador)
ALTER TABLE "DeclaracionConflictoInteres"
    ADD COLUMN IF NOT EXISTS "Firma" TEXT;
