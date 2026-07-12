using System.Collections;
using System.IO;
using Display;
using Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.PlayMode.Display
{
    /// <summary>
    ///     Tests the functionality of the Main UI Screen.
    /// </summary>
    /// <remarks>
    ///     If new panels / screens are added to the login flow,
    ///     add them to the IsOnlyPanelVisible array.
    /// </remarks>
    public class SignupLoginTests
    {
        /// <summary>
        ///     Constants to prevent any typos
        /// </summary>
        private const string KeyUserTeam = "UserTeam";

        private const string SceneLogin = "LoginUI";
        private const string SceneMap = "MapView";
        private const string TeamUoN = "UoN";
        private const string TeamNtu = "NTU";

        private SignupLogin _mainScreen;
        private VisualElement _root;

        /// <summary>
        ///     Runs before every testcase and loads in the Login UI.
        ///     Clears the team that has been chosen for future test cases.
        /// </summary>
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayerDataSaveManager.SavePlayerData(null);

            yield return SceneManager.LoadSceneAsync(SceneLogin);

            _mainScreen = Object.FindAnyObjectByType<SignupLogin>();
            Assert.IsNotNull(_mainScreen, "Did not find the UI Screen");
            _root = _mainScreen.GetComponent<UIDocument>().rootVisualElement;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var savePath = Path.Combine(Application.persistentDataPath, "player_data.json");

            if (File.Exists(savePath)) File.Delete(savePath);

            yield return null;
        }

        private bool IsOnlyPanelVisible(string panelName)
        {
            var panels = new[] { "MainSelection", "SignUpPanel", "TeamChoicePanel" };

            foreach (var panel in panels)
            {
                var display = _root.Q<VisualElement>(panel).resolvedStyle.display;

                if (panel == panelName)
                    Assert.AreEqual(DisplayStyle.Flex, display, $"{panel} should be visible");
                else
                    Assert.AreEqual(DisplayStyle.None, display, $"{panel} should be hidden");
            }

            return _root.Q<VisualElement>(panelName).resolvedStyle.display == DisplayStyle.Flex;
        }


        /// <summary>
        ///     Ensures that clicking the Sign Up button activates the Sign Up panel and
        ///     deactivates the Main UI Panel
        /// </summary>
        [UnityTest]
        public IEnumerator SignUpButtonOpensSignUpPanel()
        {
            _mainScreen.SignUpButtonClicked();
            yield return null;
            IsOnlyPanelVisible("SignUpPanel");
        }


        /// <summary>
        ///     Simulates the username and password in the Sign up panel, then clicks the submit button.
        ///     Verifies that the panel changes to Team Selection panel.
        /// </summary>
        [UnityTest]
        public IEnumerator SignUpButtonSubmitsUsernameAndPassword()
        {
            _mainScreen.SignUpButtonClicked();
            yield return null;

            _root.Q<TextField>("SignUpUsername").value = "johnny boy";
            _root.Q<TextField>("SignUpPassword").value = "johnnyspassword";

            _mainScreen.SignUpSubmitButtonClicked();
            yield return null;
            IsOnlyPanelVisible("TeamChoicePanel");
        }


        /// <summary>
        ///     Verifies that the Guest Button opens the Team selection panel.
        /// </summary>
        [UnityTest]
        public IEnumerator GuestButtonOpenTeamPanel()
        {
            _mainScreen.GuestButtonClicked();
            yield return null;
            IsOnlyPanelVisible("TeamChoicePanel");
        }


        /// <summary>
        ///     Ensures that the PlayerPrefs is set to UoN when the Team UoN button is pressed.
        /// </summary>
        [UnityTest]
        public IEnumerator ChooseTeamUoN()
        {
            _mainScreen.GuestButtonClicked();
            yield return null;

            SignupLogin.TeamUoNButtonClicked();

            var playerData = PlayerDataSaveManager.LoadPlayerData();
            Assert.AreEqual(TeamUoN, playerData?.playerTeam, "Player chose team UoN");
        }


        /// <summary>
        ///     Ensures that the PlayerPrefs is set to NTU when the Team NTU button is pressed.
        /// </summary>
        [UnityTest]
        public IEnumerator ChooseTeamNtu()
        {
            _mainScreen.GuestButtonClicked();
            yield return null;

            SignupLogin.TeamNtuButtonClicked();

            var playerData = PlayerDataSaveManager.LoadPlayerData();
            Assert.AreEqual(TeamNtu, playerData?.playerTeam, "Player chose team NTU");
        }


        /// <summary>
        ///     Checks that selecting a team triggers a scene change from LoginUI to MapView.
        /// </summary>
        [UnityTest]
        public IEnumerator OpensMapView()
        {
            _mainScreen.GuestButtonClicked();
            yield return null;


            SignupLogin.TeamUoNButtonClicked();

            var timeout = 2.0f;

            // wait 2 seconds for the scene to change from LoginUI to MapView
            while (SceneManager.GetActiveScene().name != SceneMap && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            var currentScene = SceneManager.GetActiveScene().name;
            Assert.AreEqual(SceneMap, currentScene, "MapView scene active");
        }
    }
}