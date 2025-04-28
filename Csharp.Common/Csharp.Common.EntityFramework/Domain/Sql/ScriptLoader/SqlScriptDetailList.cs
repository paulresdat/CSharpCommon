using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

public class SqlScriptDetailList : IList<SqlScriptDetail>
{
    public SqlScriptDetailList()
    {
    }

    public SqlScriptDetailList(List<SqlScriptDetail> details)
    {
        _sqlScriptDetails = details;
    }

    public SqlScriptDetailList ForEach(Action<SqlScriptDetail> action)
    {
        _sqlScriptDetails.ForEach(action);
        return this;
    }

    private readonly List<SqlScriptDetail> _sqlScriptDetails = new();
    public IEnumerator<SqlScriptDetail> GetEnumerator()
    {
        return _sqlScriptDetails.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(SqlScriptDetail item)
    {
        _sqlScriptDetails.Add(item);
    }

    public void Clear()
    {
        _sqlScriptDetails.Clear();
    }

    public bool Contains(SqlScriptDetail item)
    {
        return _sqlScriptDetails.Contains(item);
    }

    public void CopyTo(SqlScriptDetail[] array, int arrayIndex)
    {
        _sqlScriptDetails.CopyTo(array, arrayIndex);
    }

    public bool Remove(SqlScriptDetail item)
    {
        return _sqlScriptDetails.Remove(item);
    }

    public int Count => _sqlScriptDetails.Count;
    public bool IsReadOnly => false;
    public int IndexOf(SqlScriptDetail item)
    {
        return _sqlScriptDetails.IndexOf(item);
    }

    public void Insert(int index, SqlScriptDetail item)
    {
        _sqlScriptDetails.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _sqlScriptDetails.RemoveAt(index);
    }

    public SqlScriptDetail this[int index]
    {
        get => _sqlScriptDetails[index];
        set => _sqlScriptDetails[index] = value;
    }
}

public static class SqlScriptDetailListExtensions
{
    private static Dictionary<Type, List<object>> _cached = new();
    public static SqlScriptDetailList OverrideOnEntityConfigure<TEntity>(
        this SqlScriptDetailList detailList,
        Action<SqlScriptDetail, EntityTypeBuilder<TEntity>> builder)
        where TEntity : class
    {
        if (!_cached.ContainsKey(typeof(TEntity)))
        {
            _cached[typeof(TEntity)] = new();
        }

        _cached[typeof(TEntity)].Add(builder);
        return detailList;
    }

    public static void ConfigureAllEntities(this SqlScriptDetailList detailList, ModelBuilder modelBuilder)
    {
        detailList.ForEach(sqlScriptDetail =>
        {
            if (_cached.ContainsKey(sqlScriptDetail.ClassType))
            {
                _cached[sqlScriptDetail.ClassType].ForEach(action =>
                {
                    var entityMethod = typeof(ModelBuilder)
                        .GetMethods()
                        .First(x => x.Name == "Entity" && x.ContainsGenericParameters && x.GetParameters().Length > 0);


                    var generic = entityMethod.MakeGenericMethod(sqlScriptDetail.ClassType);

                    generic.Invoke(modelBuilder, new object[] {
                        (object obj) =>
                        {
                            if (action is Delegate actionDelegate)
                            {
                                actionDelegate.Method.Invoke(actionDelegate.Target, new object[]
                                {
                                    sqlScriptDetail, obj
                                });
                            }
                        }
                    });

                });
            }

            if (sqlScriptDetail.IdentifierType == SqlScriptLoader.Types.Views)
            {
                modelBuilder.Entity(sqlScriptDetail.ClassType, e =>
                {
                    e.ToView(sqlScriptDetail.Identifier.ViewName)
                        .HasNoKey();
                });
            }
        });
    }
}
