using UnityEngine;
using UnityEngine.UIElements;
using Gameplay;
using System;
using System.Collections;



public class UiController : MonoBehaviour
{

    private VisualElement playerHealthBarFill;
    private VisualElement enemyHealthBarFill;
    private Label playerName;
    private Label enemyName;
    public TextAsset npcJsonFile;
    private Label playerDiceRoll;
    private Label enemyDiceRoll;
    private Label entityTurnStatus;




    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playerHealthBarFill = root.Q<VisualElement>("PlayerHealthBarFill");
        enemyHealthBarFill = root.Q<VisualElement>("EnemyHealthBarFill");
        
        playerName = root.Q<Label>("PlayerName");
        enemyName = root.Q<Label>("EnemyName");

        playerDiceRoll = root.Q<Label>("PlayerDiceRollNumber");
        enemyDiceRoll = root.Q<Label>("EnemyDiceRollNumber");

        entityTurnStatus = root.Q<Label>("CurrentTurn");

        LoadAndSetNames();
    }



    public void UpdateTurnStatus(string message)
    {
        if(entityTurnStatus != null)
        {
            entityTurnStatus.text = message;
        }
    }

    public string GetEnemyName()
    {
        return enemyName != null ? enemyName.text : "Rocket Go";
    }

    public void StartDiceRoll(bool isPlayer, int finalRolledNumber)
    {
        var targetLabel = isPlayer ? playerDiceRoll : enemyDiceRoll;

        if(targetLabel != null)
        {
            StartCoroutine(AnimateDice(targetLabel, finalRolledNumber));
        }
    }

    private static IEnumerator AnimateDice(Label labelToRoll, int finalRolledNumber)
    {
        float duration = 1.0f;
        float timer = 0.0f;
        float speed = 0.05f;

        while(timer < duration)
        {
            labelToRoll.text = UnityEngine.Random.Range(1, 7).ToString();

            yield return new WaitForSeconds(speed);
            timer += speed;
        }

        labelToRoll.text = finalRolledNumber.ToString();    
    }


    private void LoadAndSetNames()
    {
        var savedPlayerData = PlayerDataSaveManager.LoadPlayerData();

        if(savedPlayerData != null && !string.IsNullOrEmpty(savedPlayerData.playerName))
        {
            playerName.text = savedPlayerData.playerName;
        }
        else
        {
            playerName.text = "You";
        }


        if (enemyName == null) return;
        if (npcJsonFile == null) return;
        
        var parseJson = JsonUtility.FromJson<NPCWrapper>(npcJsonFile.text);

        if(parseJson.NPCData is { Length: > 0 })
        {
            var randomNum = UnityEngine.Random.Range(0, parseJson.NPCData.Length);
            enemyName.text = parseJson.NPCData[randomNum].name;
        }
        else
        {
            enemyName.text = "Rocket Go";
        }

    }


    public void UpdateHealthBar(bool isPlayer, float currentHealth, int maxHealth)
    {
        var healthPercentage = ( currentHealth / maxHealth) * 100f;

        healthPercentage = Mathf.Clamp(healthPercentage, 0, 100);

        if(isPlayer)
        {
            playerHealthBarFill.style.width = Length.Percent(healthPercentage);
        }
        else
        {
            enemyHealthBarFill.style.width = Length.Percent(healthPercentage);
        }
    }


    [Serializable]
    public struct LeaderboardItems
    {
        public string id;
        public string name;
        public string team;
        public int tokensCaught;
        public int enemiesDefeated;
    }


    [Serializable]
    public struct NPCWrapper
    {
        public LeaderboardItems[] NPCData;
    }

}
