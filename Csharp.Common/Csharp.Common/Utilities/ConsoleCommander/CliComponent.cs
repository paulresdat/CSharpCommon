namespace Csharp.Common.Utilities.ConsoleCommander;

public interface ICliComponent
{
    string ReadLine();
    void SetConsolePrompt(string prompt = "$> ");
}
public class CliComponent : ICliComponent
{
    private readonly List<string> _history = new();
    private string Prompt { get; set; } = "$> ";

    public void SetConsolePrompt(string prompt = "$> ")
    {
        Prompt = prompt;
    }

    public string ReadLine()
    {
        try
        {
            return ReadLineInternal();
        }
        catch (Exception)
        {
            Console.WriteLine("Exception was thrown: ");
            Console.WriteLine("Current character list: " + string.Join("", _currentCharacterList));
            Console.WriteLine("Arrow at: " + ArrowLeftCount);
            Console.WriteLine("Spliced idx at: " + SpliceIdx);
            throw;
        }
    }

    private readonly List<char> _currentCharacterList = new();
    private int ArrowLeftCount { get; set; } = 0;
    private int SpliceIdx { get; set; } = 0;

    private void Clear()
    {
        _currentCharacterList.Clear();
    }

    private void AddCharacterAtEnd(char what)
    {
        _currentCharacterList.Add(what);
    }

    private void InsertCharacters(string what)
    {
        _currentCharacterList.AddRange(what);
    }

    private void SpliceIn(int at, char what)
    {
        _currentCharacterList.RemoveAt(at);
        _currentCharacterList.Insert(at, what);
    }

    private void InsertAt(int at, char what)
    {
        _currentCharacterList.Insert(at, what);
    }

    private void ClearConsoleWithPrompt(string addedData = "")
    {
        Console.Write("\r                                     \r" + Prompt + addedData);
    }

    private void ClearConsoleWithPrompt(List<char> characterList)
    {
        Console.Write("\r" + Prompt + string.Join("", characterList));
    }

    private void Backspace()
    {
        CursorToTheLeft(2);
        Console.Write(" ");
        CursorToTheLeft(1);
    }

    private void CursorToTheLeft(int byHowMuch)
    {
        Console.SetCursorPosition(Console.CursorLeft - byHowMuch, Console.CursorTop );
    }

    private void CursorToTheRight(int byHowMuch)
    {
        Console.SetCursorPosition(Console.CursorLeft + byHowMuch, Console.CursorTop );
    }


    private class CurrentCursorPosition
    {
        public int FromLeft { get; set; }
        public int FromTop { get; set; }
    }

    private readonly CurrentCursorPosition _currentCursorPosition = new();
    private void SaveCursorPosition()
    {
        _currentCursorPosition.FromLeft = Console.CursorLeft;
        _currentCursorPosition.FromTop = Console.CursorTop;
    }

    private void ResetCursorToLastSaveAfterInput()
    {
        Console.SetCursorPosition(_currentCursorPosition.FromLeft, _currentCursorPosition.FromTop);
    }

    private int EndCursorIdx => Prompt.Length + _currentCharacterList.Count;

    private string ReadLineInternal()
    {
        _currentCharacterList.Clear();
        ArrowLeftCount = 0;
        var idx = _history.Count;
        Console.Write(Prompt);
        while (true)
        {
            var chInt = Console.ReadKey();
            SaveCursorPosition();
            if (chInt.Key == ConsoleKey.UpArrow)
            {
                if (idx != 0)
                {
                    idx--;
                }

                if (_history.Count > 0)
                {
                    Clear();
                    InsertCharacters(_history[idx]);
                    ClearConsoleWithPrompt(_history[idx]);
                }
            }
            else if (chInt.Key == ConsoleKey.DownArrow)
            {
                if (idx < _history.Count)
                {
                    idx++;
                }

                if (idx == _history.Count)
                {
                    ClearConsoleWithPrompt();
                    Clear();
                }
                else
                {
                    Clear();
                    InsertCharacters(_history[idx]);
                    ClearConsoleWithPrompt(_history[idx]);
                }
                
            }
            else if (chInt.Key == ConsoleKey.Backspace)
            {
                if (_currentCharacterList.Count != 0)
                {
                    Backspace();
                    _currentCharacterList.RemoveAt(_currentCharacterList.Count - 1);
                }
                else
                {
                    CursorToTheLeft(1);
                }
            }
            else if (chInt.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                var command = string.Join("", _currentCharacterList);
                _history.Add(command);
                return command;
            }
            else if (chInt.Key == ConsoleKey.LeftArrow)
            {
                if (ArrowLeftCount != _currentCharacterList.Count)
                {
                    CursorToTheLeft(1);
                    ArrowLeftCount++;
                }
            }
            else if (chInt.Key == ConsoleKey.RightArrow)
            {
                if (ArrowLeftCount != 0)
                {
                    CursorToTheRight(1);
                    ArrowLeftCount--;
                }
            }
            else
            {
                SpliceIdx = _currentCharacterList.Count - ArrowLeftCount;
                InsertAt(SpliceIdx, chInt.KeyChar);
                ClearConsoleWithPrompt(_currentCharacterList);
                ResetCursorToLastSaveAfterInput();
            }
        }
    }
}
