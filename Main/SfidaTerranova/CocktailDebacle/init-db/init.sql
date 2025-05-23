IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CocktailDb')
BEGIN
    CREATE DATABASE CocktailDb;
    PRINT 'Database CocktailDb created successfully';
END
GO

USE CocktailDb;
GO

-- Crea la tabella Users se non esiste
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = 'Users'
)
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(100) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        AcceptCookies BIT NOT NULL,
        Language NVARCHAR (64) NOT NULL
    );
    PRINT 'Table Users created successfully';
END
GO

-- Inserisce un utente solo se non esiste
IF NOT EXISTS (
    SELECT * FROM Users WHERE UserName = 'admin'
)
BEGIN
    INSERT INTO Users (
        UserName, Name, LastName, Email, PasswordHash, AcceptCookies
    )
    VALUES (
        'admin',
        'Admin',
        'User',
        'admin@example.com',
        'AQAAAAEAACcQAAAAEFakeHashForNow==', -- Inserisci un hash reale qui
        1,
        'it',
    );
    PRINT 'Admin user inserted successfully';
END
GO
