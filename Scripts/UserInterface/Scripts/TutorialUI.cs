using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    private VisualElement m_Root;
    private VisualElement m_catching_items;
    private VisualElement m_menu_select;
    private VisualElement m_close;
    private VisualElement m_catching_items_button;
    private VisualElement m_PvP_button;
    private VisualElement m_PvP;
    private VisualElement m_Stats_button;
    private VisualElement m_Stats;
    private VisualElement m_LeaderBoard_button;
    private VisualElement m_LeaderBoard;
    private VisualElement m_UniStop_button;
    private VisualElement m_UniStop;
    private VisualElement m_Login_button;
    private VisualElement m_Login;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.Log("uiDocument not found");
            return;
        }

        m_Root = uiDocument.rootVisualElement;
        m_Root.style.display = DisplayStyle.Flex;

        m_menu_select = m_Root.Q<VisualElement>("menuSelect");
        m_close = m_Root.Q<VisualElement>("close");
        
        m_catching_items_button = m_Root.Q<VisualElement>("catching_items_button");
        m_catching_items = m_Root.Q<VisualElement>("Catching_items");

        m_PvP_button = m_Root.Q<VisualElement>("PvP_button");
        m_PvP = m_Root.Q<VisualElement>("PvP");

        m_Stats_button = m_Root.Q<VisualElement>("Stats_button");
        m_Stats = m_Root.Q<VisualElement>("Stats");

        m_LeaderBoard_button = m_Root.Q<VisualElement>("LeaderBoard_button");
        m_LeaderBoard = m_Root.Q<VisualElement>("LeaderBoard");

        m_UniStop_button = m_Root.Q<VisualElement>("UniStop_button");
        m_UniStop = m_Root.Q<VisualElement>("UniStop");

        m_Login_button = m_Root.Q<VisualElement>("Login_button");
        m_Login = m_Root.Q<VisualElement>("Login");

        if (m_close != null)
        {
            m_close.RegisterCallback<ClickEvent>(closeAllTutorials);
        }
        if (m_catching_items_button != null)
        {
            m_catching_items_button.RegisterCallback<ClickEvent>(openCatchTutorial);
        }
        if (m_PvP_button != null)
        {
            m_PvP_button.RegisterCallback<ClickEvent>(openPvPTutorial);
        }
        if (m_Stats_button != null)
        {
            m_Stats_button.RegisterCallback<ClickEvent>(openStatsTutorial);
        }
        if (m_LeaderBoard_button != null)
        {
            m_LeaderBoard_button.RegisterCallback<ClickEvent>(openLeaderboardTutorial);
        }
        if (m_UniStop_button != null)
        {
            m_UniStop_button.RegisterCallback<ClickEvent>(openGoStopTutorial);
        }
        if (m_Login_button != null)
        {
            m_Login_button.RegisterCallback<ClickEvent>(openLoginTutorial);
        }

        closeAllTutorialsOnEnable();
    }

    private void closeAllTutorials(ClickEvent evt)
    {
        if (m_menu_select.enabledSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            closeTutorial(m_catching_items);
            closeTutorial(m_PvP);
            closeTutorial(m_Stats);
            closeTutorial(m_LeaderBoard);
            closeTutorial(m_UniStop);
            m_menu_select.SetEnabled(true);
            m_menu_select.visible = true;
        }
    }

    private void closeAllTutorialsOnEnable()
    {
        closeTutorial(m_catching_items);
        closeTutorial(m_PvP);
        closeTutorial(m_Stats);
        closeTutorial(m_LeaderBoard);
        closeTutorial(m_UniStop);
        m_menu_select.SetEnabled(true);
        m_menu_select.visible = true;
    }
    private void closeTutorial(VisualElement tutorial)
    {
        tutorial.SetEnabled(false);
        tutorial.visible = false;
    }

    private void openCatchTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_catching_items.SetEnabled(true);
        m_catching_items.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
    private void openPvPTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_PvP.SetEnabled(true);
        m_PvP.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
    private void openStatsTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_Stats.SetEnabled(true);
        m_Stats.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
    private void openLeaderboardTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_LeaderBoard.SetEnabled(true);
        m_LeaderBoard.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
    private void openGoStopTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_UniStop.SetEnabled(true);
        m_UniStop.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
    private void openLoginTutorial(ClickEvent evt)
    {
        Debug.Log("opening tutorial");
        m_Login.SetEnabled(true);
        m_Login.visible = true;
        m_menu_select.SetEnabled(false);
        m_menu_select.visible = false;
    }
}
