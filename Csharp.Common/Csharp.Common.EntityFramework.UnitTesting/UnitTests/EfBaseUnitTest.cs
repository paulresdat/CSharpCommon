using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Sql.Procedures;
using Csharp.Common.Extensions;
using Csharp.Common.UnitTesting.UnitTests;
using Moq;

namespace Csharp.Common.EntityFramework.UnitTesting.UnitTests;

public class EfBaseUnitTest : BaseSingleServiceProviderUnitTesting
{
    /// <summary>
    /// Mocks an injectable IEntityProcedure method.  This also automates the bootstrapped code, you just need
    /// the data structure and it'll create one automatically for you.
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException"></exception>
    protected void MockEntityProcedure<T>(IQueryable<T> data) where T : class
    {
        var inst = (Mock<IEntityProcedure<T>>?) Activator.CreateInstance(typeof(Mock<IEntityProcedure<T>>)) ?? 
                   throw new InvalidOperationException();
        inst.Setup(x => x.SetProcedure(It.IsAny<string>()))
            .Returns(inst.Object);
        inst.Setup(x => x.SetDbContext(It.IsAny<IAppDbContext>()))
            .Returns(inst.Object);
        inst.Setup(x => x.WithParameters(It.IsAny<object>()))
            .Returns(inst.Object);
        inst.Setup(x => x.AsQueryable<T>())
            .Returns(data);
        ServiceCollection.RefreshSingleton(inst.Object);
    }
}