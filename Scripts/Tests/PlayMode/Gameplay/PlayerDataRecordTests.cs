using System.IO;
using Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode.Gameplay
{
    public class PlayerDataRecordTests
    {
        [TearDown]
        public void CleanUp()
        {
            var filePath = Path.Combine(Application.persistentDataPath, "player_data.json");

            if (File.Exists(filePath)) File.Delete(filePath);
        }

        [Test]
        public void PlayerDataConstructorTest()
        {
            var expectedName = "Test1";

            var dataRecord = new PlayerDataRecord(expectedName);

            Assert.AreEqual(expectedName, dataRecord.playerName);
            Assert.AreEqual("", dataRecord.playerTeam);
            Assert.AreEqual(0, dataRecord.brandsCaptured);
        }


        [Test]
        public void IncrementBrandCapturedTest()
        {
            var dataRecord = new PlayerDataRecord("Test1");

            dataRecord.IncrementBrandsCaptured();

            Assert.AreEqual(1, dataRecord.GetBrandsCaptured());
        }


        [Test]
        public void GetBrandCapturedTest()
        {
            var dataRecord = new PlayerDataRecord("Test1");

            dataRecord.brandsCaptured = 100;

            Assert.AreEqual(100, dataRecord.GetBrandsCaptured());
        }


        [Test]
        public void GetPlayerTeamTest()
        {
            var dataRecord = new PlayerDataRecord("Test1");

            dataRecord.playerTeam = "UoN";

            Assert.AreEqual("UoN", dataRecord.GetPlayerTeam());
        }
    }
}