using System.Diagnostics;

namespace Csharp.Common.Utilities.WaitFor;

/// <summary>
/// The Waiter: A simple wrapper interface around the concept that you have to wait for a status of something, be it
/// a status expected within an object, list of objects or even a database call.  The Wait For class can be injected
/// or fetched as a transient from your service provider and with the simple interface using your custom wait result
/// object, it will wait for an expected outcome you define before it returns the wait result object can be utilized
/// to wait for an event before it errors out within a specified amount of time.
///
/// It does not wait forever, it must have a timeout.
/// </summary>
public interface IWait : ITransientService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="action"></param>
    /// <param name="iterationWaitInMilliseconds"></param>
    /// <param name="timeoutStatusReason"></param>
    /// <returns></returns>
    IWaitResult For(
        int seconds,
        Func<WaitStatusResult>? action = null,
        int iterationWaitInMilliseconds = 1000,
        string timeoutStatusReason = "Action reason did not return a status of success within allotted time");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="action"></param>
    /// <param name="iterationWaitInMilliseconds"></param>
    /// <param name="timeoutStatusReason"></param>
    /// <returns></returns>
    IWaitResult For(
        int seconds,
        Func<WaitStatus>? action = null,
        int iterationWaitInMilliseconds = 1000,
        string timeoutStatusReason = "Action reason did not return a status of success within allotted time");
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Wait<T> : IWait where T: class, IWaitResult, new()
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="action"></param>
    /// <param name="iterationWaitInMilliseconds"></param>
    /// <param name="timeoutStatusReason"></param>
    /// <returns></returns>
    public IWaitResult For(
        int seconds,
        Func<WaitStatus>? action = null,
        int iterationWaitInMilliseconds = 1000,
        string timeoutStatusReason = "Action reason did not return a status of success within allotted time")
    {
        return For(
            seconds: seconds,
            iterationWaitInMilliseconds: iterationWaitInMilliseconds,
            timeoutStatusReason: timeoutStatusReason,
            action: () =>
            {
                var result = action?.Invoke();
                return new WaitStatusResult
                {
                    WaitStatus = result ?? WaitStatus.Error,
                    Reason = result is null ? "(result returned null)" : null,
                };
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="action"></param>
    /// <param name="iterationWaitInMilliseconds"></param>
    /// <param name="timeoutStatusReason"></param>
    /// <returns></returns>
    public IWaitResult For(
        int seconds,
        Func<WaitStatusResult>? action = null,
        int iterationWaitInMilliseconds = 1000,
        string timeoutStatusReason = "Action reason did not return a status of success within allotted time")
    {
        var shouldContinue = true;
        var timer = new Stopwatch();
        timer.Start();
        while (shouldContinue)
        {
            if (action is not null)
            {
                var result = action();
                if (result.WaitStatus != WaitStatus.Continue)
                {
                    return GetResultInstance(result);
                }
            }

            shouldContinue = timer.Elapsed.TotalSeconds <= seconds;
            Thread.Sleep(iterationWaitInMilliseconds);
        }
        timer.Stop();

        return GetResultInstance(new() {
            WaitStatus = WaitStatus.Error,
            Reason = timeoutStatusReason,
        });
    }

    protected virtual IWaitResult GetResultInstance(WaitStatusResult result)
    {
        var returnResult = Activator.CreateInstance<T>();
        returnResult.WaitStatus = result.WaitStatus;
        returnResult.Success = result.WaitStatus == WaitStatus.Success;
        returnResult.Reason = result.Reason ?? "(no reason specified)";
        return returnResult;
    }
}