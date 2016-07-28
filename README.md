[![Build Status](https://travis-ci.org/mcai4gl2/KickStart.Net.svg)](https://travis-ci.org/mcai4gl2/KickStart.Net)

# KickStart.Net

KickStart is a collection of useful resuable functions which are too small to be have its own library.

## KickStart.Net.dll

### CollectionExtensions
* `RemoveRange` removes multiple keys from a dictionary
* `RemoveByValue` removes keys from a dictionary by a value
* `GetOrDefault` gets value from dictionary, returns the default value if key is not found
* `AddRange` adds multiple items to a `ICollection<T>`
* `SetRange` adds or replaces multiple items in a `ICollection<T>` or `IList<T>`
* `ToHashSet` adds all the item in an `IEnumerable<T>` into a new `HashSet<T>`

### StreamExtensions
* `ToStream` converts from `string` or `byte[]` into `stream`
* `StreamToString` converts from `stream` to `string`

### PrintExtensions
* `Print` or `P` to print out the object in console

## Diagnostic
* `Trace.Here()` returns a trace object which contains the line number, method name and file name where the method is called
