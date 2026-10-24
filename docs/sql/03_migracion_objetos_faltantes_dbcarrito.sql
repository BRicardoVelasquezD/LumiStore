/* Organiza sección */
SET NOCOUNT ON;
IF DB_NAME() <> N'DBCARRITO'
BEGIN
    RAISERROR('Base inesperada. Se cancela el script sin cambios.',16,1);
    RETURN;
END;

PRINT 'Sin cambios aplicados. Revise los resultados del diagnóstico y obtenga respaldo antes de preparar DDL.';
PRINT 'No ejecute una migración productiva hasta validar esquema, datos, nulabilidad y dependencias en un entorno aislado.';
