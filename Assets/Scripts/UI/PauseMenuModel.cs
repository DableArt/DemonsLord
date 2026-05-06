using R3;
using System;

public class PauseMenuModel : IDisposable
{
    public ReactiveProperty<bool> IsPaused { get; } = new ReactiveProperty<bool>(false);

    public void Dispose()
    {
        IsPaused.Dispose();
    }
}
