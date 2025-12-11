using System;

[Serializable]
public class GameData
{
    // Score global (persiste entre escenas)
    public int totalScore;
    
    // Health de la torreta
    public float turretCurrentHealth;
    public float turretMaxHealth;
    
    // Tracking de progreso
    public string currentScene;
    public bool hasCompletedMeteorScene;
}