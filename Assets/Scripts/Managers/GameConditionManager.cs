using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using KingdomClash.UI;

namespace KingdomClash
{
    /// <summary>
    /// Manages game win/loss conditions and end game states
    /// </summary>
    public class GameConditionManager : MonoBehaviour
    {
        // Singleton instance
        public static GameConditionManager Instance { get; private set; }
        
        [Header("Game Condition Settings")]
        [SerializeField] private string playerCastleTag = "PlayerCastle";
        [SerializeField] private string enemyCastleTag = "EnemyCastle";        [Header("UI References")]
        [SerializeField] private GameOverPanel gameOverPanel;
        
        // Track if game is over
        private bool isGameOver = false;
        
        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
          private void Start()
        {
            // Start checking for win/loss conditions
            StartCoroutine(CheckGameConditions());
        }
        
        /// <summary>
        /// Continuously check for win/loss conditions
        /// </summary>
        private IEnumerator CheckGameConditions()
        {
            // Wait a moment for the game to initialize
            yield return new WaitForSeconds(2f);
            
            while (!isGameOver)
            {
                // Check for player castle
                GameObject playerCastle = GameObject.FindGameObjectWithTag(playerCastleTag);
                
                // Check for enemy castle
                GameObject enemyCastle = GameObject.FindGameObjectWithTag(enemyCastleTag);
                
                // Player loses if their castle is destroyed
                if (playerCastle == null)
                {
                    EndGame(false); // Player lost
                    break;
                }
                
                // Player wins if enemy castle is destroyed
                if (enemyCastle == null)
                {
                    EndGame(true); // Player won
                    break;
                }
                
                // Check every second
                yield return new WaitForSeconds(1f);
            }
        }
          /// <summary>
        /// End the game with either a win or loss
        /// </summary>
        /// <param name="isVictory">True if player won, false if player lost</param>
        public void EndGame(bool isVictory)
        {
            if (isGameOver)
                return;
                
            isGameOver = true;
            
            // Show the game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(isVictory);
            }
            else
            {
                // No panel available, just pause the game
                Time.timeScale = 0f;
                Debug.LogWarning("GameOverPanel not assigned in GameConditionManager!");
            }
            
            Debug.Log(isVictory ? "Player Victory!" : "Player Defeat!");
        }
    }
}
