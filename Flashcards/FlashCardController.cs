using Flashcards.Models;

namespace Flashcards;

public class FlashCardController
{
    public static FlashCardDto MapToDto(FlashCard flashcard)
    {
        return new FlashCardDto
        {
            FlashcardIndex = flashcard.FlashcardIndex,
            CardFront = flashcard.CardFront,
            CardBack = flashcard.CardBack,
        };
    }

    public static List<FlashCardDto> MapToDto(List<FlashCard> flashcards)
    {
        return flashcards.Select(MapToDto).ToList();
    }
}