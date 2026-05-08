using System;
using System.IO;
using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public static class CombatAutoSave
    {
        private const string SaveFileName = "combat_autosave.json";

        public static string SaveFilePath
        {
            get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
        }

        public static void Save(CombatSaveData data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log("[CombatAutoSave] Combat state saved to: " + SaveFilePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[CombatAutoSave] Failed to save: " + exception.Message);
            }
        }

        public static CombatSaveData Load()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    return null;
                }

                var json = File.ReadAllText(SaveFilePath);
                return JsonUtility.FromJson<CombatSaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError("[CombatAutoSave] Failed to load: " + exception.Message);
                return null;
            }
        }
    }
}
