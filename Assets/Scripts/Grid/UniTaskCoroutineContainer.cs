using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class UniTaskCoroutineContainer : IDisposable
{
    private readonly List<UniTaskCoroutine> _coroutines = new List<UniTaskCoroutine>();
    private readonly DisposerContainer _disposer = new DisposerContainer();
    private bool _disposed;

    /// <summary>
    /// Добавляет корутину в контейнер и регистрирует для автоматического Dispose.
    /// </summary>
    public void Add(UniTaskCoroutine coroutine)
    {
        ThrowIfDisposed();
        if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
        _coroutines.Add(coroutine);
        _disposer.Add(coroutine);
    }

    /// <summary>
    /// Запускает все зарегистрированные корутины.
    /// </summary>
    public void RunAll()
    {
        ThrowIfDisposed();
        foreach (var coroutine in _coroutines)
            coroutine.Run();
    }

    /// <summary>
    /// Асинхронно запускает все зарегистрированные корутины и ждёт их завершения.
    /// </summary>
    public async UniTask RunAllAsync()
    {
        ThrowIfDisposed();
        var tasks = new List<UniTask>();
        foreach (var coroutine in _coroutines)
            tasks.Add(coroutine.RunAsync());
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// Останавливает все зарегистрированные корутины.
    /// </summary>
    public void StopAll()
    {
        ThrowIfDisposed();
        foreach (var coroutine in _coroutines)
            coroutine.Stop();
    }

    /// <summary>
    /// Асинхронно останавливает все зарегистрированные корутины и ждёт их завершения.
    /// </summary>
    public async UniTask StopAllAsync()
    {
        ThrowIfDisposed();
        var tasks = new List<UniTask>();
        foreach (var coroutine in _coroutines)
            tasks.Add(coroutine.StopAsync());
        await UniTask.WhenAll(tasks);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Гарантируем остановку всех корутин перед освобождением ресурсов
            try
            {
                StopAll();
            }
            catch { /* игнорируем ошибки при остановке */ }

            _disposer.Dispose();
            _coroutines.Clear();
        }
        _disposed = true;
    }
    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniTaskCoroutineContainer));
    }

    ~UniTaskCoroutineContainer()
    {
        Dispose(false);
    }
}
public class UniTaskCoroutineContainer<TArg> : IDisposable
{
    private readonly IList<UniTaskCoroutine<TArg>> _coroutines = new List<UniTaskCoroutine<TArg>>();
    private readonly DisposerContainer _disposer = new DisposerContainer();
    private bool _disposed;

    /// <summary>
    /// Добавляет корутину в контейнер и регистрирует для автоматического Dispose.
    /// </summary>
    public void Add(UniTaskCoroutine<TArg> coroutine)
    {
        ThrowIfDisposed();
        if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
        _coroutines.Add(coroutine);
        _disposer.Add(coroutine);
    }

    /// <summary>
    /// Запускает все корутины с одинаковым аргументом.
    /// </summary>
    public void RunAll(TArg arg)
    {
        ThrowIfDisposed();
        foreach (var coroutine in _coroutines)
            coroutine.Run(arg);
    }

    /// <summary>
    /// Асинхронно запускает все корутины с одинаковым аргументом и ждёт завершения каждого старта.
    /// </summary>
    public async UniTask RunAllAsync(TArg arg)
    {
        ThrowIfDisposed();
        var tasks = new List<UniTask>();
        foreach (var coroutine in _coroutines)
            tasks.Add(coroutine.RunAsync(arg));
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// Останавливает все корутины.
    /// </summary>
    public void StopAll()
    {
        ThrowIfDisposed();
        foreach (var coroutine in _coroutines)
            coroutine.Stop();
    }

    /// <summary>
    /// Асинхронно останавливает все корутины и ждёт завершения.
    /// </summary>
    public async UniTask StopAllAsync()
    {
        ThrowIfDisposed();
        var tasks = new List<UniTask>();
        foreach (var coroutine in _coroutines)
            tasks.Add(coroutine.StopAsync());
        await UniTask.WhenAll(tasks);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            try { StopAll(); } catch { }
            _disposer.Dispose();
            _coroutines.Clear();
        }
        _disposed = true;
    }
    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniTaskCoroutineContainer<TArg>));
    }

    ~UniTaskCoroutineContainer()
    {
        Dispose(false);
    }

}
