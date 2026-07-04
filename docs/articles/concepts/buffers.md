# Buffers and pooled strings

`SquidStd.Core.Buffers` and `SquidStd.Core.Extensions.Strings` are a small, dependency-free toolkit for allocation-sensitive string and byte work: a single-threaded array pool, a stack-friendly string builder, and a family of ordinal/case-insensitive string helpers that skip culture-aware comparisons entirely.

## When to reach for these types

Reach for this toolkit on hot paths that would otherwise allocate a `char[]` or `byte[]` on every call: parsers, network frame encoders/decoders, per-tick game loop work, or any single-threaded pipeline that runs often enough for GC pressure to matter. For one-off, cold-path string work, `string`, `StringBuilder`, and `ArrayPool<T>.Shared` are simpler and perfectly fine.

## STArrayPool

`STArrayPool<T>` is a single-threaded adaptation of the .NET runtime's shared `ArrayPool<T>` (`TlsOverPerCoreLockedStacksArrayPool`). It keeps the same bucketing scheme - powers of two, from 16 elements up to roughly 1 GiB - and the same rent/return API, but drops every lock: there is no per-core or thread-local synchronization at all.

> [!WARNING]
> `STArrayPool<T>.Shared` is NOT thread-safe. It performs no locking, so renting or returning from more than one thread at a time is a race - it can hand out the same array twice or corrupt its internal buckets. Only use it from code that already guarantees exclusive, single-threaded access.

Use the decision table to pick the right pool:

| Scenario | Use |
|---|---|
| A single-threaded hot path that owns its buffer exclusively (a parser, formatter, or one tick of a game loop) | `STArrayPool<T>.Shared` |
| Any code that might run concurrently, or where you can't prove exclusive ownership | `ArrayPool<T>.Shared` |

`STArrayPool<T>` trims itself automatically: the first time it caches a returned array it registers a Gen2 GC callback, and every Gen2 collection runs `Trim()`. `Trim()` reads `GC.GetGCMemoryInfo()` and classifies the current memory load into `Low`, `Medium`, or `High` pressure (at or above 70% and 90% of the high memory-load threshold). Under `High` pressure the pool clears its single-slot fast-path cache immediately and ages arrays out of the per-size overflow stacks after only 10 seconds. Under `Medium` and `Low` pressure it is gentler: the fast-path cache ages out after 10 or 30 seconds of sitting idle, and the overflow stacks age out after 60 seconds. This keeps a short burst of activity from being trimmed away between GCs while still giving memory back once the process is actually under pressure.

`Return` does not clear the array's contents unless you pass `clearArray: true`. For arrays of a reference type, that means previously stored object references stay reachable through the pool until the array is rented out again (and overwritten) or discarded by a trim - pass `clearArray: true` when that matters.

```csharp
using SquidStd.Core.Buffers;

var array = STArrayPool<byte>.Shared.Rent(1024);
try
{
    // use array...
}
finally
{
    STArrayPool<byte>.Shared.Return(array);
}
```

## ValueStringBuilder

`ValueStringBuilder` is a `ref struct` string builder backed by a single growable `Span<char>` instead of `StringBuilder`'s linked chunks. Being a ref struct, it must stay on the stack: it cannot be boxed, stored in a field of a non-ref struct, or captured by a lambda or async method.

```csharp
using SquidStd.Core.Buffers;

using var builder = new ValueStringBuilder(stackalloc char[64]);

builder.Append("hello ");
builder.Append($"world {42}");

var text = builder.ToString(); // copies the written span into a new string
```

`ToString()` only reads the builder's contents - it does not release the buffer. Wrap the builder in a `using` declaration (as above) so `Dispose()` returns any rented buffer to the pool once you're done; if the builder never grew past its initial `stackalloc` span, `Dispose()` is a harmless no-op.

By default (`mt: false`), the builder rents from `STArrayPool<char>.Shared`, so the same single-threaded rule from the previous section applies: create, append to, and dispose the builder from one thread. Pass `mt: true` when the builder - or a buffer it grew into - might cross threads; this switches renting to `ArrayPool<char>.Shared` instead:

```csharp
var builder = new ValueStringBuilder(64, mt: true);
builder.Append("thread safe pool path");
var text = builder.ToString();
builder.Dispose();
```

## Pooled helpers

`ToPooledArray` copies a string into a buffer rented from `STArrayPool<char>.Shared`. The caller owns the returned array and must return it; the buffer may be longer than the source string, so only the first `str.Length` characters are valid.

```csharp
using SquidStd.Core.Buffers;
using SquidStd.Core.Extensions.Strings;

var array = "hello world".ToPooledArray();
try
{
    // only array[..11] holds the copied characters
}
finally
{
    STArrayPool<char>.Shared.Return(array);
}
```

`PooledArraySpanFormattable` wraps a `STArrayPool<char>`-rented buffer as an `ISpanFormattable`, so it can flow through interpolated string handlers and span-formatting APIs without allocating an intermediate string. `TryFormat` is single-use: a successful call copies the characters into the destination and returns the buffer to the pool as part of that same call, so the wrapper must not be used again afterward. `ToString()` is idempotent instead - the first call caches the string and releases the buffer, and later calls return the same cached instance.

## String helpers

`OrdinalStringHelpers` and `InsensitiveStringHelpers` mirror each other: one comparison family, ordinal (case-sensitive) on one side and ordinal-ignore-case on the other, both skipping culture lookups entirely. Every member works on `string`, and most also have a `ReadOnlySpan<char>` overload.

| Family | Ordinal | Insensitive |
|---|---|---|
| Starts with | `StartsWithOrdinal` | `InsensitiveStartsWith` |
| Ends with | `EndsWithOrdinal` | `InsensitiveEndsWith` |
| Equals | `EqualsOrdinal` | `InsensitiveEquals` |
| Contains | `ContainsOrdinal` | `InsensitiveContains` |
| Compare | `CompareOrdinal` | `InsensitiveCompare` |
| Index of | `IndexOfOrdinal` | `InsensitiveIndexOf` |
| Remove | `RemoveOrdinal` | `InsensitiveRemove` |
| Replace | `ReplaceOrdinal` | `InsensitiveReplace` |

```csharp
using SquidStd.Core.Extensions.Strings;

"hello world".StartsWithOrdinal("hello"); // true, no culture lookup
"Hello".InsensitiveEquals("hELLO");       // true
```

`StringHelpers` rounds out the set with general-purpose utilities:

- `Capitalize` - title-cases every space-separated word, leaving "the " untouched.
- `Wrap` - word-wraps text into lines of at most N characters, hard-breaking words that don't fit on their own line.
- `IndentMultiline` - prefixes every line of a multiline string with an indent.
- `TrimMultiline` - trims every line of a multiline string independently.
- `IndexOfTerminator` - finds a null terminator in a byte/char/uint buffer.
- `ReplaceAny` - replaces every occurrence of a set of characters with their paired replacements, in place only.

## Related

- [SquidStd.Core](../core.md) - the package these types ship in, alongside the rest of the dependency-free helpers.
- [Home](../../index.md) - back to the SquidStd documentation home.
