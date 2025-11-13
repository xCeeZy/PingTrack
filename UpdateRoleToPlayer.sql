-- Скрипт для обновления роли "Студент" на "Игрок" в базе данных
-- Выполнить этот скрипт в SQL Server Management Studio или через sqlcmd

USE RusakovPingTrack;
GO

-- Обновить название роли с "Студент" на "Игрок"
UPDATE Roles
SET Role_Name = N'Игрок'
WHERE Role_Name = N'Студент';
GO

-- Проверить результат
SELECT * FROM Roles;
GO
