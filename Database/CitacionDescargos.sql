-- =====================================================================
-- CitacionDescargos (Citación a Descargos / Gestión Humana)
-- Estructura objetivo. Ejecutar sobre la base de datos FloresMvc.
-- Si la tabla anterior quedó mal creada, usa CitacionDescargos_Recrear.sql
-- o el ALTER en CitacionDescargos_Alter_Estructura.sql.
-- =====================================================================

CREATE TABLE IF NOT EXISTS "CitacionDescargos" (
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

    -- Opcional:
    -- ,CONSTRAINT fk_citaciondescargos_cargo FOREIGN KEY ("IdCargo") REFERENCES "Cargo"("IdCargo")
    -- ,CONSTRAINT fk_citaciondescargos_area  FOREIGN KEY ("IdArea")  REFERENCES "areas"("idarea")
);
