using R3;
using UnityEngine;

/// <summary>
/// Слушает нажатие Escape и публикует событие через Observable.
/// </summary>
public class PauseInputHandler : MonoBehaviour
{
    // Внешние подписчики могут слушать это событие
    public Observable<Unit> OnEscapePressed => _escapeSubject;

    private readonly Subject<Unit> _escapeSubject = new Subject<Unit>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _escapeSubject.OnNext(Unit.Default);
    }

    private void OnDestroy()
    {
        _escapeSubject.Dispose();
    }
}
