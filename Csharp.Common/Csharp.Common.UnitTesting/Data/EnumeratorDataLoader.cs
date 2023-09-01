using System.Collections;

namespace Csharp.Common.UnitTesting.Data;

public interface IEnumeratorFileLoader
{
    IEnumeratorFileLoader ReadFromFile(params string[] fullFileName);
    IEnumerator<object[]> ForeachLineInFile(Func<string, List<object>> runThisAction);
}

/// <summary>
/// Enumerator Data Loader
///
/// This class helps one transfer a larger test case theory (that has a lot of inline data) to
/// a class data structure.  This can be helpful, more so than say an inline data structure
/// because it keeps your tests readable without as much boiler code around the test data.
///
/// This class takes it a step further by putting the test data into a text file taking out
/// the code from the file.  If your test cases get too big, it can mess with your IDE and
/// this can also hinder productivity around writing unit tests in that file.  You can use
/// this class extend your class data structure with in your unit test to easily parse a
/// text file into an inline test case in xUnit.  Documentation will be provided at a later
/// date.
/// </summary>
public abstract class EnumeratorDataLoader : IEnumeratorFileLoader, IEnumerable<object[]>
{
    public abstract IEnumerator<object[]> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private List<string> _fileContent = new();
    public IEnumeratorFileLoader ReadFromFile(params string[] fullFileName)
    {
        _fileContent.Clear();
        _fileContent.AddRange(File.ReadAllLines(Path.Join(fullFileName)));
        return this;
    }

    public IEnumerator<object[]> ForeachLineInFile(Func<string, List<object>> runThisAction)
    {
        foreach (var line in _fileContent)
        {
            yield return runThisAction(line).ToArray();
        }
    }

    protected T ToEnum<T>(string enumType)
    {
        if (Enum.TryParse(typeof(T), enumType, true, out var enumResult))
        {
            return (T)enumResult;
        }

        throw new InvalidOperationException(
            "Invalid enum type found for called type: '" + typeof(T).Name + "', value: '" + enumType + "'");
    }

    protected T? ToNullableEnum<T>(string enumType)
    {
        if (Enum.TryParse(typeof(T), enumType, true, out var enumResult))
        {
            return (T)enumResult;
        }

        return default;
    }

    protected bool TrueFalse(string data)
    {
        return data == "true";
    }

    protected int ToInt(string data)
    {
        int.TryParse(data, out var intData);
        return intData;
    }

    protected long ToLong(string data)
    {
        long.TryParse(data, out var longData);
        return longData;
    }
}