[![Build Status](https://travis-ci.org/mcai4gl2/KickStart.Net.svg)](https://travis-ci.org/mcai4gl2/KickStart.Net)
[![NuGet Status](http://img.shields.io/nuget/v/KickStart.Net.svg?style=flat)](https://www.nuget.org/packages/KickStart.Net/)

# KickStart.Net

KickStart is a collection of useful resuable functions which are too small to be have its own library.

## KickStart.Net.dll

### CollectionExtensions
* `AddRange` adds multiple items to a `ICollection<T>`
* `SetRange` adds or replaces multiple items in a `ICollection<T>` or `IList<T>`
* `RemoveRange` removes multiple keys from a dictionary
* `RemoveByValue` removes keys from a dictionary by a value
* `GetOrDefault` gets value from dictionary, returns the default value if key is not found
* `ToHashSet` adds all the item in an `IEnumerable<T>` into a new `HashSet<T>`
* `IndexOf` to find the index of an item in a list using an `IEqualityComparer<T>`
* `ToDelimitedString` to convert some items into a delmited string
* `IndexBy` to convert `IEnumerable<T>` into a new `Dictionary<K, T>` keyed by key selector `Func<T, K>`

### StreamExtensions
* `ToStream` converts from `string` or `byte[]` into `stream`
* `StreamToString` converts from `stream` to `string`

### PrintExtensions
* `Print` or `P` to print out the object in console

### TaskExtensions
* `ToListAsync` awaits a task which returns `IEnumerable<T>` and returns `List<T>`
* `TimeoutAfter` awaits a task to return or timeout passed
* `DontWait` indicates don't wait for completion of the task is intentionally
* `ContinueWhenCancelled` ignores `TaskCancelledException` when task is cancelled 

### HttpMessageExtensions
* `WithContent` adds request content to a `HttpRequestMessage`
* `ContentAsStream` reads response content from `HttpResponseMessage` and return as `Stream`

### Diagnostic
* `Trace.Here()` returns a trace object which contains the line number, method name and file name where the method is called
