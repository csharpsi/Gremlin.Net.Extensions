using System;

namespace Gremlin.Net.Extensions
{
    internal static class ValidationExtensions
    {
        internal static void ThrowIfNull<TInput>(this TInput input, string name) where TInput : class => _ = input ?? throw new ArgumentNullException(name);
    }
}