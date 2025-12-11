using UnityEngine;
using UnityEngine.UI;

public class UIButtonsManager : MonoBehaviour
{
    [Header("Game Control Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    [Header("Optional: Show Messages")]
    [SerializeField] private bool showDebugMessages = true;

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveClicked);
        }
        
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadClicked);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnSaveClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
            
            if (showDebugMessages)
            {
                Debug.Log("Juego guardado!");
            }
        }
    }
    
    private void OnLoadClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGame();
            
            if (showDebugMessages)
            {
                Debug.Log("Juego cargado!");
            }
        }
    }
    
    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}