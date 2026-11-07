-- =====================================================================
-- ModeloDisciplinario (Manejo Disciplinario / Gestión Humana)
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- =====================================================================

CREATE TABLE IF NOT EXISTS "ModeloDisciplinario" (
    "IdModelo"        serial        PRIMARY KEY,
    "IdArea"          integer       NOT NULL,
    "fecha_creacion"  timestamp without time zone NOT NULL,
    "para"            varchar(50)   NOT NULL,
    "asunto"          varchar(50)   NOT NULL,
    "NomTrabajador"   varchar(50)   NOT NULL,
    "Cargo"           integer       NOT NULL,
    "Proceso"         varchar(50)   NOT NULL,
    "Situacion"       varchar(255)  NOT NULL,
    "Firma"           varchar(50)   NOT NULL,
    "Owner"           varchar(30),
    "DateOwner"       timestamp without time zone,
    "LastModify"      varchar(30),
    "LastDate"        timestamp without time zone
);

-- Opcional: llaves foráneas si aplican en tu esquema
-- ALTER TABLE "ModeloDisciplinario"
--     ADD CONSTRAINT fk_modelodisciplinario_area FOREIGN KEY ("IdArea") REFERENCES "Areas"("IdArea");
-- ALTER TABLE "ModeloDisciplinario"
--     ADD CONSTRAINT fk_modelodisciplinario_cargo FOREIGN KEY ("Cargo") REFERENCES "Cargo"("IdCargo");
