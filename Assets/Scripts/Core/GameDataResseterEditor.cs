using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameDataResetter))]
public class GameDataResetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameDataResetter resetter = (GameDataResetter)target;

        if (GUILayout.Button("Reset Player Data"))
        {
            resetter.ResetPlayerData();
        }

        if (GUILayout.Button("Print Current Save Data"))
        {
            resetter.PrintSaveData();
        }

        if (GUILayout.Button("Force Load Fresh Save"))
        {
            resetter.ForceLoadFresh();
        }
    }
}
