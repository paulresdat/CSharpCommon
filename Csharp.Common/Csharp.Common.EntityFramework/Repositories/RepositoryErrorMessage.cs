using Csharp.Common.Interfaces;

namespace Csharp.Common.EntityFramework.Repositories;

public class RepositoryErrorMessage : IErrorMessage<RepositoryError>
{
    private string? _message;

    public string Message
    {
        set => _message = value;
        get => _message ?? throw new InvalidOperationException();
    }

    private RepositoryError? _error;
    public RepositoryError Error
    {
        set => _error = value;
        get => _error ?? throw new InvalidOperationException();
    }

    public Exception? Exception { get; set; }

    public object GetError()
    {
        return Error;
    }
}