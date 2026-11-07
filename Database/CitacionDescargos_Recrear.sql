-- =====================================================================
-- RECREAR CitacionDescargos con la estructura objetivo.
-- ADVERTENCIA: elimina la tabla y sus datos actuales.
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- =====================================================================

DROP TABLE IF EXISTS "CitacionDescargos";

CREATE TABLE "CitacionDescargos" (
    "IdCitacion"       SERIAL PRIMARY KEY,
    "Ciudad"           VARCHAR(50),
    "fecha_Creacion"   TIMESTAMP NOT NULL,
    "NomTrabajador"    VARCHAR(20) NOT NULL,
    "IdCargo"          integer NOT NULL,
    "IdArea"           integer NOT NULL,
    "Proceso"          VARCHAR(20) NOT NULL,
    "fecha_Diligencia" TIMESTAMP,
    "HoraDiligencia"   TIMESTAMP,
    "FechaHechos"      TIMESTAMP,
    "CcRecibido"       VARCHAR(20) NOT NULL,
    "Owner"            VARCHAR(30),
    "DateOwner"        TIMESTAMP,
    "LastModify"       VARCHAR(30),
    "LastDate"         TIMESTAMP
);
