[![Build status](https://ci.appveyor.com/api/projects/status/bvk29vnl7o7tna9s?svg=true)](https://ci.appveyor.com/project/mcai4gl2/kickstart-net)
[![NuGet Status](http://img.shields.io/nuget/v/KickStart.Net.svg?style=flat)](https://www.nuget.org/packages/KickStart.Net/)

# KickStart.Net

KickStart is a collection of useful reusable functions which are too small to be have its own library.

## KickStart.Net.dll

* `Objects.ToStringHelper` a port of Guava's `MoreObjects.ToStringHelper` which is helpful to implement `ToString` method 
* `TimeUnits` is a port of Java's `java.util.concurrent.TimeUnit` originally written by Doug Lea as part of JSR-166
* `Optional<T>` is an immutable object that may contain a non-null reference of type `T`. This ports Java's `java.util.Optional`
* `Trace.Here()` returns a trace object which contains the line number, method name and file name where the method is called

### Collections
* `Combinations` generates cartesian product based on inputs, e.g.:
```C#
var combinations = new Combinations<int>(new[] {1, 2, 3}, new[] {2, 3, 4});
/// Generates list of: [[1,2],[1,3],[1,4],[2,2],[2,3],[2,4],[3,2],[3,3],[3,4]]
```
* `HashBasedTable` implements `ITable`, which is a collection which associates an ordered pair of keys with a value. This implementation is based on nested `Dictionary` and it is not multi thread safe.

### Cache
A port of Guava Cache into C#. More details at [here](src/KickStart.Net/Cache/README.md).

### Metrics
A C# port of Coda Hale's Metrics library [https://github.com/dropwizard/metrics](https://github.com/dropwizard/metrics).

### Extensions

##### Collection
* `AddRange` adds multiple items to a `ICollection<T>`
* `SetRange` adds or replaces multiple items in a `ICollection<T>` or `IList<T>`
* `RemoveRange` removes multiple keys from a dictionary
* `RemoveByValue` removes keys from a dictionary by a value
* `GetOrDefault` gets value from dictionary, returns the default value if key is not found
* `ToHashSet` adds all the item in an `IEnumerable<T>` into a new `HashSet<T>`
* `IndexOf` to find the index of an item in a list using an `IEqualityComparer<T>`
* `ToDelimitedString` to convert some items into a delimited string
* `IndexBy` to convert `IEnumerable<T>` into a new `Dictionary<K, T>` keyed by key selector `Func<T, K>`
* `IsEmpty` and `IsNotEmpty` checks if a `IList` is empty or not
* `Split` splits a `IList` based on a `Predicate` 
* `All<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)` extends `All` to access index
* `Any<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)` extends `Any` to access index
* `Parse` returns items from the IEnumerable<string> that can be transformed into a different type via a delegate, e.g. `int.TryParse`
* `Attempt` returns a sequence of a result, where each result can be the result of applying a function, or the exception that occurred when applying the function
* `SafeContainsKey`, `SafeRemove`, `SafeGet`, `SafeSet`, `SafeAdd` extends `IDictionary<TK, TV>` to apply a safe operation without throwing exceptions

##### Stream
* `ToStream` converts from `string` or `byte[]` into `stream`
* `StreamToString` converts from `stream` to `string`

##### Print
* `Print` or `P` to print out the object in console

##### Task
* `ToListAsync` awaits a task which returns `IEnumerable<T>` and returns `List<T>`
* `TimeoutAfter` awaits a task to return or timeout passed
* `DontWait` indicates don't wait for completion of the task is intentionally
* `ContinueWhenCancelled` ignores `TaskCancelledException` when task is cancelled 

##### TaskFactory
* `Schedule` executes a one-shot action after a given delay
* `ScheduleAtFixedRate` executes a periodic action firstly after the given initial delay and subsequently with the given period
* `ScheduleAtFixedDelay` executes a periodic action firstly after the given initial delay and subsequently with the given delay