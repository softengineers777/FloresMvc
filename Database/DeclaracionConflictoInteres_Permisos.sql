-- Permisos del módulo Declaración de Conflicto de Interés
INSERT INTO "Permisos" ("Clave", "Modulo", "Descripcion") VALUES
('DeclaracionConflictoInteres.Ver',       'DeclaracionConflictoInteres', 'Ver declaraciones de conflicto de interés'),
('DeclaracionConflictoInteres.Crear',     'DeclaracionConflictoInteres', 'Crear declaraciones de conflicto de interés'),
('DeclaracionConflictoInteres.Editar',    'DeclaracionConflictoInteres', 'Editar declaraciones de conflicto de interés'),
('DeclaracionConflictoInteres.Eliminar',  'DeclaracionConflictoInteres', 'Eliminar declaraciones de conflicto de interés'),
('DeclaracionConflictoInteres.Documento', 'DeclaracionConflictoInteres', 'Ver/imprimir documento de declaración de conflicto de interés')
ON CONFLICT ("Clave") DO NOTHING;
