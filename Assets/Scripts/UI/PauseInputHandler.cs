using R3;
using UnityEngine;

public class PauseInputHandler : MonoBehaviour
{
    public Observable<bool> OnEscapePressed => _escapeSubject;
    private readonly Subject<bool> _escapeSubject = new Subject<bool>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _escapeSubject.OnNext(true);
    }

    private void OnDestroy()
    {
        _escapeSubject.Dispose();
    }
}