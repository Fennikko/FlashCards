using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Flashcards;


UserInput.GetUserInput();
//var sqlConnection = @"Server=localhost;Integrated Security=true;Database=master";
//using var connection = new SqlConnection(sqlConnection);

//var testQuery = "SELECT database_id FROM sys.databases Where name = 'FlashCards'";
//var testDatabaseExists = connection.Query(testQuery);

//if (!testDatabaseExists.Any())
//{
//    var databaseCreation = "CREATE DATABASE FlashCards ON PRIMARY " +
//                           "(NAME = FlashCards, " +
//                           @"FILENAME = 'C:\temp\FlashCards.mdf', " +
//                           "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
//                           "LOG ON (NAME = MyDatabase_Log, " +
//                           @"FILENAME = 'C:\temp\FlashCardsLog.ldf', " +
//                           "SIZE = 1MB, " +
//                           "MAXSIZE = 5MB, " +
//                           "FILEGROWTH = 10%)";
//    connection.Execute(databaseCreation);
//}

//var newSqlConnection = @"Server=localhost;Integrated Security=true;Database=FlashCards";
//using var newConnection = new SqlConnection(newSqlConnection);

//newConnection.Execute(
//    """
//    IF OBJECT_ID(N'stacks', N'U') IS NULL
//    CREATE TABLE stacks (
//         StackId int IDENTITY(1,1) PRIMARY KEY,
//         StackName VARCHAR(255) NOT NULL,
//         UNIQUE (StackName)
//         )
//    """);

//newConnection.Execute(
//    """
//    IF OBJECT_ID(N'flash_cards', N'U') IS NULL
//    CREATE TABLE flash_cards (
//         FlashcardId int IDENTITY(1,1) PRIMARY KEY,
//         CardFront TEXT,
//         CardBack TEXT,
//         StackId int NOT NULL,
//         CONSTRAINT FK_flash_cards_stacks FOREIGN KEY (StackId)
//         REFERENCES stacks (StackId)
//         ON DELETE CASCADE
//         )
//    """);

//newConnection.Execute(
//    """
//    IF OBJECT_ID(N'study_sessions', N'U') IS NULL
//    CREATE TABLE study_sessions (
//         StudyId int IDENTITY(1,1) PRIMARY KEY,
//         SessionDate DateTime,
//         SessionScore int,
//         StackId int NOT NULL,
//         CONSTRAINT FK_study_sessions_stacks FOREIGN KEY (StackId)
//         REFERENCES stacks (StackId)
//         ON DELETE CASCADE
//         )
//    """);

//var testInitial = ConfigurationManager.AppSettings.Get("initialConnectionString");
//var testFinal = ConfigurationManager.AppSettings.Get("connectionString");
//Console.WriteLine(testInitial);
//Console.WriteLine(testFinal);
//Console.ReadLine();