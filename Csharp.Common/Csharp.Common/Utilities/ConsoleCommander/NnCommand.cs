namespace Csharp.Common.Utilities.ConsoleCommander;

public abstract class NnCommand
{
    public string? Command { get; set; }
    public string? RegexStr { get; set; }
    public string? Description { get; set; }
    public bool IsAsync { get; set; }
    protected object? ActionVal { get; set; }
    public object GetAction => ActionVal ?? throw new InvalidOperationException();
}
public class NnCommand<T> : NnCommand where T : Delegate
{
    public T? Action
    {
        set => ActionVal = value;
        get => (T) GetAction;
    }

    public Func<string, Task>? ActionAsync
    {
        set
        {
            IsAsync = true;
            ActionVal = value;
        }

        get => (Func<string, Task>)GetAction;
    }
}
