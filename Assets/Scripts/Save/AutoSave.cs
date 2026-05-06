using System.IO;
using UnityEngine;

public static class AutoSave
{
    private const string SaveFileName = "autosave.json";

    public static string SaveFilePath => System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void Save(WorldSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[AutoSave] World saved to: {SaveFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AutoSave] Failed to save: {ex.Message}");
        }
    }

    public static WorldSaveData Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("[AutoSave] No save file found.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            return JsonUtility.FromJson<WorldSaveData>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AutoSave] Failed to load: {ex.Message}");
            return null;
        }
    }
}
