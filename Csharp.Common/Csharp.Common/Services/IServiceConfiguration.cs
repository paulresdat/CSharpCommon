using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Services;

/// <summary>
/// The IServiceConfiguration has some public facing methods that require documentation:
/// TODO - Add documentation
/// </summary>
public interface IServiceConfiguration
{
    /// <summary>
    /// TODO - add documentation (see concrete) needs examples of use
    /// </summary>
    /// <param name="configurationBuilder"></param>
    void SetAppSettingsBuilder(IServiceCollectionBuilderConfiguration? configurationBuilder);
    /// <summary>
    /// TODO - add documentation (see concrete) needs examples of use
    /// </summary>
    /// <param name="serviceCollection"></param>
    void ConfigureServices(IServiceCollection serviceCollection);
}
