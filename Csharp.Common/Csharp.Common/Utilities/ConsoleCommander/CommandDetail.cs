namespace Csharp.Common.Utilities.ConsoleCommander;

internal enum AsyncType
{
    None,
    Simple,
    WithParameter
}

internal abstract class CommandDetail
{
    public string? Command { get; set; }
    public string? RegexStr { get; set; }
    public string? Description { get; set; }
    public AsyncType AsyncType { get; set; } = AsyncType.None;
    protected object? ActionVal { get; set; }
    public object GetAction => ActionVal ?? throw new InvalidOperationException();
}

internal class CommandDetail<T> : CommandDetail where T : Delegate
{
    public T? Action
    {
        set => ActionVal = value;
        // get => (T) GetAction;
    }

    public Func<Task> SimpleAsync
    {
        // get => (Func<Task>) GetAction;
        set
        {
            AsyncType = AsyncType.Simple;
            ActionVal = value;
        }
    }

    public Func<string, Task>? ActionAsync
    {
        set
        {
            AsyncType = AsyncType.WithParameter;
            ActionVal = value;
        }
        // get => (Func<string, Task>)GetAction;
    }
}
