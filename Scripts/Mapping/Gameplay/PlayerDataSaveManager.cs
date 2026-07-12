using System.IO;
using UnityEngine;

namespace Gameplay
{
    public static class PlayerDataSaveManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "player_data.json");


        public static void SavePlayerData(PlayerDataRecord dataRecord)
        {
            var json = JsonUtility.ToJson(dataRecord);

            var tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(SavePath)) File.Delete(SavePath);

            File.Move(tmp, SavePath);
        }

        public static PlayerDataRecord LoadPlayerData()
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                var dataRecord = JsonUtility.FromJson<PlayerDataRecord>(json);
                return dataRecord;
            }

            return null;
        }
    }
}