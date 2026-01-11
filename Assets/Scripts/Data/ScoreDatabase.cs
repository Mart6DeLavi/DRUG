using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages high score persistence using JSON file storage.
/// Maintains a leaderboard of top 10 scores sorted in descending order.
/// </summary>
public static class ScoreDatabase
{
    private static string filePath => Path.Combine(Application.persistentDataPath, "scoreboard.json");

    /// <summary>
    /// Loads all saved scores from disk.
    /// Returns an empty list if no save file exists.
    /// </summary>
    public static List<ScoreRecord> LoadScores()
    {
        if (!File.Exists(filePath))
            return new List<ScoreRecord>();

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<ScoreList>(json)?.scores ?? new List<ScoreRecord>();
    }

    /// <summary>
    /// Saves the score list to disk as JSON.
    /// </summary>
    /// <param name="scores">List of score records to save</param>
    public static void SaveScores(List<ScoreRecord> scores)
    {
        var wrapper = new ScoreList { scores = scores };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Adds a new score to the leaderboard.
    /// Automatically sorts scores in descending order and keeps only top 10.
    /// </summary>
    /// <param name="record">The score record to add</param>
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
    /// Deletes all saved scores from disk.
    /// Use this to reset the leaderboard.
    /// </summary>
    public static void ClearAllScores()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[ScoreDatabase] All scores cleared.");
        }
    }

    /// <summary>
    /// Internal wrapper class for JSON serialization of score lists.
    /// </summary>
    [System.Serializable]
    private class ScoreList
    {
        public List<ScoreRecord> scores;
    }
}