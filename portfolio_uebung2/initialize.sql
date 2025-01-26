-- initialize.sql
DROP TABLE IF EXISTS Wishes;

CREATE TABLE IF NOT EXISTS Wishes (
    Id INT AUTO_INCREMENT,
    Description VARCHAR(500) NOT NULL,
    FileName VARCHAR(100),
    Status ENUM('Formulated', 'InProgress', 'Delivering', 'UnderTree') NOT NULL,
    PRIMARY KEY (Id)
    );

INSERT INTO Wishes (Description, FileName, Status) VALUES ('I want a new bike', '', 'Formulated');