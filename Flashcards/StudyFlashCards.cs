using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Flashcards;
using Spectre.Console;

public class StudyFlashCards
{
    public static readonly string? ConnectionString = ConfigurationManager.AppSettings.Get("connectionString");
    public static void NewStudySession()
    {
        using var connection = new SqlConnection(ConnectionString);
        var sessionDate = DateTime.Now;
        var stackId = DatabaseController.GetStacks("to study");
        var flashCards = DatabaseController.GetAllFlashCards(stackId);
        int score = 0;
        foreach (var flashCard in flashCards)
        {
            Console.WriteLine($"{flashCard.FlashcardIndex} {flashCard.CardFront} {flashCard.CardBack}");

            AnsiConsole.Clear();
            var table = new Table();
            table.Title(new TableTitle("[blue]Study Session[/]"));
            table.AddColumn(new TableColumn("[#FFA500]FlashcardId[/]").Centered());
            table.AddColumn(new TableColumn("[#104E1D]Question[/]").Centered());

            table.AddRow($"[#3EB489]{flashCard.FlashcardIndex}[/]", $"[#3EB489]{flashCard.CardFront}[/]");
            AnsiConsole.Write(table);

            var answer = flashCard.CardBack;
            var studyAnswer = AnsiConsole.Prompt(
                new TextPrompt<string>("please enter your [green]answer[/] to the above question: ")
                    .PromptStyle("blue")
                    .AllowEmpty());
            while (string.IsNullOrWhiteSpace(studyAnswer))
            {
                studyAnswer = AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Invalid entry, cannot be empty.[/] Please enter your [green]answer[/] to the above question: ")
                        .PromptStyle("blue")
                        .AllowEmpty());
            }

            if (studyAnswer.Trim().Equals(answer.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                score++;
                AnsiConsole.MarkupLine($"Correct! your current score is [green]{score}[/]. Press any key to continue");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Incorrect.[/] Press any key to continue");
                Console.ReadKey();
            }
            

        }
        AnsiConsole.MarkupLine($"The final score of your study session is: [green]{score}[/] Press any key to continue.");
        Console.ReadKey();

        var studySessionCreationCommand =
            "INSERT INTO study_sessions (SessionDate,SessionScore,StackId) VALUES (@SessionDate,@SessionScore,@StackId)";
        var newStudySession = new StudySession
            {SessionDate = sessionDate, SessionScore = score, StackId = stackId};
        var studySession = connection.Execute(studySessionCreationCommand,newStudySession);
        AnsiConsole.Write(new Markup($"[green]{studySession}[/] study session added. Press any key to continue."));
        Console.ReadKey();
    }
}
