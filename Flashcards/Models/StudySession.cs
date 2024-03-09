namespace Flashcards.Models;

public class StudySession
{
    public int StudyId { get; set; }

    public DateTime SessionDate { get; set; }

    public int SessionScore { get; set; }

    public int StackId { get; set; }
}