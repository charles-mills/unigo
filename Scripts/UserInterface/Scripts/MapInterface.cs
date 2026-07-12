using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using UserInterface.Scripts;

namespace Display
{
    public class MapInterface : MonoBehaviour
    {
        [Header("Scene References")]
        public GameObject hamBurger;
        public GameObject tutorial;


        [Header("Box Animation")]
        public Texture2D boxClosed;
        public Texture2D boxOpen;

        private VisualElement _leftHalf;
        private VisualElement _menuButton;

        private VisualElement _menuIcon;
        private Label _playerName;
        private VisualElement _rightHalf;

        private VisualElement _tutorialButton;

        [Header("UI Elements")]
        private VisualElement _root;

        public void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _menuButton = _root.Q<VisualElement>("MenuButton");
            _leftHalf = _root.Q<VisualElement>("LeftHalf");
            _rightHalf = _root.Q<VisualElement>("RightHalf");
            _playerName = _root.Q<Label>("PlayerName");
            _menuIcon = _root.Q<VisualElement>("MenuIcon");
            _tutorialButton = _root.Q<VisualElement>("TutorialButton");

            _menuButton?.RegisterCallback<ClickEvent>(OnMenuButtonClicked);
            _tutorialButton?.RegisterCallback<ClickEvent>(OnTutorialButtonClicked);

            UpdatePlayerInfo();

            var playerIcon = _root.Q<VisualElement>("PlayerIcon");
            var preview = FindFirstObjectByType<PlayerModelPreview>();
            
            if (preview != null && preview.PreviewTexture != null)
            {
                playerIcon.style.backgroundImage =
                    new StyleBackground(Background.FromRenderTexture(preview.PreviewTexture));
            }
            
            playerIcon.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                playerIcon.style.height = evt.newRect.width * 1.5f;
            });
        }


        private void OnMenuButtonClicked(ClickEvent evt)
        {
            if (hamBurger == null) return;

            var opening = !hamBurger.activeSelf;
            hamBurger.SetActive(opening);

            _menuIcon.style.backgroundImage = opening ? boxOpen : boxClosed;
        }

        private void OnTutorialButtonClicked(ClickEvent evt)
        {
            if (tutorial == null) return;

            var opening = !tutorial.activeSelf;
            tutorial.SetActive(opening);
        }

        private void UpdatePlayerInfo()
        {
            var currentPlayerData = PlayerDataSaveManager.LoadPlayerData();

            if (currentPlayerData == null) return;

            if (_playerName != null) _playerName.text = currentPlayerData.playerName;

            var team = currentPlayerData.GetPlayerTeam();

            switch (team)
            {
                case "UoN":
                    SetCircleColours(Color.yellow, Color.green);
                    break;
                case "NTU":
                    SetCircleColours(Color.black, Color.magenta);
                    break;
            }
        }

        private void SetCircleColours(Color left, Color right)
        {
            if (_leftHalf != null) _leftHalf.style.backgroundColor = left;

            if (_rightHalf != null) _rightHalf.style.backgroundColor = right;
        }
    }
}