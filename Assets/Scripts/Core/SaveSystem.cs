using UnityEngine;

namespace MiniTeam.Core
{
    public static class SaveSystem
    {
        private const string SAVE_STAGE_KEY = "SavedCurrentStage";
        private const string SAVE_CUTSCENE_KEY = "SavedCutscenePlayed";

        public static void Save(GameProgressData data)
        {
            PlayerPrefs.SetInt(SAVE_STAGE_KEY, data.currentStage);
            PlayerPrefs.SetInt(SAVE_CUTSCENE_KEY, data.isCutscenePlayed ? 1 : 0);
            // isLastGameCleared is usually session-based, but we can serialize it if needed later.
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Game Saved. Current Stage: {data.currentStage}, Cutscene Played: {data.isCutscenePlayed}");
        }

        public static GameProgressData Load()
        {
            GameProgressData data = new GameProgressData();
            data.currentStage = PlayerPrefs.GetInt(SAVE_STAGE_KEY, 0);
            data.isCutscenePlayed = PlayerPrefs.GetInt(SAVE_CUTSCENE_KEY, 0) == 1;
            data.isLastGameCleared = false; // Reset session-based data
            Debug.Log($"[SaveSystem] Game Loaded. Current Stage: {data.currentStage}, Cutscene Played: {data.isCutscenePlayed}");
            return data;
        }

        public static void ResetSaveData()
        {
            PlayerPrefs.DeleteKey(SAVE_STAGE_KEY);
            PlayerPrefs.DeleteKey(SAVE_CUTSCENE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[SaveSystem] Save Data Reset.");
        }
    }
}
