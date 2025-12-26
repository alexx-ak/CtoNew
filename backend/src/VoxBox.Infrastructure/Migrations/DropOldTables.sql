-- Drop existing tables if they exist with integer-based schema
IF OBJECT_ID('Tenants', 'U') IS NOT NULL DROP TABLE Tenants;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO
