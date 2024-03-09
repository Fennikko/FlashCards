namespace Flashcards;

public class FlashCardController
{
    public static FlashCardDTO MapToDto(FlashCard flashcard)
    {
        return new FlashCardDTO
        {
            FlashcardIndex = flashcard.FlashcardIndex,
            CardFront = flashcard.CardFront,
            CardBack = flashcard.CardBack,
        };
    }
    public static List<FlashCardDTO> MapToDto(List<FlashCard> flashcards)
    {
        return flashcards.Select(MapToDto).ToList();
    }
}