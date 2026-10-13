using System.Diagnostics.CodeAnalysis;
using Csharp.Common.UnitTesting;
using Csharp.Common.Unsafe;
using Xunit;

namespace Csharp.Common.UnitTests.Unsafe;

[ExcludeFromCodeCoverage]
public class InverseSqrtTests : BaseUnitTest
{
    [Fact(DisplayName = "001 Inverse functions works appropriately")]
    public void T001()
    {
        var f1 = 100.1f;
        f1.FastInverseSqrt();
        f1.BetterInverseSqrt();
    }
}