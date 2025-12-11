using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SavingSystem
{
    public class SaveLoadManager : MonoBehaviour
    {
        private static string SavePath => Application.persistentDataPath + "/gameData.json";

        public static void SaveData()
        {
            var data = new GameData();

            // Buscar todos los objetos que implementen ISavable
            foreach (var savable in GetSaveDataProviders())
            {
                savable?.Save(ref data);
            }

            var json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(SavePath, json);

            Debug.Log($"Datos guardados en: {SavePath}");
        }

        private static IEnumerable<ISavable> GetSaveDataProviders()
        {
            var savables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                .Where(m => typeof(ISavable).IsAssignableFrom(m.GetType()))
                .Select(m => m as ISavable);
            return savables;
        }

        public static void LoadData()
        {
            if (!System.IO.File.Exists(SavePath))
            {
                Debug.LogError("No hay archivo de guardado en: " + SavePath);
                return;
            }

            var json = System.IO.File.ReadAllText(SavePath);
            var gameData = JsonUtility.FromJson<GameData>(json);

            if (gameData is null)
            {
                Debug.LogError("Error al leer datos del archivo");
                return;
            }

            // Cargar datos en todos los objetos ISavable
            foreach (var savable in GetSaveDataProviders())
            {
                savable?.Load(ref gameData);
            }

            Debug.Log($"Datos cargados desde: {SavePath}");
        }

        public static bool HasSaveData()
        {
            return System.IO.File.Exists(SavePath);
        }

        public static void DeleteSaveData()
        {
            if (System.IO.File.Exists(SavePath))
            {
                System.IO.File.Delete(SavePath);
                Debug.Log("Datos de guardado eliminados");
            }
        }
    }
}