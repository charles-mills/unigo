using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Solo.MOST_IN_ONE;

namespace Gameplay
{
    public enum GameState
    {
        Win,
        Lose,
        PlayerTurn,
        EnemyTurn,
        Start
    }


    public class GameLoop : MonoBehaviour
    {
        public GameObject player;
        public GameObject enemy;

        public float counter;
        public GameState currentGameState;
        public int diceRoll;

        public TextMeshProUGUI diceRollText;
        public UiController uiController;

        private Animator playerAnimator;
        private Animator enemyAnimator;

        private bool isTurnExecuting;
        
        public float playerHealth;
        public float enemyHealth;

        public CastEffects castEffects;

        /// <summary>
        /// A multiplier to easily extend or reduce the average length of a game.
        /// </summary>
        [SerializeField] private float healthMultiplier = 1.0f;
        [SerializeField] private float playerStartingHealth = 5.0f;
        [SerializeField] private float enemyStartingHealth = 5.0f;
        [SerializeField] private float rollDelay = 1.5f;
        [SerializeField] private float postDamageDelay = 2.0f;
        [SerializeField] private float switchTurnDelay = 0.5f;


        void Start()
        {
            AudioManager.Instance?.PlayMusic(AudioManager.Instance.pvpMusic);

            playerHealth = playerStartingHealth * healthMultiplier;
            enemyHealth = enemyStartingHealth * healthMultiplier;
            currentGameState = GameState.Start;

            playerAnimator = player.GetComponent<Animator>();
            enemyAnimator = enemy.GetComponent<Animator>();

            PickFirstTurn();
        }

        public void Update()
        {
            if (currentGameState == GameState.Win || currentGameState == GameState.Lose) return;
            if (counter < 2.0f)
            {
                counter += Time.deltaTime;
            }
            else
            {
                if (isTurnExecuting) return;
                counter = 0;
                RunPvPLogic();
            }
        }

        public void PickFirstTurn()
        {
            var playerDiceRoll = GenerateRandomNumber();
            var enemyDiceRoll = GenerateRandomNumber();

            while (playerDiceRoll == enemyDiceRoll)
            {
                playerDiceRoll = GenerateRandomNumber();
                enemyDiceRoll = GenerateRandomNumber();
            }

            currentGameState = playerDiceRoll > enemyDiceRoll ? GameState.PlayerTurn : GameState.EnemyTurn;

            UpdateTurnText();
        }

        public void RunPvPLogic()
        {
            isTurnExecuting = true;

            switch (currentGameState)
            {
                case GameState.PlayerTurn:
                    StartCoroutine(ManageTurnCoroutine(true));
                    break;
                case GameState.EnemyTurn:
                    StartCoroutine(ManageTurnCoroutine(false));
                    break;
            }
        }


        public IEnumerator ManageTurnCoroutine(bool isPlayer)
        {
            if (uiController == null)
            {
                Debug.Log("UI Controller is null");
                isTurnExecuting = false;
                yield return null;
            }

            var attackerName = isPlayer ? "You" : uiController.GetEnemyName();
            var defenderName = isPlayer ? uiController.GetEnemyName() : "You";

            uiController.UpdateTurnStatus("Rolling Dice....");

            RollDice();
            uiController.StartDiceRoll(isPlayer, diceRoll);

            yield return new WaitForSeconds(rollDelay);

            uiController.UpdateTurnStatus(attackerName + " rolled a " + diceRoll + "!");

            yield return new WaitForSeconds(rollDelay);
            
            var damage = diceRoll;

            yield return StartCoroutine(AttackAnimation(isPlayer, damage));
            
            TakeDamage(!isPlayer, damage);

            uiController.UpdateTurnStatus($"{defenderName} took {damage} damage!");

            yield return new WaitForSeconds(postDamageDelay);

            var entityDeath = IsDead();

            if (!entityDeath)
            {
                SwitchTurns();
                yield return new WaitForSeconds(switchTurnDelay);
            }

            switch (isPlayer)
            {
                case true:
                    Debug.Log("Player attacked the enemy.");
                    break;
                case false:
                    Debug.Log("Enemy attacked the player.");
                    break;
            }

            isTurnExecuting = false;
        }


        private IEnumerator AttackAnimation(bool isPlayer, int damage)
        {
            var attacker = isPlayer ? playerAnimator : enemyAnimator;
            var defender = isPlayer ? enemyAnimator : playerAnimator;

            attacker.Play("Attack");
            
            yield return new WaitForSeconds(0.5f);

            castEffects.PlayImpactEffect(defender.transform, damage);
            defender.Play(damage >= 4 ? "HitHeavy" : "HitLight");
        }

        public void RollDice()
        {
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.diceRoll);
            diceRoll = GenerateRandomNumber();

            if (diceRollText != null)
            {
                diceRollText.text = "Dice Rolled: " + diceRoll;
            }
        }


        private static int GenerateRandomNumber()
        {
            return Random.Range(1, 7);
        }

        public void SwitchTurns()
        {
            currentGameState = currentGameState switch
            {
                GameState.PlayerTurn => GameState.EnemyTurn,
                GameState.EnemyTurn => GameState.PlayerTurn,
                _ => currentGameState
            };

            UpdateTurnText();
        }

        public void TakeDamage(bool isPlayer, int damageAmount)
        {
            if (isPlayer)
            {
                playerHealth = Mathf.Max(playerHealth - damageAmount, 0);
                
            }
            else
            {
                enemyHealth = Mathf.Max(enemyHealth - damageAmount, 0);
            }

            var currentHealth = isPlayer ? playerHealth : enemyHealth;
            var maxHealth = isPlayer ? playerStartingHealth * healthMultiplier : enemyStartingHealth * healthMultiplier;

            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
            uiController?.UpdateHealthBar(isPlayer, currentHealth, (int)maxHealth);
        }

        public bool IsDead()
        {
            if (playerHealth > 0 && enemyHealth > 0)
            {
                return false;
            }

            var playerWon = enemyHealth <= 0;
            currentGameState = playerWon ? GameState.Win : GameState.Lose;
            
            AudioManager.Instance?.StopMusic();
            AudioManager.Instance?.PlaySfx(playerWon ? AudioManager.Instance.catchSuccess : AudioManager.Instance.catchFail);

            UpdateTurnText();

            var winnerAnimator = playerWon ? playerAnimator : enemyAnimator;
            var loserAnimator = playerWon ? enemyAnimator : playerAnimator;
            winnerAnimator.Play("Macarena");
            loserAnimator.Play("Sad");
            
            if (playerWon)
            {
                GrantReward();
            }
            
            StartCoroutine(EndGame());
            return true;
        }


        public void GrantReward()
        {
            if (currentGameState == GameState.Win)
            {
                Debug.Log("The player has been given a reward.");
            }
        }


        private void UpdateTurnText()
        {
            if (uiController == null) return;

            string statusMessage = currentGameState switch
            {
                GameState.PlayerTurn => "Your Turn!",
                GameState.EnemyTurn => "Enemy's Turn!",
                GameState.Win => "You won the battle!",
                _ => "You lost the battle!"
            };

            uiController.UpdateTurnStatus(statusMessage);
        }

        private static IEnumerator EndGame()
        {
            yield return new WaitForSeconds(4.0f);
            Debug.Log("Game Over! Loading map scene");
            SceneManager.LoadScene("MapView");
        }
    }
}