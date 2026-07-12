using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour
    {
        public InventoryData inventory;

        [SerializeField]
        private string savePathOverride;

        [Header("Expiry")]
        [SerializeField]
        private int expiryMonths = 3;

        [Header("Debug")]
        [SerializeField]
        private bool addFakeItemOnBoot;

        public static InventoryManager Instance { get; private set; }

        private string SavePath => string.IsNullOrWhiteSpace(savePathOverride) ? null : savePathOverride;
        private int EffectiveExpiryMonths => Mathf.Max(1, expiryMonths);

        /// <summary>
        ///     Used for loading the inventory when manager is loaded.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialise();
            SeedFakeItemOnBoot();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        ///     Inbuilt unity event, used for cleanly saving the inventory when the app is closed.
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveInventory();
        }

        public event Action<InventoryItem> ItemAdded;
        public event Action<string> ItemRemoved;

        /// <summary>
        ///     Forcefully init the inventory with an existing save or a new one.
        /// </summary>
        public void Initialise()
        {
            inventory = SavePath == null ? InventorySave.Load() : InventorySave.Load(SavePath);
            EnsureInventory();

            if (MigrateLegacyItemsAndRemoveExpired())
                SaveInventory();
        }

        private void SeedFakeItemOnBoot()
        {
            if (!addFakeItemOnBoot || inventory == null) return;

            AddItem("test", "12341234");
        }

        /// <summary>
        ///     Used for setting a custom save path for tests.
        /// </summary>
        /// <param name="path"></param>
        public void SetSavePathForTests(string path)
        {
            savePathOverride = path;
        }

        /// <summary>
        ///     Used for getting the whole inventory for incrementally getting inventory values.
        /// </summary>
        /// <returns>returns the entire inventory.</returns>
        public List<InventoryItem> GetInventory()
        {
            RemoveExpiredItems();
            return inventory.items;
        }

        /// <summary>
        ///     Used for adding a new item to the inventory.
        /// </summary>
        /// <param name="name">the item's name/brand name</param>
        /// <param name="code">the specific brand discount code to add</param>
        public void AddItem(string name, string code)
        {
            EnsureInventory();
            var now = DateTimeOffset.UtcNow;

            var inventoryItem = new InventoryItem(
                name,
                code,
                now.ToUnixTimeSeconds(),
                now.AddMonths(EffectiveExpiryMonths).ToUnixTimeSeconds()
            );

            inventory.items.Add(inventoryItem);
            SaveInventory();

            ItemAdded?.Invoke(inventoryItem);
        }

        /// <summary>
        ///     Used for removing an item from the inventory.
        /// </summary>
        /// <param name="code">the specific brand discount code to remove</param>
        public void RemoveByCode(string code)
        {
            EnsureInventory();

            var removedCount = inventory.items.RemoveAll(i => i.code == code);
            if (removedCount <= 0) return;

            if (SavePath == null) InventorySave.Save(inventory);
            else SaveInventory();

            ItemRemoved?.Invoke(code);
        }

        /// <summary>
        ///     Used for getting an item from the inventory.
        /// </summary>
        /// <param name="code">the specific brand discount code to look for</param>
        /// <returns>InventoryItem with code and name</returns>
        public InventoryItem FindByCode(string code)
        {
            RemoveExpiredItems();
            return inventory.items.Find(i => i.code == code);
        }

        private void EnsureInventory()
        {
            if (inventory == null)
                inventory = new InventoryData();
        }

        private void SaveInventory()
        {
            EnsureInventory();

            if (SavePath == null) InventorySave.Save(inventory);
            else InventorySave.Save(inventory, SavePath);
        }

        private void RemoveExpiredItems()
        {
            if (MigrateLegacyItemsAndRemoveExpired())
                SaveInventory();
        }

        private bool MigrateLegacyItemsAndRemoveExpired()
        {
            EnsureInventory();

            var now = DateTimeOffset.UtcNow;
            var changed = false;

            foreach (var item in inventory.items)
            {
                if (item == null) continue;

                if (item.capturedAtUnixSeconds <= 0 || item.expiresAtUnixSeconds <= 0)
                {
                    item.capturedAtUnixSeconds = now.ToUnixTimeSeconds();
                    item.expiresAtUnixSeconds = now.AddMonths(EffectiveExpiryMonths).ToUnixTimeSeconds();
                    changed = true;
                }
            }

            if (inventory.items.RemoveAll(i => i == null || i.IsExpired(now)) > 0)
                changed = true;

            return changed;
        }
    }
}
