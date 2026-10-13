using Csharp.Common.Db.Entities;
using Csharp.Common.EntityFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Csharp.Common.Db;

public interface ICsharpCommonTestingDbContext : IAppDbContext
{
    DbSet<Person> People { get; }
    DbSet<Car> Cars { get; }
    DbSet<PersonCarXref> PersonCarXrefs { get; }
}

public class CsharpCommonTestingDbContext(IOptions<DbOptions> options)
    : AppDbContext(options), ICsharpCommonTestingDbContext
{
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(ConnectionString, sqlServerOptionsAction: sqlOptions => { });
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<PersonCarXref> PersonCarXrefs => Set<PersonCarXref>();
}