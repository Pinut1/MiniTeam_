using System;

namespace MiniTeam.Core
{
    [Serializable]
    public class GameProgressData
    {
        public int currentStage = 0;
        public bool isCutscenePlayed = false;
        public bool isLastGameCleared = false;

        public void Reset()
        {
            currentStage = 0;
            isCutscenePlayed = false;
            isLastGameCleared = false;
        }
    }
}
