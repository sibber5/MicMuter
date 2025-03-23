using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicMuter;

public sealed class LazyService<T>(IServiceProvider provider) where T : notnull
{
    private T? _service;
    public T Value => _service ??= provider.GetRequiredService<T>();
}
