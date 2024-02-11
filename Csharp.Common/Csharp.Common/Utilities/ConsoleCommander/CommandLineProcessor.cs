namespace Csharp.Common.Utilities.ConsoleCommander;

public interface ICommandLineProcessor
{
    string ReadLine();
    void SetConsolePrompt(string prompt = "$> ");
}

public class CommandLineProcessor : ICommandLineProcessor
{
    private readonly IConsoleOutput _cnsl;

    public CommandLineProcessor(IConsoleOutput output)
    {
        _cnsl = output;
    }

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
            _cnsl.WriteLine("Exception was thrown: ")
                .WriteLine("Current character list: " + string.Join("", _currentCharacterList))
                .WriteLine("Arrow at: " + ArrowLeftCount)
                .WriteLine("Spliced idx at: " + SpliceIdx);
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
        _cnsl.Write("\r                                     \r" + Prompt + addedData);
    }

    private void ClearConsoleWithPrompt(List<char> characterList)
    {
        _cnsl.Write("\r" + Prompt + string.Join("", characterList));
    }

    private void Backspace()
    {
        CursorToTheLeft(2);
        _cnsl.Write(" ");
        CursorToTheLeft(1);
    }

    private void CursorToTheLeft(int byHowMuch)
    {
        _cnsl.SetCursorPosition(_cnsl.CursorLeft - byHowMuch, _cnsl.CursorTop);
    }

    private void CursorToTheRight(int byHowMuch)
    {
        _cnsl.SetCursorPosition(_cnsl.CursorLeft + byHowMuch, _cnsl.CursorTop);
    }


    private class CurrentCursorPosition
    {
        public int FromLeft { get; set; }
        public int FromTop { get; set; }
    }

    private readonly CurrentCursorPosition _currentCursorPosition = new();
    private void SaveCursorPosition()
    {
        _currentCursorPosition.FromLeft = _cnsl.CursorLeft;
        _currentCursorPosition.FromTop = _cnsl.CursorTop;
    }

    private void ResetCursorToLastSaveAfterInput()
    {
        _cnsl.SetCursorPosition(_currentCursorPosition.FromLeft, _currentCursorPosition.FromTop);
    }

    private int EndCursorIdx => Prompt.Length + _currentCharacterList.Count;

    private string ReadLineInternal()
    {
        _currentCharacterList.Clear();
        ArrowLeftCount = 0;
        var idx = _history.Count;
        _cnsl.Write(Prompt);
        while (true)
        {
            var chInt = _cnsl.ReadKey();
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
                _cnsl.WriteLine();
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
