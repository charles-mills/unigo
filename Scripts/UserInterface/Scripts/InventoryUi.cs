using System;
using System.Collections.Generic;
using Inventory;
using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Display
{
    public class InventoryUi : MonoBehaviour
    {
        public List<InventorySlot> InventoryItems = new();
        private InventoryManager m_InventoryManager;
        private VisualElement m_MapButton;

        private VisualElement m_Root;
        private VisualElement m_SlotContainer;

        private VisualElement m_ConfirmationOverlay;
        private Label m_ConfirmationText;
        private Button m_ConfirmUseButton;
        private Button m_GoBackButton;

        private string m_PendingCodeToRemove;


        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.Log("uiDocument not found");
                return;
            }

            uiDocument.sortingOrder = 100;
            m_Root = uiDocument.rootVisualElement;
            m_Root.style.display = DisplayStyle.Flex;
            m_Root.style.flexGrow = 1;
            m_Root.style.backgroundColor = new StyleColor(new Color(0.10f, 0.12f, 0.12f, 0.94f));

            m_SlotContainer = m_Root.Q<VisualElement>("SlotContainer");
            m_MapButton = m_Root.Q<VisualElement>("MapButton");

            m_ConfirmationOverlay = m_Root.Q<VisualElement>("ConfirmationOverlay");
            m_ConfirmationText = m_Root.Q<Label>("ConfirmationText");
            m_ConfirmUseButton = m_Root.Q<Button>("ConfirmUseButton");
            m_GoBackButton = m_Root.Q<Button>("GoBackButton");
            

            m_InventoryManager = ResolveInventoryManager();


            if (m_MapButton != null) m_MapButton.RegisterCallback<ClickEvent>(OnCloseInventoryClick);

            if (m_ConfirmUseButton != null) m_ConfirmUseButton.clicked += ConfirmMarkAsUsed;
            if (m_GoBackButton != null) m_GoBackButton.clicked += HideConfirmationPopup;
            if (m_ConfirmationOverlay != null) m_ConfirmationOverlay.style.display = DisplayStyle.None;

            if (m_InventoryManager != null) {

                m_InventoryManager.ItemAdded += OnInventoryItemAdded;
                m_InventoryManager.ItemRemoved += OnInventoryItemRemoved;
             }

            RenderInventory();
        }

        private void OnDisable()
        {
            if (m_MapButton != null) m_MapButton.UnregisterCallback<ClickEvent>(OnCloseInventoryClick);

            if (m_ConfirmUseButton != null) m_ConfirmUseButton.clicked -= ConfirmMarkAsUsed;

            if (m_GoBackButton != null) m_GoBackButton.clicked -= HideConfirmationPopup;

            if (m_InventoryManager != null){
                m_InventoryManager.ItemAdded -= OnInventoryItemAdded;
                m_InventoryManager.ItemRemoved -= OnInventoryItemRemoved;
            }
        }

        private InventoryManager ResolveInventoryManager()
        {
            if (InventoryManager.Instance != null) return InventoryManager.Instance;

            var existingManager = FindAnyObjectByType<InventoryManager>();
            if (existingManager != null) return existingManager;

            var managerObject = new GameObject("InventoryManager");
            return managerObject.AddComponent<InventoryManager>();
        }

        private void RenderInventory()
        {
            if (m_SlotContainer == null)
            {
                Debug.LogWarning("SlotContainer element not found in Inventory UXML.");
                return;
            }

            m_SlotContainer.style.flexDirection = FlexDirection.Column;
            m_SlotContainer.style.flexWrap = Wrap.NoWrap;
            m_SlotContainer.style.justifyContent = Justify.FlexStart;
            m_SlotContainer.style.alignContent = Align.FlexStart;
            m_SlotContainer.style.paddingLeft = 8;
            m_SlotContainer.style.paddingRight = 8;
            m_SlotContainer.style.paddingTop = 8;
            m_SlotContainer.style.paddingBottom = 120;

            m_SlotContainer.Clear();
            InventoryItems.Clear();

            if (m_InventoryManager == null)
            {
                Debug.LogWarning("InventoryManager not found. Inventory UI cannot render items.");
                return;
            }

            foreach (var inventoryItem in m_InventoryManager.GetInventory()) AddSlot(inventoryItem);
        }

        private void OnInventoryItemAdded(InventoryItem inventoryItem)
        {
            if (m_SlotContainer == null || inventoryItem == null) return;

            AddSlot(inventoryItem);
        }

        private void OnInventoryItemRemoved(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            RenderInventory();
        }


        private void AddSlot(InventoryItem inventoryItem)
        {
            var slot = new InventorySlot();
            slot.MarkAsUsedClicked += OnMarkAsUsedClicked;

            slot.SetItem(
                inventoryItem.name,
                inventoryItem.code,
                GetExpiryText(inventoryItem)
            );

            InventoryItems.Add(slot);
            m_SlotContainer.Add(slot);
        }

        private void OnMarkAsUsedClicked(string code){
            if (string.IsNullOrWhiteSpace(code) || m_ConfirmationOverlay == null) return;

            m_PendingCodeToRemove = code;

            if (m_ConfirmationText != null) m_ConfirmationText.text = $"Are you sure you want to remove code {code}?";

            m_ConfirmationOverlay.style.display = DisplayStyle.Flex;
        }

        private void HideConfirmationPopup(){
            m_PendingCodeToRemove = null;

            if (m_ConfirmationOverlay != null) m_ConfirmationOverlay.style.display = DisplayStyle.None;

        }

        private void ConfirmMarkAsUsed(){
            if (m_InventoryManager == null || string.IsNullOrWhiteSpace(m_PendingCodeToRemove)) return;

            m_InventoryManager.RemoveByCode(m_PendingCodeToRemove);
            HideConfirmationPopup();
        }

        private static string GetExpiryText(InventoryItem inventoryItem)
        {
            if (inventoryItem == null || inventoryItem.expiresAtUnixSeconds <= 0)
                return "Expires: Unknown";

            var expiryDate = DateTimeOffset
                .FromUnixTimeSeconds(inventoryItem.expiresAtUnixSeconds)
                .ToLocalTime()
                .ToString("dd/MM/yyyy");

            return $"Expires: {expiryDate}";
        }


        private void OnCloseInventoryClick(ClickEvent evt)
        {
            Debug.Log("Closing inventory");
            gameObject.SetActive(false);
        }
    }
}
