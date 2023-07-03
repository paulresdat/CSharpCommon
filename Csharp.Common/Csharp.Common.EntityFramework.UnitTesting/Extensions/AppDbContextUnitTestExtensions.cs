using System.Linq.Expressions;
using Csharp.Common.EntityFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Csharp.Common.EntityFramework.UnitTesting.Extensions;

public static class AppDbContextUnitTestExtensions
{
    public static Mock<TInterfaceDbContext> AddDbSet<T, TInterfaceDbContext>(
        this Mock<TInterfaceDbContext> dbContext,
        Expression<Func<TInterfaceDbContext, DbSet<T>>> predicate, IQueryable<T> data)
        where TInterfaceDbContext: class, IAppDbContext
        where T: class
    {
        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        dbContext.Setup(predicate).Returns(mockDbSet.Object);
        return dbContext;
    }
}
