using System.Diagnostics.CodeAnalysis;
using Csharp.Common.Builders;
using Csharp.Common.UnitTesting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class BuilderDbContextTests : BaseUnitTest
{
    private readonly ITestOutputHelper _output;

    public BuilderDbContextTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private MockedDbContext DbContext => new MockedDbContext();

    [Fact(DisplayName = "001 Build Db Context can build a dto without hitting the database")]
    public void T001()
    {
        var dto = GenericDtoBuilder.Create(DbContext)
            .With(x => { x.Id = 1; })
            .Build();
        dto.Id.Should().Be(1);
    }

    [Fact(DisplayName = "002 When injecting the DbContext, calling save changes will indeed return the build object")]
    public void T002()
    {
        var dto = GenericDtoBuilder.Create(DbContext)
            .With(x => { x.Name = "Hi"; })
            .BuildAndSave();
        dto.Name.Should().Be("Hi");
    }

    [Fact(DisplayName = "003 When injecting the service scope, calling save changes will indeed return the build object")]
    public void T003()
    {
        ServiceCollection.AddSingleton<MockedDbContext>();
        var sp = GetNewServiceProvider;
        var dto = GenericDtoBuilder.Create(sp.GetRequiredService<IServiceScopeFactory>())
            .With(x => { x.Name = "Hi"; })
            .BuildAndSave();
        dto.Name.Should().Be("Hi");
    }

    [Fact(DisplayName = "004 You technically don't need the db context when just building the object")]
    public void T004()
    {
        var dto = GenericDtoBuilder.Create()
            .With(x => { x.Name = "Hi"; })
            .Build();
        dto.Name.Should().Be("Hi");
    }

    [Fact(DisplayName = "005 If you try and build and save the object without the db context factory injected, it will throw an error")]
    public void T005()
    {
        Assert.Throws<BuilderException>(() => GenericDtoBuilder.Create()
            .With(x => { x.Name = "Hi"; })
            .BuildAndSave());
    }

    private class GenericDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class MockedDbContext
    {
        public void SaveChanges()
        {
            
        }
    }

    private class GenericDtoBuilder : BuilderDbContext<GenericDto, MockedDbContext, GenericDtoBuilder>
    {
        public override GenericDtoBuilder WithDefaults()
        {
            With(x => { });
            return this;
        }

        public override GenericDto BuildAndSave()
        {
            var built = (GenericDto) this;
            DbContext.SaveChanges();
            return built;
        }
    }
}