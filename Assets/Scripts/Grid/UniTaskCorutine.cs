using Cysharp.Threading.Tasks;
using System;
using System.Threading;

/// <summary>
/// Manages a cancellable UniTask-based coroutine, similar to Unity's Coroutine but with cancellation/token control.
/// </summary>
public class UniTaskCoroutine : IDisposable
{
    private CancellationTokenSource _cts;
    private Func<CancellationToken, UniTask> _asyncDelegate;
    private UniTask _runningTask = UniTask.CompletedTask;

    /// <summary>
    /// Indicates whether the coroutine is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    private bool _disposed;

    /// <summary>
    /// Constructs a new UniTaskCoroutine wrapping the given asynchronous delegate.
    /// </summary>
    /// <param name="asyncDelegate">A function taking a CancellationToken and returning a UniTask.</param>
    public UniTaskCoroutine(Func<CancellationToken, UniTask> asyncDelegate)
    {
        _asyncDelegate = asyncDelegate ?? throw new ArgumentNullException(nameof(asyncDelegate));
    }

    /// <summary>
    /// Starts the coroutine if not already running.
    /// </summary>
    public void Run()
    {
        ThrowIfDisposed();
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = RunInternalAsync();
    }

    /// <summary>
    /// Starts the coroutine and awaits its completion.
    /// </summary>
    public async UniTask RunAsync()
    {
        ThrowIfDisposed();
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = RunInternalAsync();
        await _runningTask;
    }

    /// <summary>
    /// Internal routine that runs and completes the provided UniTask, handling cancellation and exceptions.
    /// </summary>
    private async UniTask RunInternalAsync()
    {
        try
        {
            await _asyncDelegate.Invoke(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected on manual cancellation via token
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            throw;
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Stop the coroutine and awaits its completion if running (async).
    /// </summary>
    public async UniTask StopAsync()
    {
        ThrowIfDisposed();
        if (!IsRunning)
            return;
        _cts?.Cancel();
        try
        {
            await _runningTask;
        }
        catch (OperationCanceledException)
        {
            // Ignored: normal cancellation
        }
    }

    /// <summary>
    /// Stop the coroutine immediately (does not await completion).
    /// </summary>
    public void Stop()
    {
        ThrowIfDisposed();
        if (!IsRunning)
            return;
        _cts?.Cancel();
    }

    /// <summary>
    /// Disposes the coroutine, cancels the task, and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this object has already been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UniTaskCoroutine));
    }

    /// <summary>
    /// Handles actual disposal logic.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (IsRunning)
            {
                // Safely cancel the running task if necessary
                try
                {
                    _cts?.Cancel();
                }
                catch { }
            }
            _cts?.Dispose();
            _cts = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Destructor (finalizer) to ensure disposal.
    /// </summary>
    ~UniTaskCoroutine()
    {
        Dispose(false);
    }
}

/// <summary>
/// Same as UniTaskCoroutine, but supports passing an argument on Run/RunAsync.
/// </summary>
public class UniTaskCoroutine<TArg> : IDisposable
{
    private CancellationTokenSource _cts;
    private Func<TArg, CancellationToken, UniTask> _asyncDelegate;
    private UniTask _runningTask = UniTask.CompletedTask;

    private bool _disposed;

    /// <summary>
    /// Indicates whether the coroutine is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Constructs a new UniTaskCoroutine accepting one argument for the handler.
    /// </summary>
    /// <param name="asyncDelegate">A function taking TArg and CancellationToken, returning UniTask.</param>
    public UniTaskCoroutine(Func<TArg, CancellationToken, UniTask> asyncDelegate)
    {
        _asyncDelegate = asyncDelegate ?? throw new ArgumentNullException(nameof(asyncDelegate));
    }

    /// <summary>
    /// Starts the coroutine with the given argument if not already running.
    /// </summary>
    public void Run(TArg arg)
    {
        ThrowIfDisposed();
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = RunInternalAsync(arg);
    }

    /// <summary>
    /// Starts the coroutine with the argument and awaits its completion.
    /// </summary>
    public async UniTask RunAsync(TArg arg)
    {
        ThrowIfDisposed();
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = RunInternalAsync(arg);
        await _runningTask;
    }

    /// <summary>
    /// Internal routine that runs the UniTask, handling argument passing and cancellation.
    /// </summary>
    private async UniTask RunInternalAsync(TArg arg)
    {
        try
        {
            await _asyncDelegate.Invoke(arg, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            throw;
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Stops the coroutine and awaits its completion if running (async).
    /// </summary>
    public async UniTask StopAsync()
    {
        if (!IsRunning)
            return;
        _cts?.Cancel();
        try
        {
            await _runningTask;
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
    }

    /// <summary>
    /// Stops the coroutine immediately (does not await completion).
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;
        _cts?.Cancel();
    }

    /// <summary>
    /// Disposes the coroutine, cancels the task, and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles the actual disposal logic, including cancellation and resource cleanup.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (IsRunning)
            {
                // Safely cancel the running task if necessary
                try
                {
                    _cts?.Cancel();
                }
                catch { }
            }
            _cts?.Dispose();
            _cts = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this object has already been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UniTaskCoroutine<TArg>));
    }

    /// <summary>
    /// Finalizer to ensure cleanup in case Dispose was not called.
    /// </summary>
    ~UniTaskCoroutine()
    {
        Dispose(false);
    }
}
