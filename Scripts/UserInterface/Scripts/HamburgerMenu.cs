using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class HamburgerMenu : MonoBehaviour
{
    public GameObject InventoryScreen;
    public GameObject PlayerStatsScreen;
    public GameObject LeaderboardScreen;
    private VisualElement _handleBar;
    private VisualElement inventoryButton;
    private VisualElement leaderboardButton;
    private VisualElement mapButton;

    private VisualElement root;
    private VisualElement statButton;
    private VisualElement pvpButton;


    private void OnEnable()
    {   
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        mapButton = root.Q<VisualElement>("MapButton");
        inventoryButton = root.Q<VisualElement>("InventoryButton");
        statButton = root.Q<VisualElement>("PlayerStatButton");
        leaderboardButton = root.Q<VisualElement>("LeaderboardButton");
        _handleBar = root.Q<VisualElement>("HandleBar");
        pvpButton = root.Q<VisualElement>("PvPButton");


        mapButton?.RegisterCallback<ClickEvent>(OnCloseGameMenuClick);
        inventoryButton?.RegisterCallback<ClickEvent>(OnOpenInventoryClick);
        statButton?.RegisterCallback<ClickEvent>(OnOpenStatScreenClick);
        leaderboardButton?.RegisterCallback<ClickEvent>(OnOpenLeaderboardScreenClick);
        pvpButton?.RegisterCallback<ClickEvent>(OnOpenPvPSceneClick);

        UpdateHandleBarColour();
    }

    private void UpdateHandleBarColour()
    {
        var data = PlayerDataSaveManager.LoadPlayerData();

        var team = data.GetPlayerTeam();
        _handleBar.style.backgroundColor = team switch
        {
            "UoN" => Color.green,
            "NTU" => Color.magenta,
            _ => _handleBar.style.backgroundColor
        };
    }


    private void OnCloseGameMenuClick(ClickEvent evt)
    {
        Debug.Log("Closing in game menu");
        gameObject.SetActive(false);
    }


    private void OnOpenInventoryClick(ClickEvent evt)
    {
        Debug.Log("Opening Inventory screen");
        InventoryScreen.SetActive(true);
    }


    private void OnOpenStatScreenClick(ClickEvent evt)
    {
        Debug.Log("Opening player stat screen");
        PlayerStatsScreen.SetActive(true);
    }


    private void OnOpenLeaderboardScreenClick(ClickEvent evt)
    {
        Debug.Log("Opening leaderboard screen");
        LeaderboardScreen.SetActive(true);
    }

    private void OnOpenPvPSceneClick(ClickEvent evt)
    {
        Debug.Log("Opening PvP Scene");
        SceneManager.LoadScene("PvP");
    }
}