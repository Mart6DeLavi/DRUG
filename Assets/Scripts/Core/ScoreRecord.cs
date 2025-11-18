[System.Serializable]
public class ScoreRecord
{
    public string playerName;
    public int score;
    public float time;
    public string date;

    public ScoreRecord(string name, int score, float time, string date)
    {
        this.playerName = name;
        this.score = score;
        this.time = time;
        this.date = date;
    }
}