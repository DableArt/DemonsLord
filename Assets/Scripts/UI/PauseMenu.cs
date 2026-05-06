using R3;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private PauseInputHandler inputHandler;

    private PauseMenuModel _model;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private void Start()
    {
        if (pauseMenuCanvas == null)
        {
            Debug.LogError("PauseMenu: pauseMenuCanvas is not assigned!", this);
            enabled = false;
            return;
        }

        if (inputHandler == null)
        {
            Debug.LogError("PauseMenu: inputHandler is not assigned!", this);
            enabled = false;
            return;
        }

        _model = new PauseMenuModel();

        // Подписка на нажатие Esc — переключаем состояние паузы
        inputHandler.OnEscapePressed
            .Subscribe(_ => TogglePause())
            .AddTo(_disposables);

        // Реактивная привязка состояния к Canvas
        _model.IsPaused
            .Subscribe(isPaused =>
            {
                pauseMenuCanvas.SetActive(isPaused);
                Time.timeScale = isPaused ? 0f : 1f;
            })
            .AddTo(_disposables);

        // Убедиться, что меню закрыто при старте
        pauseMenuCanvas.SetActive(false);
    }

    private void TogglePause()
    {
        _model.IsPaused.Value = !_model.IsPaused.Value;
    }

    public void ResumeGame()
    {
        _model.IsPaused.Value = false;
    }

    public void PauseGame()
    {
        _model.IsPaused.Value = true;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (_model != null && _model.IsPaused.Value)
            Time.timeScale = 1f;

        _disposables.Dispose();
        _model?.Dispose();
    }
}
