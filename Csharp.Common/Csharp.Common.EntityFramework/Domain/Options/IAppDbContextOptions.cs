namespace Csharp.Common.EntityFramework.Domain.Options;

public interface IAppDbContextOptions
{
    ConnectionStringOptions? ConnectionStrings { get; set; }
    int NumberOfQueryRetriesBeforeSendingException { get; set; }
}