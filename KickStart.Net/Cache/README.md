# Cache

We port [Guava cache](https://github.com/google/guava/wiki/CachesExplained) from Java into C#.

The following features in Guava cache are not supported currently:
* `WeakReference` is not supported. Guava implementation leverages Java `WeakReference`'s [ReferenceQueue](https://docs.oracle.com/javase/7/docs/api/java/lang/ref/WeakReference.html) feature for expiration. This however is not supported in C#. Alternatively, in C#, we would need to loop through the segment to check if WeakReference is expired or not. This will be implemeneted in later versions.
* `SoftReference` and `PhantomReference ` are not supported as there is no equivalent in C#.
* Cache serialization is not supported in this version.

## Example Usage
```C#
var cache = CacheBuilder<string, string>.NewBuilder()
                .WithExpireAfterAccess(TimeSpan.FromMilliseconds(1))
                .WithTicker(ticker)
                .WithRemovalListener(removalListener)
                .RecordStats()
                .Build(customLoader);
```

## Internals

## Future Enhancements