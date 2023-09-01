using Xunit;

namespace Csharp.Common.UnitTesting.Utilities;

/// <summary>
/// Disable Parallelization for xUnit Tests
///
/// There may be an intermittent issue with xUnit and Castle (an internal class in the .net framework)
/// where an exception is thrown because of "Could not load type 'Castles.Proxies...'.  When this happens
/// it's likely because of xUnit's parallel execution in conjunction with a known .net race condition.
///
/// See: https://github.com/castleproject/Core/issues/193
///
/// To fix this, use this class to shutoff parallelization for the offending tests.
///
/// Example:
/// <code>
/// [Collection(nameof(DisableParallelization))]
/// public class MyTestsAreNotParallelAnymore
/// {
///    [Fact(DisplayName="Fake test as an example')]
///    public void T001()
///    {
///      Assert.True(true);
///    }
/// }
/// </code>
/// </summary>
[CollectionDefinition(nameof(DisableParallelization), DisableParallelization = true)]
public class DisableParallelization
{

}
