-- =====================================================================
-- Ajuste: permitir almacenar firma capturada (imagen/base64) en "Firma"
-- Ejecutar sobre la base de datos FloresMvc si la tabla ya fue creada
-- con Firma varchar(50).
-- =====================================================================

ALTER TABLE "ModeloDisciplinario"
    ALTER COLUMN "Firma" TYPE text;
