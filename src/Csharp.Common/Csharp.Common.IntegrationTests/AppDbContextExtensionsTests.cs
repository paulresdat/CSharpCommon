using Csharp.Common.Db;
using Csharp.Common.EntityFramework.Extensions;
using Csharp.Common.EntityFramework.UnitTesting.UnitTests;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.IntegrationTests;

[Trait("Category", "Integration")]
public class AppDbContextExtensionsTests : IntegrationTestingFixture
{
  protected override bool UseAmbientTransaction => false;

  public AppDbContextExtensionsTests()
  {
    Services.AddOptions();
    var configuration = DesignTimeDbContextFactory.GetConfiguration();
    Services.Configure<DbOptions>(configuration.GetSection("DbContextOptions"));
    AddDbContext<ICsharpCommonTestingDbContext, CsharpCommonTestingDbContext>();
    StartTestTransaction();
  }

  private ICsharpCommonTestingDbContext DbContext =>
    ServiceProvider.GetRequiredService<ICsharpCommonTestingDbContext>();

  [Fact]
  public void ClearTable_removes_all_rows_from_the_table()
  {
    using var transaction = DbContext.Database.BeginTransaction();

    DbContext.People.Add(new() { Name = "Clear table test" });
    DbContext.SaveChanges();

    Assert.NotEmpty(DbContext.People.ToList());

    DbContext.ClearTable(x => x.People);

    Assert.Empty(DbContext.People);
    transaction.Rollback();
  }
}
