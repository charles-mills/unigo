using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

public class StatsUi : MonoBehaviour
{
    private VisualElement closeStatsPage;
    public static bool isStatsUiOpen = false;

    private void OnEnable()
    {
        
        isStatsUiOpen = true;

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var dataRecord = PlayerDataSaveManager.LoadPlayerData();

        if (dataRecord == null) return;


        var usernameLabel = root.Q<Label>("Username");
        var teamLabel = root.Q<Label>("Team");
        var catchSuccessLabel = root.Q<Label>("CatchSuccess");
        var catchFailLabel = root.Q<Label>("CatchFail");
        closeStatsPage = root.Q<VisualElement>("MenuButton");

        if (usernameLabel != null) usernameLabel.text = dataRecord.playerName;

        if (teamLabel != null)
        {
            var teamDisplayText = dataRecord.playerTeam switch
            {
                "UoN" => "University of Nottingham",
                "NTU" => "Nottingham Trent University",
                _ => "No Team"
            };

            teamLabel.text = teamDisplayText;
        }

        if (catchSuccessLabel != null) catchSuccessLabel.text = "Catch Successes: " + dataRecord.GetCatchSuccess();

        if (catchFailLabel != null) catchFailLabel.text = "Catch Fails: " + dataRecord.GetCatchFail();

        if (closeStatsPage != null) closeStatsPage.RegisterCallback<ClickEvent>(OnCloseStatScreenClick);
    }


    private void OnCloseStatScreenClick(ClickEvent evt)
    {
        Debug.Log("Closing stats screen");
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isStatsUiOpen = false;
    }

}