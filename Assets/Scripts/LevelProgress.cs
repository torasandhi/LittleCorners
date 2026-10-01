using UnityEngine;

public static class LevelProgress
{
    private const string ClearedLevelKeyPrefix = "ClearedLevel_";

    public static void MarkCleared(int levelBuildIndex)
    {
        PlayerPrefs.SetInt(GetKey(levelBuildIndex), 1);
        PlayerPrefs.Save();
    }

    public static bool IsCleared(int levelBuildIndex)
    {
        return PlayerPrefs.GetInt(GetKey(levelBuildIndex), 0) == 1;
    }

    private static string GetKey(int levelBuildIndex)
    {
        return ClearedLevelKeyPrefix + levelBuildIndex;
    }
}
