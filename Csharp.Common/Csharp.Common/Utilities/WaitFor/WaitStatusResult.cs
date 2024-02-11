namespace Csharp.Common.Utilities.WaitFor;

public interface IWaitStatusResult
{
    WaitStatus WaitStatus { get; set; }
    string? Reason { get; set; }
}

public class WaitStatusResult : IWaitStatusResult
{
    public WaitStatus WaitStatus { get; set; }
    public string? Reason { get; set; }
}