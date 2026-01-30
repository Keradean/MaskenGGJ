using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// ==================================================
// GAME MANAGER CLASS
// ==================================================
// This is the main Game Manager that controls high-level game logic
// It inherits from Singleton<GameManager> so only ONE instance exists in the game
// This can be accessed from anywhere with: GameManager.Instance
//
// The GameManager holds references to important game objects (like the Player)
// and provides centralized methods for game-wide actions (like adding experience)
public class GameManager : Singleton<GameManager>
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    // Internal variable for PlayerHealth (currently not used in the code)
    // "internal" means it can be accessed by other scripts in the same assembly
    // "object" is a very generic type - should probably be PlayerHealth type instead
    internal object PlayerHealth;

    // Reference to the main Player object in the scene
    [SerializeField] private Player player;

    // Public property to access the player instance from other scripts
    // The => syntax creates a read-only property that returns the player reference
    // Other scripts can use: GameManager.Instance.Player to get the player
    public Player Player => player; // Public property to access the player instance

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Currently only used for testing/debugging purposes
    /// <summary>
    /// Test Only
    /// </summary>
    private void Update()
    {
        // Testing shortcut: Press L key to reset player stats
        // This is useful during development to quickly reset health, stamina, etc.
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Call the ResetStats method on the player instance
            // This resets health to max, stamina to max, and other stats to default values
            player.ResetStats(); // Call the ResetStats method on the player instance
        }
    }
}