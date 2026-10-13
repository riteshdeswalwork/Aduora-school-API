-- Reusable table type for passing a list of GUIDs into a stored procedure
-- (e.g. a data-scope's "allowed IDs" restriction) as a single round trip
-- instead of a huge IN (...) list or N parameters.
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'GuidListType')
BEGIN
    EXEC('CREATE TYPE dbo.GuidListType AS TABLE (Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY)');
END
