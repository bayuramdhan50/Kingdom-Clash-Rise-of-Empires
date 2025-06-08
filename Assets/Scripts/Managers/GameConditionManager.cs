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
        [SerializeField] private string enemyCastleTag = "EnemyCastle";
        [Header("UI References")]
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
            
            // Periksa keberadaan castle di awal permainan
            bool playerCastleExisted = false;
            bool enemyCastleExisted = false;
            
            GameObject initialPlayerCastle = GameObject.FindGameObjectWithTag(playerCastleTag);
            GameObject initialEnemyCastle = GameObject.FindGameObjectWithTag(enemyCastleTag);
            
            // Tandai jika castle ada di awal permainan
            if (initialPlayerCastle != null) playerCastleExisted = true;
            if (initialEnemyCastle != null) enemyCastleExisted = true;
            
            Debug.Log($"Initial check: Player Castle exists: {playerCastleExisted}, Enemy Castle exists: {enemyCastleExisted}");
            
            // Jika tidak ada castle sama sekali, tampilkan warning dan hentikan pengecekan
            if (!playerCastleExisted && !enemyCastleExisted)
            {
                Debug.LogWarning("No castles found in the scene! Win/loss conditions will not be checked.");
                yield break;
            }
            
            while (!isGameOver)
            {
                // Check for player castle
                GameObject playerCastle = GameObject.FindGameObjectWithTag(playerCastleTag);
                
                // Check for enemy castle
                GameObject enemyCastle = GameObject.FindGameObjectWithTag(enemyCastleTag);
                
                // Player loses if their castle is destroyed (tetapi hanya jika sebelumnya castle pemain ada)
                if (playerCastleExisted && playerCastle == null)
                {
                    EndGame(false); // Player lost
                    break;
                }
                
                // Player wins if enemy castle is destroyed (tetapi hanya jika sebelumnya castle musuh ada)
                if (enemyCastleExisted && enemyCastle == null)
                {
                    EndGame(true); // Player won
                    break;
                }
                
                // Check every second
                yield return new WaitForSeconds(1f);
            }
        }        /// <summary>
        /// End the game with either a win or loss
        /// </summary>
        /// <param name="isVictory">True if player won, false if player lost</param>
        public void EndGame(bool isVictory)
        {
            if (isGameOver)
                return;
                
            // Pengecekan tambahan - apakah castle ditemukan di game?
            // Jika castle tidak ditemukan sejak awal, tidak perlu menampilkan panel game over
            GameObject playerCastle = GameObject.FindGameObjectWithTag(playerCastleTag);
            GameObject enemyCastle = GameObject.FindGameObjectWithTag(enemyCastleTag);
            
            // Jika kedua castle tidak ada sejak awal, mungkin tag salah atau castle tidak ada di scene
            if (playerCastle == null && enemyCastle == null)
            {
                Debug.LogWarning("No castles were found in scene! Game over state cannot be properly determined.");
                return;
            }
                
            isGameOver = true;
            
            // Show the game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(isVictory);
                Debug.Log(isVictory ? "Player Victory!" : "Player Defeat!");
            }
            else
            {
                // No panel available, just pause the game
                Time.timeScale = 0f;
                Debug.LogWarning("GameOverPanel not assigned in GameConditionManager!");
            }
        }
    }
}
