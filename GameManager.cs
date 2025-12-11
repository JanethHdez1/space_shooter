using SavingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Scene Names")]
    [SerializeField] private string meteorSceneName = "Meteoritos";
    [SerializeField] private string enemiesSceneName = "Enemies";
    [SerializeField] private string youWinSceneName = "YouWin";
    [SerializeField] private string gameOverSceneName = "GameOver";
    
    [Header("Score Thresholds")]
    [SerializeField] private int scoreToReachEnemies = 100;
    [SerializeField] private int scoreToWin = 250;
    
    private bool _isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("GameManager inicializado");
    }

    private void Start()
    {
       
    }


    
    public void CheckSceneTransition(int currentScore)
    {
        if (_isTransitioning) return;
        
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Desde escena de meteoritos
        if (currentScene == meteorSceneName && currentScore >= scoreToReachEnemies)
        {
            Debug.Log($"{scoreToReachEnemies} puntos alcanzados! Cargando escena de enemigos...");
            LoadScene(enemiesSceneName, saveBefore: false); 
        }
        // Desde escena de enemigos
        else if (currentScene == enemiesSceneName && currentScore >= scoreToWin)
        {
            Debug.Log($"{scoreToWin} puntos alcanzados! ¡Victoria!");
            LoadScene(youWinSceneName, saveBefore: false);
        }
    }
    
    public void OnTurretDeath()
    {
        if (_isTransitioning) return;
        
        Debug.Log("Torreta destruida! Game Over");
        LoadScene(gameOverSceneName, saveBefore: false);
    }
    
  
    private void LoadScene(string sceneName, bool saveBefore = false)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        
        if (saveBefore)
        {
            SaveLoadManager.SaveData();
            Debug.Log("Guardado forzado antes de cambiar de escena");
        }
        
        SceneManager.LoadScene(sceneName);
        _isTransitioning = false;
    }


    public void RestartGame()
    {
        SaveLoadManager.DeleteSaveData();
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        SceneManager.LoadScene(meteorSceneName);
        
        Debug.Log("Juego reiniciado");
    }
    
    public void SaveGame()
    {
        SaveLoadManager.SaveData();
        Debug.Log("Juego guardado manualmente");
    }
    
    public void LoadGame()
    {
        SaveLoadManager.LoadData();
        Debug.Log("Juego cargado manualmente");
    }
    
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
