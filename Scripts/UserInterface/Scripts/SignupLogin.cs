using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Display
{
    public class SignupLogin : MonoBehaviour
    {
        private VisualElement _mainSelection;

        [Header("UI Elements")] 
        private VisualElement _root;

        private VisualElement _signUpPanel;

        private TextField _signUpPassword;

        [Header("UI Fields")]
        private TextField _signUpUsername;

        private VisualElement _teamChoicePanel;

        private void Start()
        {
            AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
        }

        private void OnEnable()
        {
            TouchScreenKeyboard.hideInput = true;

            _root = GetComponent<UIDocument>().rootVisualElement;

            _mainSelection = _root.Q<VisualElement>("MainSelection");
            _signUpPanel = _root.Q<VisualElement>("SignUpPanel");
            _teamChoicePanel = _root.Q<VisualElement>("TeamChoicePanel");

            _signUpUsername = _root.Q<TextField>("SignUpUsername");
            _signUpPassword = _root.Q<TextField>("SignUpPassword");


            _root.Q<Button>("LoginSignupButton").clicked += SignUpButtonClicked;
            _root.Q<Button>("ContinueGuestButton").clicked += GuestButtonClicked;
            _root.Q<Button>("BackButton").clicked += BackButtonClicked;
            _root.Q<Button>("TeamUoNButton").clicked += TeamUoNButtonClicked;
            _root.Q<Button>("TeamNTUButton").clicked += TeamNtuButtonClicked;
            _root.Q<Button>("BackButtonTeam").clicked += BackButtonClicked;

            var submitButton = _root.Q<Button>("SubmitButton");
            submitButton.clicked += SignUpSubmitButtonClicked;
            submitButton.SetEnabled(false);

            _signUpUsername.RegisterValueChangedCallback(_ => UpdateSubmitButton(submitButton));
            _signUpPassword.RegisterValueChangedCallback(_ => UpdateSubmitButton(submitButton));
        }


        private void ShowPanel(VisualElement panelToShow)
        {
            _mainSelection.style.display = DisplayStyle.None;
            _signUpPanel.style.display = DisplayStyle.None;
            _teamChoicePanel.style.display = DisplayStyle.None;
            panelToShow.style.display = DisplayStyle.Flex;
        }

        private void ShowPanelWithSfx(VisualElement panelToShow)
        {
            ShowPanel(panelToShow);
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.tapped);
        }

        private void UpdateSubmitButton(Button submitButton)
        {
            var hasInput = !string.IsNullOrEmpty(_signUpUsername.value) &&
                           !string.IsNullOrEmpty(_signUpPassword.value);
            submitButton.SetEnabled(hasInput);
        }

        public void SignUpButtonClicked()
        {
            ShowPanelWithSfx(_signUpPanel);
            Debug.Log("Sign up button clicked");
        }

        public void GuestButtonClicked()
        {
            var existingPlayer = PlayerDataSaveManager.LoadPlayerData();
            if (existingPlayer == null) PlayerDataSaveManager.SavePlayerData(new PlayerDataRecord("Guest"));

            ShowPanelWithSfx(_teamChoicePanel);
            Debug.Log("Guest button clicked");
        }


        public void BackButtonClicked()
        {
            _signUpUsername.value = "";
            _signUpPassword.value = "";
            ShowPanelWithSfx(_mainSelection);
            Debug.Log("Back button clicked");
        }


        public void SignUpSubmitButtonClicked()
        {
            var username = _signUpUsername.value;
            var password = _signUpPassword.value;

            Debug.Log("Username: " + username + " and password: " + password);

            var existingPlayerData = PlayerDataSaveManager.LoadPlayerData();


            if (existingPlayerData == null || existingPlayerData.playerName != username)
            {
                var newPlayerData = new PlayerDataRecord(username);
                PlayerDataSaveManager.SavePlayerData(newPlayerData);
                CheckTeamSelectionAfterAuth();
            }
            else
            {
                Debug.Log("You are an existing player");
                SceneManager.LoadScene("MapView", LoadSceneMode.Single);
            }
        }


        private void CheckTeamSelectionAfterAuth()
        {
            // Check if user has already selected a team

            var currentPlayer = PlayerDataSaveManager.LoadPlayerData();

            if (currentPlayer == null || string.IsNullOrEmpty(currentPlayer.playerTeam))
            {
                // User hasn't selected a team yet - show team selection
                Debug.Log("User needs to select a team");
                ShowPanelWithSfx(_teamChoicePanel);
            }
            else
            {
                // User already has a team - go to map
                Debug.Log("User already has team: " + currentPlayer.playerTeam + " - going to map");
                SceneManager.LoadScene("MapView");
            }
        }

        public static void TeamUoNButtonClicked()
        {
            SelectTeam("UoN");
        }

        public static void TeamNtuButtonClicked()
        {
            SelectTeam("NTU");
        }

        private static void SelectTeam(string teamName)
        {
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.tapped);
            var currentPlayer = PlayerDataSaveManager.LoadPlayerData();

            if (currentPlayer != null)
            {
                currentPlayer.playerTeam = teamName;
                PlayerDataSaveManager.SavePlayerData(currentPlayer);
                Debug.Log($"User selected team: {teamName}");
            }
            else
            {
                Debug.Log("No Player data found");
            }

            SceneManager.LoadScene("MapView");
        }
    }
}