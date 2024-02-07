using System.ComponentModel.DataAnnotations;
using Csharp.Common.Builders;
using Csharp.Common.UnitTesting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests.Common;

public class BuilderTests : BaseUnitTest
{
    private readonly ITestOutputHelper _output;

    public BuilderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "001 A builder can be instantiated using the basic build abstract class")]
    public void T001()
    {
        var gen = new GenericBuilder();
        var dto = gen.WithDefaults().WithId(1).Build();
        dto.Id.Should().Be(1);
    }

    [Fact(DisplayName =
        "002 A builder can utilize the custom string length attribute to impose length restrictions on set values")]
    public void T002()
    {
        var gen = GenericBuilder.Create();
        var dto = gen.WithDefaults().WithId(1).WithName("Testing").Build();
        dto.Name.Should().Be("Testing");

        Assert.Throws<BuilderException>(() => GenericBuilder.Create()
            .WithDefaults()
            .WithId(1)
            .WithName("Testing 123 Goes past 20 characters by quite a bit")
            .Validate()
            .Build());
    }

    [Fact(DisplayName = "003 A builder can validate against a custom data validation attribute")]
    public void T003()
    {
        var dto = GenericDtoWithCustomAttributeBuilder.Create()
            .WithDefaults()
            .With(x => x.Name = "test")
            .Validate()
            .Build();

        dto.Name.Should().Be("test");
    }

    [Fact(DisplayName = "004 A builder can use regular expressions to validate a string based dto")]
    public void T004()
    {
        var dto = GenericWithRegexBuilder.Create()
            .WithRegexString("string is going to validation")
            .Validate()
            .Build();

        dto.RegexString.Should().Be("string is going to validation");

        Assert.Throws<BuilderException>(() => GenericWithRegexBuilder.Create()
            .WithRegexString("this string will throw an error")
            .Validate()
            .Build());
    }

    [Fact(DisplayName = "005 When a builder connect is not furnished a service scope provider, it errors out")]
    public void T005()
    {
        var dto = new BuilderConnector();
        dto.WithDefaults().Build();
        Assert.Throws<BuilderException>(() => dto.GetDbContext); 
    }

    [Fact(DisplayName =
        "006 Registering the db context directly will result in the db context being set with that type")]
    public void T006()
    {
        var dto = new BuilderConnector();
        var o = new BuilderDbConnectorContextMock();
        dto.Register(o);
        dto.GetDbContext.Should().Be(o);
    }

    [Fact(DisplayName = "007 When registering the db context in the service provider, the service scope will fetch it")]
    public void T007()
    {
        ServiceCollection.AddSingleton<BuilderDbConnectorContextMock>();
        var dto = new BuilderConnector();
        dto.ServiceScope(GetNewServiceProvider.GetRequiredService<IServiceScopeFactory>());
        dto.GetDbContext.GetType().Name.Should().Be(nameof(BuilderDbConnectorContextMock));
    }

    [Fact(DisplayName = "008 Builder factory can fetch a builder of the IBuilder type without a service scope")]
    public void T008()
    {
        ServiceCollection.AddSingleton<BuilderFactoryOne>();

        var sp = GetNewServiceProvider;
        var bf = sp.GetRequiredService<BuilderFactoryOne>();
        var builder = bf.Fetch<GenericDtoWithCustomAttributeBuilder>();
        builder.Should().NotBeNull();
    }

    [Fact(DisplayName = "009 Builder factory can fetch a builder of the IBuilderServiceScope type and the service scope should return it")]
    public void T009()
    {
        ServiceCollection.AddSingleton<BuilderFactoryOne>();

        var sp = GetNewServiceProvider;
        var bf = sp.GetRequiredService<BuilderFactoryOne>();
        var builder = bf.Fetch<GenericDtoWithCustomAttributeBuilder>();
        builder.Should().NotBeNull();
    }

    [Fact(DisplayName = "010 Builder factory can fetch a builder of the IBuilderServiceScope type and the service scope should return it")]
    public void T010()
    {
        ServiceCollection.AddSingleton<BuilderFactoryOne>();
        ServiceCollection.AddSingleton<GenericWithRegexBuilder>();

        var sp = GetNewServiceProvider;
        var bf = sp.GetRequiredService<BuilderFactoryOne>();
        var builder = bf.Fetch<GenericWithRegexBuilder>();
        builder.Should().NotBeNull();
        builder.ServiceScopeRegistered.Should().BeTrue();
    }

    [Fact(DisplayName = "010 Builder factory will throw an exception if the builder is not of the expected interface types")]
    public void T011()
    {
        ServiceCollection.AddSingleton<BuilderFactoryOne>();

        var sp = GetNewServiceProvider;
        var bf = sp.GetRequiredService<BuilderFactoryOne>();
        Assert.Throws<BuilderFactoryException>(() => bf.Fetch<GenericDtoWithCustomAttributeDto>());
    }

    private class BuilderFactoryOne : BuilderFactory
    {
        public BuilderFactoryOne(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
    }

    private class BuilderDbConnectorContextMock
    {
    }

    private class BuilderConnector : BuilderDbConnector<BuilderDbConnectorContextMock, GenericDto, BuilderConnector>
    {
        public BuilderDbConnectorContextMock GetDbContext => DbContext;

        public override BuilderConnector WithDefaults()
        {
            With(x => { });
            return this;
        }

        public void Register<T>(T m) where T: BuilderDbConnectorContextMock
        {
            RegisterTheDbContextExplicitlyRatherThanScopeFactory(m);
        }

        public void ServiceScope(IServiceScopeFactory sc)
        {
            RegisterBuilderDbConnectorServiceScopeFactory(sc);
        }
    }

    private class GenericDto
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Name { get; set; } = string.Empty;
    }

    private class GenericWithRegex
    {
        [RegularExpression("^string.*?validation$")]
        public string RegexString { get; set; } = string.Empty;
    }

    private class GenericWithRegexBuilder : Builder<GenericWithRegex, GenericWithRegexBuilder>, IBuilderServiceScope
    {
        public bool ServiceScopeRegistered { get; set; }
        public override GenericWithRegexBuilder WithDefaults()
        {
            With(x => x.RegexString = string.Empty);
            return this;
        }

        public GenericWithRegexBuilder WithRegexString(string stringData)
        {
            With(x => x.RegexString = stringData);
            return this;
        }

        public void RegisterServiceScope(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeRegistered = true;
        }
    }

    private class CustomValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return true;
        }
    }

    private class GenericDtoWithCustomAttributeDto
    {
        public int Id { get; set; }

        [CustomValidation]
        public string Name { get; set; } = string.Empty;
    }

    private class GenericDtoWithCustomAttributeBuilder : Builder<GenericDtoWithCustomAttributeDto, GenericDtoWithCustomAttributeBuilder>
    {
        public override GenericDtoWithCustomAttributeBuilder WithDefaults()
        {
            With(x => x.Id = 0);
            return this;
        }
    }

    private class GenericBuilder : Builder<GenericDto, GenericBuilder>
    {
        public override GenericBuilder WithDefaults()
        {
            With(x =>
            {
                x.Id = 0;
            });
            return this;
        }

        public GenericBuilder WithId(int id)
        {
            With(x => x.Id = id);
            return this;
        }

        public GenericBuilder WithName(string name)
        {
            With(x => x.Name = name);
            return this;
        }
    }
}
