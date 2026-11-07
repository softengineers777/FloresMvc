-- =====================================================================
-- Migrar Cargo (varchar) -> IdCargo (integer, referencia a Cargo.IdCargo)
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- NOTA: si la tabla ya tiene datos, el paso de conversión intenta
-- emparejar por nombre (NomCargo). Revisa los NULL antes del NOT NULL.
-- =====================================================================

-- 1) Agregar columna nueva
ALTER TABLE "CitacionDescargos"
    ADD COLUMN IF NOT EXISTS "IdCargo" integer;

-- 2) (Opcional, solo si había datos con nombres en "Cargo")
--    Intenta mapear el texto al IdCargo correspondiente.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'CitacionDescargos' AND column_name = 'Cargo'
    ) THEN
        UPDATE "CitacionDescargos" cd
        SET "IdCargo" = c."IdCargo"
        FROM "Cargo" c
        WHERE cd."Cargo" = c."NomCargo"
          AND cd."IdCargo" IS NULL;
    END IF;
END $$;

-- 3) Eliminar la columna anterior (varchar)
ALTER TABLE "CitacionDescargos"
    DROP COLUMN IF EXISTS "Cargo";

-- 4) Exigir valor (ajusta si hay filas sin mapear)
--    Si tienes registros con IdCargo NULL, asígnalos antes de ejecutar esto.
ALTER TABLE "CitacionDescargos"
    ALTER COLUMN "IdCargo" SET NOT NULL;

-- 5) (Opcional) Llave foránea
-- ALTER TABLE "CitacionDescargos"
--     ADD CONSTRAINT fk_citaciondescargos_cargo
--     FOREIGN KEY ("IdCargo") REFERENCES "Cargo"("IdCargo");
