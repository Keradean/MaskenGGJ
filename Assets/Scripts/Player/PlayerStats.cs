using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player Stats")] // Create a new PlayerStats asset
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    public float Health; // Player health
    public float MaxHealth; // Maximum player health

    [Header("Stamina")]
    public float Stamina; // Player Stamina
    public float MaxStamina; // Maximum player Stamina

    [Header("Score Values")]
    public int EnemyApeScore; 
    public int FoxScore; 
    public int BossScore; 
    public int Score;

  
    public void AddScore(int points)
    {
        Score += points;
    }

    public void ResetStats()
    {
        Health = MaxHealth; // Reset health to maximum health
        Stamina = MaxStamina; // Reset stamina to maximum stamina
        Score = 0;
    }
}
