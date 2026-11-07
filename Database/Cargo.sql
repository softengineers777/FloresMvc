-- =====================================================================
-- Cargo
-- Ejecutar sobre la base de datos FloresMvc.
-- =====================================================================

CREATE TABLE IF NOT EXISTS "Cargo" (
    "IdCargo"        serial        PRIMARY KEY,
    "NomCargo"       varchar(50)   NOT NULL,
    "fecha_Creacion" timestamp without time zone DEFAULT now(),
    "Owner"          varchar(30),
    "DateOwner"      timestamp without time zone DEFAULT now(),
    "LastModify"     varchar(30),
    "LastDate"       timestamp without time zone
);
