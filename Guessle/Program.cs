// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

const char RightLetterRightPlace = '+';
const char RightLetterWrongPlace = '?';
const char WrongLetter = '.';
const string AllTheLetters = "abcdefghijklmnopqrstuvwxyz";
var fiveLetterWords = GetFiveLetterWords("Content/dictionary.txt");

var guessHistory = new GuessHistory();

ShowHowToPlay();

while (true)
{
    Console.Write("> ");

    var guess = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

    if (guess == "quit")
        break;
    else if (guess == "hint")
        ShowHints(guessHistory, fiveLetterWords);
    else if (guess == "restart")
    {
        guessHistory = new GuessHistory();
    }
    else if (guess.Length == 5)
    {
        if (guess.Any(l => !AllTheLetters.Contains(l)))
        {
            Console.WriteLine("A guess must only contain letters.");
            continue;
        }

        Console.WriteLine("What was WORDLE's response? Type 5 characters using the following rules:");
        Console.WriteLine($"{RightLetterRightPlace} for a letter in the correct position");
        Console.WriteLine($"{RightLetterWrongPlace} for a letter in the wrong position");
        Console.WriteLine($"{WrongLetter} for a letter not in the word");
        Console.WriteLine("Type nothing to cancel.");

        while (true)
        {
            Console.Write("> ");

            var response = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

            if (response.Length == 0)
                break;

            if (response.Length != 5)
            {
                Console.WriteLine("Please enter 5 characters.");
                continue;
            }

            if (!response.Any(r => r is not RightLetterRightPlace or RightLetterWrongPlace or WrongLetter))
            {
                Console.WriteLine($"Only {RightLetterRightPlace}, {RightLetterWrongPlace}, and {WrongLetter} should be used.");
                continue;
            }

            Console.WriteLine($"Got it. You guessed \"{guess}\", and:");

            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine($"{guess[i]} is {DescribeResponse(response[i])}");
            }

            guessHistory.Guesses.Add(new(guess, response));
            break;
        }
    }
    else
    {
        ShowHowToPlay();
    }
}

static string DescribeResponse(char c) => c switch
{
    '+' => "correct",
    '?' => "in the wrong place",
    '.' => "not in the word at all",
    _ => throw new UnreachableException()
};

static List<string> GetFiveLetterWords(string fileName)
{
    var words = new List<string>();

    using var reader = new StreamReader(fileName);

    while (reader.ReadLine()?.Trim() is {} line)
    {
        if (line.Length == 5)
            words.Add(line.ToLowerInvariant());
    }

    return words;
}

static void ShowHowToPlay()
{
    Console.WriteLine("Enter a five letter word to register a guess. You will then be asked for WORDLE's response.");
    Console.WriteLine("Type \"hint\" to get a list of words that the solution COULD be.");
    Console.WriteLine("Type \"restart\" to restart the game.");
    Console.WriteLine("Type \"quit\" to quit.");
}

static void ShowHints(GuessHistory guessHistory, List<string> allFiveLetterWords)
{
    var possibleWords = allFiveLetterWords
        .Where(w => WordIsPossible(w, guessHistory))
        .ToList();

    Console.WriteLine($"There are {possibleWords.Count} possible words.");
    Console.WriteLine("Show them now? (Y/n)");

    var showThem = GetYesOrNo();

    if (!showThem)
    {
        Console.WriteLine("Okay.");
        return;
    }

    Console.WriteLine(string.Join(Environment.NewLine, possibleWords));
}

static bool WordIsPossible(string word, GuessHistory wordInformation)
    => wordInformation.Guesses.All(guess => WordMatchesGuess(word, guess));

static bool WordMatchesGuess(string word, Guess guess)
{
    if(WordContainsAnyWrongLetters(word, guess))
        return false;

    // all right-letter-right-place letters must be accounted for
    for (int i = 0; i < 5; i++)
    {
        if(guess.Response[i] == RightLetterRightPlace && word[i] != guess.Word[i])
            return false;
    }

    if (!WordContainsAllRequiredLetters(word, guess))
        return false;

    for (int i = 0; i < 5; i++)
    {
        if (word[i] == guess.Word[i] && guess.Response[i] != RightLetterRightPlace)
            return false;
    }

    return true;
}

static bool WordContainsAnyWrongLetters(string word, Guess guess)
    => word.Any(l => guess.Word.Contains(l) && guess.Response[guess.Word.IndexOf(l)] == WrongLetter);

static bool WordContainsAllRequiredLetters(string word, Guess guess)
{
    var remainingLetters = guess.Word.Where((_, i) => guess.Response[i] != WrongLetter).ToList();

    for (int i = 0; i < 5 && remainingLetters.Count > 0; i++)
    {
        if(remainingLetters.IndexOf(word[i]) >= 0)
            remainingLetters.RemoveAt(remainingLetters.IndexOf(word[i]));
    }

    return remainingLetters.Count == 0;
}

static bool GetYesOrNo()
{
    while (true)
    {
        Console.Write("> ");

        var response = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

        if (response is "y" or "yes")
            return true;
        else if (response is "" or "n" or "no")
            return false;
    }
}

public class GuessHistory
{
    public List<Guess> Guesses { get; } = new();
}

public record Guess(string Word, string Response);
