IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='CocktailDb')
BEGIN
    CREATE DATABASE CocktailDb;
    PRINT 'Database CocktailDb created successfully';
END
GO

-- Aggiungi qui eventuali script di inizializzazione tabelle

-- IF NOT EXISTS(SELECT * FROM sys.sql_logins WHERE name = 'efcore_user')
-- BEGIN
--     CREATE LOGIN efcore_user WITH PASSWORD = 'EfCore!Test123';
--     PRINT 'Login efcore_user created successfully';
-- END
-- GO

-- USE CocktailDb;
-- GO

-- -- Crea l'utente nel database e assegna permessi
-- IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name = 'efcore_user')
-- BEGIN
--     CREATE USER efcore_user FOR LOGIN efcore_user;
--     EXEC sp_addrolemember 'db_owner', 'efcore_user';
--     PRINT 'User efcore_user created in CocktailDb with db_owner role';
-- END
-- GO