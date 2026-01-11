/// <summary>
/// Represents a single score entry in the leaderboard.
/// Contains player name, score value, time played, and date achieved.
/// </summary>
[System.Serializable]
public class ScoreRecord
{
    /// <summary>Player's name or identifier</summary>
    public string playerName;
    
    /// <summary>Score value achieved</summary>
    public int score;
    
    /// <summary>Time played in seconds</summary>
    public float time;
    
    /// <summary>Date when score was achieved</summary>
    public string date;

    /// <summary>
    /// Creates a new score record with the specified values.
    /// </summary>
    /// <param name="name">Player's name</param>
    /// <param name="score">Score value</param>
    /// <param name="time">Time played in seconds</param>
    /// <param name="date">Date string</param>
    public ScoreRecord(string name, int score, float time, string date)
    {
        this.playerName = name;
        this.score = score;
        this.time = time;
        this.date = date;
    }
}