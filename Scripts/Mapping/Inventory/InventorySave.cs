using System.IO;
using UnityEngine;

namespace Inventory
{
    public static class InventorySave
    {
        /// <summary>
        ///     DefaultSavePath, uses Unity application storage for json storage.
        /// </summary>
        private static string DefaultSavePath => Path.Combine(Application.persistentDataPath, "inventory.json");

        /// <summary>
        ///     Save function saves new/updated inventory data using preliminary tmp file.
        /// </summary>
        /// <param name="data">Inventory data passed in.</param>
        public static void Save(InventoryData data)
        {
            Save(data, DefaultSavePath);
        }

        public static void Save(InventoryData data, string savePath)
        {
            var json = JsonUtility.ToJson(data);

            var tmp = savePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(savePath)) File.Delete(savePath);

            File.Move(tmp, savePath);
        }

        /// <summary>
        ///     Load function loads inventory data from json file in the unity application storage.
        /// </summary>
        /// <returns>InventoryData from json</returns>
        public static InventoryData Load()
        {
            return Load(DefaultSavePath);
        }

        public static InventoryData Load(string savePath)
        {
            if (!File.Exists(savePath)) return new InventoryData();

            var json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<InventoryData>(json) ?? new InventoryData();
        }
    }
}