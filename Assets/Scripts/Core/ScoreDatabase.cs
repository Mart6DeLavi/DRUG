using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ScoreDatabase
{
    private static string filePath => Path.Combine(Application.persistentDataPath, "scoreboard.json");

    public static List<ScoreRecord> LoadScores()
    {
        if (!File.Exists(filePath))
            return new List<ScoreRecord>();

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<ScoreList>(json)?.scores ?? new List<ScoreRecord>();
    }

    public static void SaveScores(List<ScoreRecord> scores)
    {
        var wrapper = new ScoreList { scores = scores };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json);
    }

    public static void AddScore(ScoreRecord record)
    {
        var list = LoadScores();
        list.Add(record);

        list.Sort((a, b) => b.score.CompareTo(a.score));

        if (list.Count > 10)
            list.RemoveRange(10, list.Count - 10);

        SaveScores(list);
    }

    /// <summary>
    /// Delete all saved scores
    /// </summary>
    public static void ClearAllScores()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[ScoreDatabase] All scores cleared.");
        }
    }

    [System.Serializable]
    private class ScoreList
    {
        public List<ScoreRecord> scores;
    }
}