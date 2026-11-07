-- =====================================================================
-- ALTER TABLE: alinear "CitacionDescargos" a la estructura objetivo.
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- Seguro de re-ejecutar en varios pasos (IF EXISTS / IF NOT EXISTS).
-- Revisa los datos antes de aplicar NOT NULL donde se indica.
-- =====================================================================

-- 1) Renombrar columnas antiguas (si existen)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='CiudadFecha')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='Ciudad')
    THEN ALTER TABLE "CitacionDescargos" RENAME COLUMN "CiudadFecha" TO "Ciudad"; END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='NombreTrabajador')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='NomTrabajador')
    THEN ALTER TABLE "CitacionDescargos" RENAME COLUMN "NombreTrabajador" TO "NomTrabajador"; END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='FechaDiligencia')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='fecha_Diligencia')
    THEN ALTER TABLE "CitacionDescargos" RENAME COLUMN "FechaDiligencia" TO "fecha_Diligencia"; END IF;

    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='CcRecibio')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='CcRecibido')
    THEN ALTER TABLE "CitacionDescargos" RENAME COLUMN "CcRecibio" TO "CcRecibido"; END IF;
END $$;

-- 2) Agregar columnas nuevas (si faltan)
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "Ciudad"           VARCHAR(50);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "fecha_Creacion"   TIMESTAMP;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "NomTrabajador"    VARCHAR(20);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "IdCargo"          integer;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "IdArea"           integer;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "Proceso"          VARCHAR(20);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "fecha_Diligencia" TIMESTAMP;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "HoraDiligencia"   TIMESTAMP;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "FechaHechos"      TIMESTAMP;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "CcRecibido"       VARCHAR(20);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "Owner"            VARCHAR(30);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "DateOwner"        TIMESTAMP;
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "LastModify"       VARCHAR(30);
ALTER TABLE "CitacionDescargos" ADD COLUMN IF NOT EXISTS "LastDate"         TIMESTAMP;

-- 3) Migrar datos desde columnas antiguas (si aún existen)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='Cargo') THEN
        UPDATE "CitacionDescargos" cd
        SET "IdCargo" = c."IdCargo"
        FROM "Cargo" c
        WHERE cd."Cargo" = c."NomCargo" AND cd."IdCargo" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='CitacionDescargos' AND column_name='AreaProceso') THEN
        -- "AreaProceso" era texto; no hay mapeo automático a IdArea.
        -- Ajusta manualmente: UPDATE "CitacionDescargos" SET "IdArea" = ... WHERE "IdArea" IS NULL;
        NULL;
    END IF;
END $$;

-- 4) Eliminar columnas obsoletas (si existen)
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "Cargo";
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "AreaProceso";
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "CiudadFecha";
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "NombreTrabajador";
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "FechaDiligencia";
ALTER TABLE "CitacionDescargos" DROP COLUMN IF EXISTS "CcRecibio";

-- 5) Ajustar tipos/tamaños
ALTER TABLE "CitacionDescargos" ALTER COLUMN "Ciudad"           TYPE VARCHAR(50);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "NomTrabajador"    TYPE VARCHAR(20);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "Proceso"          TYPE VARCHAR(20);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "CcRecibido"       TYPE VARCHAR(20);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "Owner"            TYPE VARCHAR(30);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "LastModify"       TYPE VARCHAR(30);
ALTER TABLE "CitacionDescargos" ALTER COLUMN "fecha_Creacion"   TYPE TIMESTAMP USING "fecha_Creacion"::timestamp;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "fecha_Diligencia" TYPE TIMESTAMP USING "fecha_Diligencia"::timestamp;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "HoraDiligencia"   TYPE TIMESTAMP USING "HoraDiligencia"::timestamp;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "FechaHechos"      TYPE TIMESTAMP USING "FechaHechos"::timestamp;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "DateOwner"        TYPE TIMESTAMP USING "DateOwner"::timestamp;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "LastDate"         TYPE TIMESTAMP USING "LastDate"::timestamp;

-- 6) Valores por defecto / backfill para NOT NULL (ajusta según tus datos)
UPDATE "CitacionDescargos" SET "fecha_Creacion" = COALESCE("fecha_Creacion", "DateOwner", NOW()) WHERE "fecha_Creacion" IS NULL;
UPDATE "CitacionDescargos" SET "NomTrabajador" = COALESCE("NomTrabajador", '') WHERE "NomTrabajador" IS NULL;
UPDATE "CitacionDescargos" SET "Proceso" = COALESCE("Proceso", '') WHERE "Proceso" IS NULL;
UPDATE "CitacionDescargos" SET "CcRecibido" = COALESCE("CcRecibido", '') WHERE "CcRecibido" IS NULL;
-- Si IdCargo/IdArea quedaron NULL por falta de mapeo, asígnalos antes de NOT NULL:
-- SELECT "IdCitacion","NomTrabajador","IdCargo","IdArea" FROM "CitacionDescargos" WHERE "IdCargo" IS NULL OR "IdArea" IS NULL;

-- 7) Restricciones NOT NULL objetivo (ejecuta cuando los datos estén completos)
ALTER TABLE "CitacionDescargos" ALTER COLUMN "fecha_Creacion" SET NOT NULL;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "NomTrabajador"  SET NOT NULL;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "IdCargo"        SET NOT NULL;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "IdArea"         SET NOT NULL;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "Proceso"        SET NOT NULL;
ALTER TABLE "CitacionDescargos" ALTER COLUMN "CcRecibido"     SET NOT NULL;
