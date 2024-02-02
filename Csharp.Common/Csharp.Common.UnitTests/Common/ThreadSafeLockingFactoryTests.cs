using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities.ThreadSafety;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests.Common;

public class ThreadSafeLockingFactoryTests : BaseUnitTest
{
    private readonly ITestOutputHelper _output;
    public ThreadSafeLockingFactoryTests(ITestOutputHelper output)
    {
        _output = output;

        ServiceCollection.AddSingleton<IThreadSafeLockingFactory, ThreadSafeLockingFactory>();
        ServiceCollection.AddSingleton<ThreadSafeOnLists>();
    }

    [Fact(DisplayName = "001 Thread Safe Lock works well on a list")]
    public void T001()
    {
        var sp = GetNewServiceProvider;
        var service = sp.GetRequiredService<ThreadSafeOnLists>();

        var task1 = Task.Run(() =>
        {
            for (var i = 0; i <= 5000000; i++)
            {
                // flip between 1 and 2
                for (var j = 0; j < 1; j++)
                {
                    var value = "s" + i;
                    var val = service.WriteOn(j, value);
                    Assert.Equal(value, val);
                }
            }
        });

        // a concurrent task on the same values is also being written in the descending direction
        // values don't get mixed up this way
        var task2 = Task.Run(() =>
        {
            for (var i = 5000000; i >= 0; i--)
            {
                // flip between 1 and 2
                for (var j = 0; j < 1; j++)
                {
                    var value = "s" + i;
                    var val = service.WriteOn(j, value);
                    Assert.Equal(value, val);
                }
            }
        });

        Task.WaitAll(task1, task2);
        
        // however the following is expected to run an aggregation error
        Assert.Throws<AggregateException>(() =>
        {
            var task3 = Task.Run(() =>
            {
                for (var i = 0; i <= 5000000; i++)
                {
                    // flip between 1 and 2
                    for (var j = 0; j < 1; j++)
                    {
                        var value = "s" + i;
                        var val = service.UnsafeWrite(j, value);
                        Assert.Equal(value, val);
                    }
                }
            });

            var task4 = Task.Run(() =>
            {
                for (var i = 5000000; i >= 0; i--)
                {
                    // flip between 1 and 2
                    for (var j = 0; j < 1; j++)
                    {
                        var value = "s" + i;
                        var val = service.UnsafeWrite(j, value);
                        Assert.Equal(value, val);
                    }
                }
            });

            Task.WaitAll(task3, task4);
        });
    }

    private class ThreadLocker : ThreadSafe
    {
        public new void EnterUpgradeableReadLock(Action action)
        {
            base.EnterUpgradeableReadLock(action);
        }

        public new void EnterReadWriteLock(Action action)
        {
            base.EnterReadWriteLock(action);
        }
    }

    [Fact(DisplayName = "002 Thread safe locking allow for upgradable read")]
    public void T002()
    {
        var threadLocker = new ThreadLocker();
        threadLocker.EnterUpgradeableReadLock(() =>
        {
            threadLocker.EnterReadWriteLock(() =>
            {
                _output.WriteLine("Successfully upgraded to write lock");
            });
        });

        Assert.Throws<LockRecursionException>(() =>
        {
            threadLocker.EnterReadWriteLock(() =>
            {
                threadLocker.EnterReadWriteLock(() =>
                {
                    _output.WriteLine("whoops something is wrong");
                });
            });
        });
    }

    private class ThreadSafeOnLists
    {
        private readonly List<SensitiveData> _data = new();
        private readonly IThreadSafeLockingFactory _lockingFactory;

        public ThreadSafeOnLists(IThreadSafeLockingFactory lockingFactory)
        {
            // first populate the list with data
            for (var i = 0; i < 10; i++)
            {
                _data.Add(new SensitiveData("RandomString_" + i));
            }

            _lockingFactory = lockingFactory;
        }

        public string UnsafeRead(int num)
        {
            var key = "RandomString_" + num;
            return _data.First(x => x.Key() == key).Data ?? throw new InvalidOperationException();
        }

        public string UnsafeWrite(int num, string value)
        {
            var key = "RandomString" + "_" + num;
            var obj = _data
                .First(x => x.Key() == key);
            obj.Data = value;
            return obj.Data;
        }

        public string ReadOn(int num)
        {
            return _lockingFactory.EnterReadLock("sensitive", () =>
            {
                return _lockingFactory.EnterReadLock(num, () => UnsafeRead(num));
            });
        }

        public string WriteOn(int num, string value)
        {
            return _lockingFactory.EnterReadLock("sensitive", () =>
            {
                return _lockingFactory.EnterReadWriteLock(num, () => UnsafeWrite(num, value));
            });
        }
    }

    private class SensitiveData : IEquatable<SensitiveData>
    {
        private readonly string _key;
        public string? Data { get; set; }

        private int _accessed = 0;
        public SensitiveData(string data)
        {
            _key = data;
        }

        public string Key()
        {
            _accessed++;
            return _key;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SensitiveData) obj);
        }
        
        // all objects should be hashed to the same linked list in a dictionary
        // simulating a collision
        public override int GetHashCode()
        {
            return 1;
        }
        
        public bool Equals(SensitiveData? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            // only say the KEY is the same
            return _key == other._key;
        }
    }
    
    /// <summary>
    /// Due to the nature of collisions and how the dictionary object works, it's difficult to test accurately
    /// that the thread safe locking factory does not work well with dictionaries.  But this method is useful for finding
    /// hashcode collisions in strings.
    /// </summary>
    /// <param name="numberOfWordsInPairs"></param>
    /// <returns></returns>
    private Dictionary<int, string[]> HashCodeCollisions(int numberOfWordsInPairs)
    {
        var matches = new Dictionary<int, string[]>();
        while (matches.Count < numberOfWordsInPairs)
        {
            var words = new Dictionary<int, string>();
            var i = 0;
            string teststring;
            while (true)
            {
                i++;
                teststring = i.ToString();
                try
                {
                    if (!matches.ContainsKey(teststring.GetHashCode()))
                    {
                        words.Add(teststring.GetHashCode(), teststring);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            var collisionHash = teststring.GetHashCode();
            matches.Add(collisionHash, new string[] { words[collisionHash], teststring });
        }

        return matches;
    }
}