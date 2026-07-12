using System.IO;
using Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode.Gameplay
{
    public class PlayerDataSaveManagerTests
    {
        private string saveFilePath;
        private string tmpFilePath;

        [SetUp]
        public void Initialize()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "player_data.json");
            tmpFilePath = saveFilePath + ".tmp";
            CleanUpFiles();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUpFiles();
        }

        public void CleanUpFiles()
        {
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);

            if (File.Exists(tmpFilePath)) File.Delete(tmpFilePath);
        }


        [Test]
        public void SavePlayerDataTest()
        {
            var dataRecord = new PlayerDataRecord("Test1");

            PlayerDataSaveManager.SavePlayerData(dataRecord);

            Assert.IsTrue(File.Exists(saveFilePath), "The file manager created the .json file");
            Assert.IsFalse(File.Exists(tmpFilePath), "The file manager created the .tmp file");
        }


        [Test]
        public void LoadPlayerDataTest()
        {
            var newPlayerDataRecord = new PlayerDataRecord("Test1");
            newPlayerDataRecord.playerTeam = "UoN";
            newPlayerDataRecord.brandsCaptured = 10;

            PlayerDataSaveManager.SavePlayerData(newPlayerDataRecord);

            var loadedPlayerDataRecord = PlayerDataSaveManager.LoadPlayerData();

            Assert.IsNotNull(loadedPlayerDataRecord, "The loaded player data was null");
            Assert.AreEqual("Test1", loadedPlayerDataRecord.playerName);
            Assert.AreEqual("UoN", loadedPlayerDataRecord.playerTeam);
            Assert.AreEqual(10, loadedPlayerDataRecord.brandsCaptured);
        }


        [Test]
        public void LoadPlayerDataNullTest()
        {
            var loadedPlayerDataRecord = PlayerDataSaveManager.LoadPlayerData();

            Assert.IsNull(loadedPlayerDataRecord, "Loaded player data is null");
        }
    }
}