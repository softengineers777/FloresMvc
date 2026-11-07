-- =====================================================================
-- ActaDescargos (Gestión Humana) + ActaDescargosPreguntas (detalle)
-- Ejecutar sobre la base de datos FloresMvc (PostgreSQL).
-- =====================================================================

CREATE TABLE IF NOT EXISTS "ActaDescargos" (
    "IdActa"                   SERIAL PRIMARY KEY,
    "Ciudad"                   VARCHAR(100),
    "fecha_Creacion"           TIMESTAMP NOT NULL,
    "FechaInformeDisciplinario" DATE,
    "NombreTrabajador"         VARCHAR(150) NOT NULL,
    "CcTrabajador"             VARCHAR(50) NOT NULL,
    "DireccionResidencia"      VARCHAR(200),
    "Telefono"                 VARCHAR(50),
    "IdCargo"                  integer NOT NULL,
    "IdArea"                   integer NOT NULL,
    "Proceso"                  VARCHAR(100) NOT NULL,
    "FechaIngreso"             DATE,
    "AsistidoTestigos"         BOOLEAN,
    "NombreTestigo"            VARCHAR(150),
    "HoraInicio"               VARCHAR(20),
    "HoraFin"                  VARCHAR(20),
    "FechaDiligencia"          TIMESTAMP,
    "ConceptoGestionHumana"    TEXT,
    "DiasSancion"              integer,
    "FechaInicioSancion"       DATE,
    "Owner"                    VARCHAR(30),
    "DateOwner"                TIMESTAMP,
    "LastModify"               VARCHAR(30),
    "LastDate"                 TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "ActaDescargosPreguntas" (
    "IdPregunta"  SERIAL PRIMARY KEY,
    "IdActa"      integer NOT NULL,
    "Orden"       integer NOT NULL,
    "Pregunta"    TEXT NOT NULL,
    "Respuesta"   TEXT NOT NULL,
    "Owner"       VARCHAR(30),
    "DateOwner"   TIMESTAMP,
    "LastModify"  VARCHAR(30),
    "LastDate"    TIMESTAMP,
    CONSTRAINT fk_actadescargospreguntas_acta
        FOREIGN KEY ("IdActa") REFERENCES "ActaDescargos"("IdActa") ON DELETE CASCADE
);
