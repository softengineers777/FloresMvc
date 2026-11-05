-- =====================================================================
-- Remisiones (encabezado) y RemisionesDTO (detalle de insumos)
-- Ejecutar sobre la base de datos FloresMvc.
-- =====================================================================

CREATE TABLE IF NOT EXISTS "Remisiones" (
    "IdRemision"       serial        PRIMARY KEY,
    "NumeroRemision"   varchar(50)   NOT NULL,
    "Nombre"           varchar(50)   NOT NULL,
    "Apellido"         varchar(50)   NOT NULL,
    "Direccion"        varchar(255)  NOT NULL,
    "IdArea"           integer       NOT NULL,
    "IdFincas"         integer       NOT NULL,
    "fecha_Creacion"   timestamp without time zone DEFAULT now(),
    "PlacaVehiculo"    varchar(20)   NOT NULL,
    "DespachadoPor"    varchar(100),
    "TransportadoPor"  varchar(100),
    "RecibidoPor"      varchar(100),
    "PesoTotal"        double precision DEFAULT 0,
    "Observaciones"    varchar(255),
    "Owner"            varchar(30),
    "DateOwner"        timestamp without time zone DEFAULT now(),
    "LastModify"       varchar(30),
    "LastDate"         timestamp without time zone,
    CONSTRAINT "FK_Remisiones_Areas"
        FOREIGN KEY ("IdArea") REFERENCES areas (idarea),
    CONSTRAINT "FK_Remisiones_Fincas"
        FOREIGN KEY ("IdFincas") REFERENCES fincas (idfincas)
);

CREATE TABLE IF NOT EXISTS "RemisionesDTO" (
    "IdDetalle"     serial        PRIMARY KEY,
    "IdRemision"    integer       NOT NULL,
    "referencia"    varchar(100)  NOT NULL,
    "Cantidad"      double precision NOT NULL,
    "Descripcion"   varchar(255)  NOT NULL,
    "Owner"         varchar(30),
    "DateOwner"     timestamp without time zone DEFAULT now(),
    "LastModify"    varchar(30),
    "LastDate"      timestamp without time zone,
    CONSTRAINT "FK_RemisionesDTO_Remisiones"
        FOREIGN KEY ("IdRemision") REFERENCES "Remisiones" ("IdRemision")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Remisiones_IdArea"    ON "Remisiones" ("IdArea");
CREATE INDEX IF NOT EXISTS "IX_Remisiones_IdFincas"  ON "Remisiones" ("IdFincas");
CREATE INDEX IF NOT EXISTS "IX_RemisionesDTO_IdRemision" ON "RemisionesDTO" ("IdRemision");
