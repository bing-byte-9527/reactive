﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TSource> Intersect<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second) =>
            Intersect(first, second, comparer: null);

        public static IAsyncEnumerable<TSource> Intersect<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {
            if (first == null)
                throw Error.ArgumentNull(nameof(first));
            if (second == null)
                throw Error.ArgumentNull(nameof(second));

#if HAS_ASYNC_ENUMERABLE_CANCELLATION
            return Core();

            async IAsyncEnumerable<TSource> Core([System.Runtime.CompilerServices.EnumeratorCancellation]CancellationToken cancellationToken = default)
#else
            return Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
#endif
            {
                var set = new Set<TSource>(comparer);

                await foreach (var element in second.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    set.Add(element);
                }

                await foreach (var element in first.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    if (set.Remove(element))
                    {
                        yield return element;
                    }
                }
            }
        }
    }
}
