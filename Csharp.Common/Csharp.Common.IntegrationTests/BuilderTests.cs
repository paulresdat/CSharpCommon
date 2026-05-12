using Csharp.Common.Db;
using Csharp.Common.Db.Builders;
using Csharp.Common.EntityFramework.Extensions;
using Csharp.Common.EntityFramework.UnitTesting.UnitTests;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.IntegrationTests;

public class BuilderTests : IntegrationTestingFixture
{
    public BuilderTests()
    {
        Services.AddOptions();
        var builder = DesignTimeDbContextFactory.GetConfiguration();
        Services.Configure<DbOptions>(builder.GetSection("DbContextOptions"));
        AddDbContext<ICsharpCommonTestingDbContext, CsharpCommonTestingDbContext>();
        StartTestTransaction();
        DbContext.ClearTable(x => x.People);
    }

    private ICsharpCommonTestingDbContext DbContext =>
        ServiceProvider.GetRequiredService<ICsharpCommonTestingDbContext>();

    [Fact]
    public void Test1()
    {
        var person = PersonBuilder.Create(DbContext)
            .WithDefaults()
            .WithName("test 123")
            .BuildAndSave();

        Assert.Equal("test 123", person.Name);
        Assert.True(person.PersonId > 0);
    }
}