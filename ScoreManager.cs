using UnityEngine;
using TMPro;
using SavingSystem;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour, ISavable
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scorePrefix = "Score: ";
    
    private int _currentScore = 0;
    
    public int CurrentScore => _currentScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("✅ ScoreManager inicializado");
    }

    private void Start()
    {
        UpdateScoreUI();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scoreText == null)
        {
            FindScoreText();
        }
        UpdateScoreUI();
    }
    
    private void FindScoreText()
    {
        var allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.name.ToLower().Contains("score"))
            {
                scoreText = text;
                Debug.Log($"Texto de score encontrado: {text.name}");
                break;
            }
        }
    }

    public void AddScore(int points)
    {
        _currentScore += points;
        
        if (_currentScore < 0)
        {
            _currentScore = 0;
        }
        
        Debug.Log($"Score: {_currentScore} ({(points >= 0 ? "+" : "")}{points})");
        
        UpdateScoreUI();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckSceneTransition(_currentScore);
        }
    }
    
    public void ResetScore()
    {
        _currentScore = 0;
        UpdateScoreUI();
        Debug.Log("Score reiniciado");
    }
    
    public void SetScore(int newScore)
    {
        _currentScore = newScore;
        if (_currentScore < 0) _currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + _currentScore.ToString();
        }
    }

    public void Save(ref GameData gameData)
    {
        gameData.totalScore = _currentScore;
        gameData.currentScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"Score guardado: {_currentScore}");
    }
    
    public void Load(ref GameData gameData)
    {
        _currentScore = gameData.totalScore;
        UpdateScoreUI();
        
        Debug.Log($"Score cargado: {_currentScore}");
    }
}