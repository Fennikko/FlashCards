using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Spectre.Console;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Flashcards;

public class DatabaseController
{
    public static readonly string? InitialConnection = ConfigurationManager.AppSettings.Get("initialConnectionString");
    public static readonly string? ConnectionString = ConfigurationManager.AppSettings.Get("connectionString");

    public static void DatabaseCreation()
    {
        using var initialConnection = new SqlConnection(InitialConnection);

        var testQuery = "SELECT database_id FROM sys.databases Where name = 'FlashCards'";
        var testDatabaseExists = initialConnection.Query(testQuery);

        if (!testDatabaseExists.Any())
        {
            var databaseCreation = "CREATE DATABASE FlashCards ON PRIMARY " +
                                   "(NAME = FlashCards, " +
                                   @"FILENAME = 'C:\temp\FlashCards.mdf', " +
                                   "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
                                   "LOG ON (NAME = MyDatabase_Log, " +
                                   @"FILENAME = 'C:\temp\FlashCardsLog.ldf', " +
                                   "SIZE = 1MB, " +
                                   "MAXSIZE = 5MB, " +
                                   "FILEGROWTH = 10%)";
            initialConnection.Execute(databaseCreation);
        }

        using var connection = new SqlConnection(ConnectionString);

        connection.Execute(
            """
            IF OBJECT_ID(N'stacks', N'U') IS NULL
            CREATE TABLE stacks (
                 StackId int IDENTITY(1,1) PRIMARY KEY,
                 StackName VARCHAR(255) NOT NULL,
                 UNIQUE (StackName)
                 )
            """);

        connection.Execute(
            """
            IF OBJECT_ID(N'flash_cards', N'U') IS NULL
            CREATE TABLE flash_cards (
                 FlashcardId int IDENTITY(1,1) PRIMARY KEY,
                 FlashcardIndex int NOT NULL,
                 CardFront VARCHAR(255) NOT NULL,
                 CardBack VARCHAR(255) NOT NULL,
                 StackId int NOT NULL,
                 UNIQUE (CardFront),
                 CONSTRAINT FK_flash_cards_stacks FOREIGN KEY (StackId)
                 REFERENCES stacks (StackId)
                 ON DELETE CASCADE
                 )
            """);

        connection.Execute(
            """
            IF OBJECT_ID(N'study_sessions', N'U') IS NULL
            CREATE TABLE study_sessions (
                 StudyId int IDENTITY(1,1) PRIMARY KEY,
                 SessionDate DateTime NOT NULL,
                 SessionScore int NOT NULL,
                 StackId int NOT NULL,
                 CONSTRAINT FK_study_sessions_stacks FOREIGN KEY (StackId)
                 REFERENCES stacks (StackId)
                 ON DELETE CASCADE
                 )
            """);
    }

    public static int CreateStack()
    {
        AnsiConsole.Clear();
        var stackName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter a [green] stack name[/] or type 0 to return to the main menu: ")
                .PromptStyle("blue")
                .AllowEmpty());
        if (stackName == "0") UserInput.GetUserInput();

        while (string.IsNullOrWhiteSpace(stackName))
        {
            stackName = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter a [green] stack name[/] or type 0 to return to the main menu: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            if (stackName == "0") UserInput.GetUserInput();
        }

        using var connection = new SqlConnection(ConnectionString);
        var command = "INSERT INTO stacks (StackName) VALUES (@StackName)";
        var stack = new Stack { StackName = stackName };
        var stackCreation = connection.Execute(command, stack);
        AnsiConsole.Write(new Markup($"[green]{stackCreation}[/] stack added. Press any key to continue."));
        Console.ReadKey();

        var getStackIdCommand = $"SELECT StackId from stacks WHERE StackName = '{stackName}'";
        List<int> stackIdList = new List<int>();
        var stackIdQuery = connection.Query<Stack>(getStackIdCommand);
        foreach (var Id in stackIdQuery)
        {
            stackIdList.Add(Id.StackId);
        }
        var stackIdArray = stackIdList.ToArray();
        var stackId = stackIdArray[0];

