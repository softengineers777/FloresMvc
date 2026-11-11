-- 1. Tabla Catálogo de Situaciones / Preguntas de Conflicto de Interés
CREATE TABLE IF NOT EXISTS "PreguntasConflictoInteres" (
    "IdPregunta" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(50) NOT NULL UNIQUE,  -- Ej: 'REL_LABORAL', 'VINCULO_FAMILIAR', 'OBSEQUIOS'
    "Descripcion" TEXT NOT NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Inserción del catálogo base extraído del formato
INSERT INTO "PreguntasConflictoInteres" ("Codigo", "Descripcion") VALUES
('REL_LABORAL', 'Relación laboral directa o indirecta con una contraparte de la empresa.'),
('VINCULO_CONTRAPARTE', 'Relación o vínculo de consanguinidad o afinidad con una contraparte de la empresa.'),
('VINCULO_COLABORADOR', 'Relación o vínculo de consanguinidad o afinidad con un colaborador de la empresa.'),
('PARTICIPACION_ACTIVOS', 'Participación en transacciones, activos o inversiones con una contraparte de la empresa.'),
('PARTICIPACION_SELECCION', 'Participación en procesos de selección o contratación de contrapartes con los que se tenga vínculos de consanguinidad y afinidad.'),
('MANEJO_OBSEQUIOS', 'Manejo de obsequios, regalos o atenciones que se puedan otorgar o recibir con terceros que tengan relación con la empresa.'),
('SIN_CONFLICTO', 'En caso de no presentar ninguna situación que pueda generar conflicto de interés, marque esta casilla.')
ON CONFLICT ("Codigo") DO NOTHING;


-- 2. Tabla Cabecera: DeclaracionConflictoInteres
CREATE TABLE IF NOT EXISTS "DeclaracionConflictoInteres" (
    "IdDeclaracion" SERIAL PRIMARY KEY,
    "IdUsuario" INT NOT NULL,
    "TipoDoc" VARCHAR(10) DEFAULT 'CC',
    "NumeroDocumento" VARCHAR(50) NOT NULL,
    "IdCargo" INT,
    "IdArea" INT,
    "FechaDeclaracion" DATE NOT NULL DEFAULT CURRENT_DATE,

    -- Datos opcionales del familiar en caso de conflicto
    "NombreFamiliar" VARCHAR(150),
    "CargoFamiliar" VARCHAR(150),
    "AreaFamiliar" VARCHAR(150),
    "Observaciones" TEXT,
    "Firma" TEXT,

    -- Auditoría
    "Owner" VARCHAR(30),
    "DateOwner" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastModify" VARCHAR(30),
    "LastDate" TIMESTAMP,

    -- Llaves foráneas
    CONSTRAINT "FK_DeclaracionConflicto_Usuarios" FOREIGN KEY ("IdUsuario")
        REFERENCES "Usuarios"("IdUsuarios") ON DELETE CASCADE,
    CONSTRAINT "FK_DeclaracionConflicto_Cargo" FOREIGN KEY ("IdCargo")
        REFERENCES "Cargo"("IdCargo") ON DELETE SET NULL,
    CONSTRAINT "FK_DeclaracionConflicto_Areas" FOREIGN KEY ("IdArea")
        REFERENCES "areas"("idarea") ON DELETE SET NULL
);


-- 3. Tabla Detalle / Respuestas: DeclaracionConflictoInteresPreguntas
CREATE TABLE IF NOT EXISTS "DeclaracionConflictoInteresPreguntas" (
    "IdDeclaracionPregunta" SERIAL PRIMARY KEY,
    "IdDeclaracion" INT NOT NULL,
    "IdPregunta" INT NOT NULL,
    "Aplica" BOOLEAN NOT NULL DEFAULT FALSE,

    -- Llaves foráneas atadas a la Declaración y al Catálogo
    CONSTRAINT "FK_DetalleConflicto_Declaracion" FOREIGN KEY ("IdDeclaracion")
        REFERENCES "DeclaracionConflictoInteres"("IdDeclaracion") ON DELETE CASCADE,
    CONSTRAINT "FK_DetalleConflicto_Pregunta" FOREIGN KEY ("IdPregunta")
        REFERENCES "PreguntasConflictoInteres"("IdPregunta") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_DeclaracionConflictoInteres_IdUsuario" ON "DeclaracionConflictoInteres"("IdUsuario");
CREATE INDEX IF NOT EXISTS "IX_DeclaracionConflictoInteresPreguntas_IdDeclaracion" ON "DeclaracionConflictoInteresPreguntas"("IdDeclaracion");
CREATE INDEX IF NOT EXISTS "IX_DeclaracionConflictoInteresPreguntas_IdPregunta" ON "DeclaracionConflictoInteresPreguntas"("IdPregunta");
