using System.Diagnostics.CodeAnalysis;
using Csharp.Common.Builders;
using Csharp.Common.EntityFramework.Builders;
using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Options;
using Csharp.Common.EntityFramework.UnitTesting.Extensions;
using Csharp.Common.UnitTesting;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class BuilderDbContextTests : BaseUnitTest
{
    private class DbOptions : IAppDbContextOptions
    {
        public ConnectionStringOptions? ConnectionStrings { get; set; }
        public int NumberOfQueryRetriesBeforeSendingException { get; set; }
    }

    public interface IMockDbContext : IAppDbContext
    {
        DbSet<GenericDto> Dtos { get; set; }
    }
    private readonly ITestOutputHelper _output;

    private class Hi
    {
    }

    public BuilderDbContextTests(ITestOutputHelper output)
    {
        _output = output;
        MockOption<IAppDbContextOptions>(new DbOptions());
        Mock<IMockDbContext>(m =>
        {
            m.AddDbSet(x => x.Dtos, new List<GenericDto>().AsQueryable());
            m.Setup(x => x.SaveChanges()).Returns(1);
        });
    }

    private IMockDbContext DbContext => GetNewServiceProvider.GetRequiredService<IMockDbContext>();

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
        Services.AddSingleton<MockedDbContext>();
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

    public class GenericDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class MockedDbContext : AppDbContext
    {
        public MockedDbContext(IOptions<IAppDbContextOptions> options) : base(options)
        {
        }
    }

    private class GenericDtoBuilder : BuilderDbContext<GenericDto, IMockDbContext, GenericDtoBuilder>
    {
        public override GenericDtoBuilder WithDefaults()
        {
            With(x => { });
            return this;
        }
    }
}