using System.Collections;
using Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Gameplay
{
    public class TurnBasedCombatTests
    {
        private GameLoop combatMechanics;

        private GameObject pvpGameObject;


        [UnitySetUp]
        public IEnumerator SetUp()
        {
            pvpGameObject = new GameObject("TestPvPTurn");
            combatMechanics = pvpGameObject.AddComponent<GameLoop>();

            yield return null;
        }


        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (pvpGameObject != null) Object.Destroy(pvpGameObject);

            yield return null;
        }


        [UnityTest]
        public IEnumerator FirstTurnTest()
        {
            combatMechanics.PickFirstTurn();

            var validTurn = combatMechanics.currentGameState == GameState.PlayerTurn ||
                            combatMechanics.currentGameState == GameState.EnemyTurn;

            Assert.IsTrue(validTurn, "The game state was changed from Start to either PlayerTurn or EnemyTurn.");

            Object.Destroy(pvpGameObject);

            yield return null;
        }


        [UnityTest]
        public IEnumerator SwitchTurnFromEnemyToPlayerTest()
        {
            combatMechanics.currentGameState = GameState.EnemyTurn;

            combatMechanics.SwitchTurns();

            var playerTurn = combatMechanics.currentGameState == GameState.PlayerTurn;

            Assert.IsTrue(playerTurn, "Should change from enemys turn to players turn.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator SwitchTurnFromPlayerToEnemyTest()
        {
            combatMechanics.currentGameState = GameState.PlayerTurn;

            combatMechanics.SwitchTurns();

            var enemyTurn = combatMechanics.currentGameState == GameState.EnemyTurn;

            Assert.IsTrue(enemyTurn, "Should change from players turn to enemys turn.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator RandomDiceRollsTest()
        {
            combatMechanics.diceRoll = 1;
            var diceValueChanged = false;
            var intialDiceRoll = combatMechanics.diceRoll;


            for (var i = 0; i < 20; i++)
            {
                combatMechanics.RollDice();

                var currentDiceRoll = combatMechanics.diceRoll;

                Assert.GreaterOrEqual(currentDiceRoll, 1, "Dice roll was below 1.");
                Assert.LessOrEqual(currentDiceRoll, 6, "Dice roll was above 6.");

                if (currentDiceRoll != intialDiceRoll) diceValueChanged = true;
            }


            Assert.IsTrue(diceValueChanged, "The Dice rolled 20 times and the value changed.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator PlayerHealthZeroTest()
        {
            combatMechanics.playerHealth = 0;

            combatMechanics.IsDead();

            var gameOver = combatMechanics.currentGameState == GameState.Lose;

            Assert.IsTrue(gameOver, "Player loses the game since their health reached 0.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator PlayerHealthNotZeroTest()
        {
            combatMechanics.currentGameState = GameState.PlayerTurn;
            combatMechanics.playerHealth = 5;

            combatMechanics.enemyHealth = 10;

            combatMechanics.ManageTurnCoroutine(true);

            var gameOver = combatMechanics.currentGameState == GameState.EnemyTurn;

            Assert.IsTrue(gameOver, "Player health is above 0 so they did not lose and it is enemys turn next.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator EnemyHealthZeroTest()
        {
            combatMechanics.currentGameState = GameState.EnemyTurn;
            combatMechanics.enemyHealth = 0;

            combatMechanics.IsDead();


            var gameOver = combatMechanics.currentGameState == GameState.Win;

            Assert.IsTrue(gameOver, "Player wins the game since the enemy health is zero.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyHealthNotZeroTest()
        {
            combatMechanics.currentGameState = GameState.EnemyTurn;
            combatMechanics.enemyHealth = 5;

            combatMechanics.playerHealth = 10;


            combatMechanics.ManageTurnCoroutine(false);

            Assert.AreEqual(GameState.PlayerTurn, combatMechanics.currentGameState,
                "Enemy health is above 0 so it should move to the players turn.");


            yield return null;
        }


        [UnityTest]
        public IEnumerator PlayerGrantedReward()
        {
            combatMechanics.currentGameState = GameState.Win;

            LogAssert.Expect(LogType.Log, "The player has been given a reward.");

            combatMechanics.GrantReward();

            yield return null;
        }


        [UnityTest]
        public IEnumerator TakeDamageTest()
        {
            combatMechanics.currentGameState = GameState.PlayerTurn;
            combatMechanics.playerHealth = 5;

            var isPlayer = true;
            combatMechanics.TakeDamage(isPlayer, 2);

            Assert.AreEqual(3, combatMechanics.playerHealth, "Player loses 2 health points.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator PlayerHealthNegativeTest()
        {
            combatMechanics.currentGameState = GameState.PlayerTurn;
            combatMechanics.playerHealth = -2;

            var isPlayer = true;
            combatMechanics.TakeDamage(isPlayer, 5);

            Assert.AreEqual(0, combatMechanics.playerHealth, "Player health should be 0 and not negative.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator CheckWinState()
        {
            combatMechanics.currentGameState = GameState.Win;

            combatMechanics.RunPvPLogic();

            Assert.AreEqual(GameState.Win, combatMechanics.currentGameState, "The game state should not change.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator EnemyHealthNegativeTest()
        {
            combatMechanics.currentGameState = GameState.EnemyTurn;
            combatMechanics.enemyHealth = -5;

            var isPlayer = false;
            combatMechanics.TakeDamage(isPlayer, 5);

            Assert.AreEqual(0, combatMechanics.enemyHealth, "Enemy health should be 0 and not negative.");

            yield return null;
        }
    }
}