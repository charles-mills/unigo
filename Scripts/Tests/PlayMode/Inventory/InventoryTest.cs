using System.Collections;
using System.IO;
using System;
using Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Inventory
{
    public class InventoryTest
    {
        private GameObject gameObject;
        private InventoryManager manager;
        private string testPath;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            testPath = Path.Combine(Application.persistentDataPath, "inventoryTest.json");

            if (File.Exists(testPath)) File.Delete(testPath);

            gameObject = new GameObject("inventoryTestObj");
            manager = gameObject.AddComponent<InventoryManager>();

            manager.SetSavePathForTests(testPath);
            manager.Initialise();

            manager.inventory = new InventoryData();
            InventorySave.Save(manager.inventory, testPath);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (gameObject != null) UnityEngine.Object.Destroy(gameObject);

            if (File.Exists(testPath)) File.Delete(testPath);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddItemsTest()
        {
            manager.AddItem("test1", "123");
            manager.AddItem("test2", "456");
            manager.AddItem("test3", "789");

            Assert.AreEqual(3, manager.GetInventory().Count, $"Count was {manager.GetInventory().Count}");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveItemsTest()
        {
            manager.AddItem("test1", "123");
            manager.AddItem("test2", "456");
            manager.AddItem("test3", "789");

            InventorySave.Save(manager.inventory, testPath);
            Assert.True(File.Exists(testPath));

            yield return null;
        }

        [UnityTest]
        public IEnumerator FileLoadTest()
        {
            manager.AddItem("test1", "123");
            manager.AddItem("test2", "456");
            manager.AddItem("test3", "789");

            InventorySave.Save(manager.inventory, testPath);

            var data = InventorySave.Load(testPath);
            Assert.AreEqual(3, data.items.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveItemsTest()
        {
            manager.AddItem("test1", "123");
            manager.AddItem("test2", "456");
            manager.AddItem("test3", "789");
            InventorySave.Save(manager.inventory, testPath);

            manager.inventory = InventorySave.Load(testPath);
            manager.RemoveByCode("123");
            manager.RemoveByCode("456");
            manager.RemoveByCode("789");

            Assert.AreEqual(0, manager.inventory.items.Count);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddItemSetsExpiryTimestamps()
        {
            manager.AddItem("test1", "123");

            var item = manager.FindByCode("123");

            Assert.NotNull(item);
            Assert.Greater(item.capturedAtUnixSeconds, 0);
            Assert.Greater(item.expiresAtUnixSeconds, item.capturedAtUnixSeconds);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InitialiseRemovesExpiredItems()
        {
            var data = new InventoryData();
            data.items.Add(
                new InventoryItem(
                    "expired",
                    "123",
                    DateTimeOffset.UtcNow.AddMonths(-4).ToUnixTimeSeconds(),
                    DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds()
                )
            );

            InventorySave.Save(data, testPath);

            manager.Initialise();

            Assert.AreEqual(0, manager.GetInventory().Count);
            Assert.IsNull(manager.FindByCode("123"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator InitialiseMigratesLegacyItemsWithoutExpiry()
        {
            var data = new InventoryData();
            data.items.Add(new InventoryItem("legacy", "123", 0, 0));

            InventorySave.Save(data, testPath);

            manager.Initialise();

            var item = manager.FindByCode("123");
            Assert.NotNull(item);
            Assert.Greater(item.capturedAtUnixSeconds, 0);
            Assert.Greater(item.expiresAtUnixSeconds, item.capturedAtUnixSeconds);
            yield return null;
        }
    }
}
