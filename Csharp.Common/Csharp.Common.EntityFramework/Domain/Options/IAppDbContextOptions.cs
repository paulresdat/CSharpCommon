namespace Csharp.Common.EntityFramework.Domain;

public interface IAppDbContextOptions
{
    ConnectionStringOptions? ConnectionStrings { get; set; }
    int NumberOfQueryRetriesBeforeSendingException { get; set; }
}