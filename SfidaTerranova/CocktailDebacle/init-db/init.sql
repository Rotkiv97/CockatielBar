IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='CocktailDb')
BEGIN
    CREATE DATABASE CocktailDb;
    PRINT 'Database CocktailDb created successfully';
END
GO

-- Aggiungi qui eventuali script di inizializzazione tabelle