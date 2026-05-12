using Csharp.Common.EntityFramework.Domain.Options;

namespace Csharp.Common.Db;

public class DbOptions : IAppDbContextOptions
{
    public ConnectionStringOptions? ConnectionStrings { get; set; }
    public int NumberOfQueryRetriesBeforeSendingException { get; set; }
}