        return stackId;
    }

    public static void CreateFlashCard()
    {
        var newStackQuestion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Will this be for a new [blue]stack[/]?")
                .PageSize(10)
                .AddChoices(new[]
                {
                    "Yes", "No"
                }));

        if (newStackQuestion == "Yes")
        {
            var stackId = CreateStack();
            var flashcardIndex = 1;
            using var connection = new SqlConnection(ConnectionString);
            var cardFront = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter the flash card [green]question[/]: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            while (string.IsNullOrWhiteSpace(cardFront))
            {
                cardFront = AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Empty value not allowed.[/] Please enter the flash card [green]question[/]: ")
                        .PromptStyle("blue")
                        .AllowEmpty());
            }
            AnsiConsole.Clear();
            var cardBack = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter the flash card [green]answer[/]: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            while (string.IsNullOrWhiteSpace(cardFront))
            {
                cardBack = AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Empty value not allowed.[/] Please enter the flash card [green]answer[/]: ")
                        .PromptStyle("blue")
                        .AllowEmpty());
            }

            var cardCreationCommand =
                "INSERT INTO flash_cards (FlashcardIndex,CardFront,CardBack,StackId) VALUES (@FlashcardIndex,@CardFront,@CardBack,@StackId)";
            var flashCard = new FlashCard
                { FlashcardIndex = flashcardIndex, CardFront = cardFront, CardBack = cardBack, StackId = stackId };

            var cardCreation = connection.Execute(cardCreationCommand, flashCard);
            AnsiConsole.Write(new Markup($"[green]{cardCreation}[/] stack added. Press any key to continue."));
            Console.ReadKey();

        }
        else
        {
            var stackId = GetStacks("to get Id");
            using var connection = new SqlConnection(ConnectionString);
            var getFlashCardsCommand = $"SELECT FlashcardIndex FROM flash_cards WHERE StackId = '{stackId}'";
            var getFlashCards = connection.Query<FlashCard>(getFlashCardsCommand);
            List<int> FlashcardIndexes = getFlashCards.Select(flashCard => flashCard.FlashcardIndex).ToList();
            var FlashcardIndex = FlashcardIndexes.AsQueryable().LastOrDefault() + 1;
            
            var cardFront = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter the flash card [green]question[/]: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            while (string.IsNullOrWhiteSpace(cardFront))
            {
                cardFront = AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Empty value not allowed.[/] Please enter the flash card [green]question[/]: ")
                        .PromptStyle("blue")
                        .AllowEmpty());
            }
            AnsiConsole.Clear();
            var cardBack = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter the flash card [green]answer[/]: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            while (string.IsNullOrWhiteSpace(cardFront))
            {
                cardBack = AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Empty value not allowed.[/] Please enter the flash card [green]answer[/]: ")
                        .PromptStyle("blue")
                        .AllowEmpty());
            }

            var cardCreationCommand =
                "INSERT INTO flash_cards (FlashcardIndex,CardFront,CardBack,StackId) VALUES (@FlashcardIndex,@CardFront,@CardBack,@StackId)";
            var flashCard = new FlashCard
                { FlashcardIndex = FlashcardIndex, CardFront = cardFront, CardBack = cardBack, StackId = stackId };

            var cardCreation = connection.Execute(cardCreationCommand, flashCard);
            AnsiConsole.Write(new Markup($"[green]{cardCreation}[/] stack added. Press any key to continue."));
            Console.ReadKey();
        }

    }

    public static void DeleteStack()
    {
        AnsiConsole.Clear();
        using var connection = new SqlConnection(ConnectionString);
        var selectStack = GetStacks("to delete");
        var deleteCommand = $"DELETE from stacks WHERE StackId = '{selectStack}'";
        var deleteStack = connection.Execute(deleteCommand);
        AnsiConsole.Write(new Markup($"[green]{deleteStack}[/] stack deleted. Press any key to continue."));
        Console.ReadKey();
    }

    public static void DeleteFlashCard()
    {
        AnsiConsole.Clear();
        var stackId = GetStacks("where your flash card resides");
        var flashcardId = GetFlashCards("to delete", stackId);
        using var connection = new SqlConnection(ConnectionString);
        var deleteCommand = $"DELETE from flash_cards WHERE FlashcardId = '{flashcardId}'";
        var flashcardIndexCommand = $"SELECT FlashcardIndex from flash_cards WHERE FlashcardId = '{flashcardId}'";
        var flashCardIdQuery = connection.Query<FlashCard>(flashcardIndexCommand);
        var flashcardIndexIdList = flashCardIdQuery.Select(flashcard => flashcard.FlashcardIndex).ToList();
        var flashCardIndexId = flashcardIndexIdList[0];
        var deleteFlashcard = connection.Execute(deleteCommand);
        var flashCardIndexUpdateCommand = $"UPDATE flash_cards SET FlashcardIndex = FlashCardIndex - 1 WHERE FlashCardIndex > {flashCardIndexId} AND StackId = '{stackId}'";
        var updateFlashcardIndexes = connection.Execute(flashCardIndexUpdateCommand);
        AnsiConsole.Write(new Markup($"[green]{deleteFlashcard}[/] flashcard deleted."));
        AnsiConsole.Write(new Markup($"[green]{updateFlashcardIndexes}[/] flashcard indexes updated. Press any key to continue."));
        Console.ReadKey();

    }

    public static int GetStacks(string function)
    {
        using var connection = new SqlConnection(ConnectionString);
        var getStacksCommand = "SELECT * FROM stacks";
        var stacks = connection.Query<Stack>(getStacksCommand);
        var stackList = stacks.Select(stack => stack.StackName).ToList();
        string?[] stackArray = stackList.ToArray();
        var selectStack = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select a [blue]Stack[/] {function}")
                .PageSize(10)!
                .AddChoices(stackList));
        var getStackIdCommand = $"SELECT StackId from stacks WHERE StackName = '{selectStack}'";
        var stackIdQuery = connection.Query<Stack>(getStackIdCommand);
        var stackIdList = stackIdQuery.Select(stack => stack.StackId).ToList();
        var stackId = stackIdList[0];

        return stackId;
    }

    public static int GetFlashCards(string? function,int stackId)
    {
        using var connection = new SqlConnection(ConnectionString);
        //var stackId = GetStacks("where your flash card resides");
        var getFlashCardsCommand = $"SELECT * FROM flash_cards WHERE StackId = '{stackId}'";
        var flashCards = connection.Query<FlashCard>(getFlashCardsCommand);
        var flashCardList = flashCards.Select(flashCard => flashCard.CardFront).ToList();
        var selectFlashCard = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select a [blue]flash card[/] {function}")
                .PageSize(10)!
                .AddChoices(flashCardList));
        var getFlashCardIdCommand = $"SELECT FlashcardId FROM flash_cards WHERE CONVERT (VARCHAR, CardFront) = '{selectFlashCard}'";
        var flashCardIdQuery = connection.Query<FlashCard>(getFlashCardIdCommand);
        var flashcardIdList = flashCardIdQuery.Select(flashcard => flashcard.FlashcardId).ToList();
        var flashCardId = flashcardIdList[0];
        return flashCardId;
    }
}