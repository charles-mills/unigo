using System;
using System.Linq;
using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderBoardUI : MonoBehaviour
{
    public VisualTreeAsset npcTemplate;

    public TextAsset filePath;
    private string leaderboardDataParsed = "";
    private LeaderboardItems leaderboardDataStored;
    private VisualElement m_Root;


    private void OnEnable()
    {
        var position = 1;

        if (filePath == null) Debug.Log("Could not find the file path to LeaderboardNPCData");

        leaderboardDataParsed = filePath.text;

        var parsedJsonFile = JsonUtility.FromJson<LeaderboardWrapper>(leaderboardDataParsed);

        var dataRecord = PlayerDataSaveManager.LoadPlayerData();

        var uiDocument = GetComponent<UIDocument>();

        m_Root = uiDocument.rootVisualElement;
        var m_CloseLeaderBoardIcon = m_Root.Q<Button>("CloseUI");
        m_CloseLeaderBoardIcon.clicked += () => gameObject.SetActive(false);

        if (dataRecord == null) return;

        var playerFold = m_Root.Q<Foldout>("Player");
        var playerTokens = m_Root.Q<Label>("PlayerTokensCaught");
        var playerAI = m_Root.Q<Label>("PlayerDefeatedAi");

        var playerTokenCount = dataRecord.GetCatchSuccess();
        var playerRank = 1;

        if (parsedJsonFile.NPCData != null)
        {
            playerRank += parsedJsonFile.NPCData.Count(n => n.tokensCaught > playerTokenCount);
        }

        playerFold.text = $"{playerRank}. {dataRecord.playerName} (You)";
        playerTokens.text = "Tokens Caught: " + playerTokenCount;
        playerAI.text = "Enemies Defeated: " + dataRecord.GetEnemyDefeated();

        var scrollView = m_Root.Q<ScrollView>();
        var container = scrollView.contentContainer;
        container.Query<Foldout>("NPC").ForEach(npcEntry => npcEntry.RemoveFromHierarchy());

        if (parsedJsonFile.NPCData == null) return;
        foreach (var npc in parsedJsonFile.NPCData.OrderByDescending(n => n.tokensCaught))
        {
            VisualElement newNPCEntry = npcTemplate.Instantiate();

            var npcFoldout = newNPCEntry.Q<Foldout>();
            var tokensData = newNPCEntry.Q<Label>("NPCTokensCaught");
            var defeatedAIData = newNPCEntry.Q<Label>("NPCDefeatedAI");

            if (npcFoldout != null)
            {
                if (position == playerRank)
                {
                    position++;
                }

                npcFoldout.text = $"{position}. {npc.name}";

                switch (npc.team)
                {
                    case "UoN":
                        npcFoldout.AddToClassList("team-uon");
                        break;
                    case "NTU":
                        npcFoldout.AddToClassList("team-ntu");
                        break;
                }
            }

            if (tokensData != null) tokensData.text = "Tokens Caught: " + npc.tokensCaught;

            if (defeatedAIData != null) defeatedAIData.text = "Enemies Defeated: " + npc.enemiesDefeated;

            container.Add(newNPCEntry);
            position++;
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
    public struct LeaderboardWrapper
    {
        public LeaderboardItems[] NPCData;
    }
}