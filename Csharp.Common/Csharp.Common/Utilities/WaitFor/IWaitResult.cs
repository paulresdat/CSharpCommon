namespace Csharp.Common.Utilities.WaitFor;

public interface IWaitResult
{
    bool Success { get; set; }
    string? Reason { get; set; }
    WaitStatus WaitStatus { get; set; }
}