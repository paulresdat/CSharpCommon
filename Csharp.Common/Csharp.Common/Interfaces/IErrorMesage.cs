namespace Csharp.Common.Interfaces;

public interface IErrorMessage
{
    public string Message { get; set; }
    public Exception? Exception { get; set; }
    object GetError();
}

public interface IErrorMessage<T> : IErrorMessage where T: struct
{
    public T Error { get; set; }
}