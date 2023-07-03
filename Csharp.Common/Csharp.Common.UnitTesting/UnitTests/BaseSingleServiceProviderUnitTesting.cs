using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.UnitTesting.UnitTests;

/// <summary>
/// When you are making integration tests that require a single service provider, use this which
/// will help enforce that standard by throw exceptions and ensuring a singleton approach.  The
/// service provider remains private so it can not be accessed outside hardening single origin.
/// This also helps keep unit tests cleaner!
/// </summary>
public abstract class BaseSingleServiceProviderUnitTesting : BaseUnitTest
{
    private IServiceProvider? _serviceProvider;

    protected override IServiceProvider GetNewServiceProvider =>
        throw new InvalidOperationException("A new service provider is not supported");

    protected IServiceProvider ServiceProvider => _serviceProvider ??= ServiceCollection.BuildServiceProvider();
}