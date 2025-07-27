using System;
using System.Collections.Generic;

public class DisposerContainer : IDisposable
{
    private IList<IDisposable> _list = new List<IDisposable>();
    public void Add(IDisposable d) => _list.Add(d);
    public void Dispose()
    {
        foreach (var d in _list) d.Dispose();
        _list.Clear();
    }
}
