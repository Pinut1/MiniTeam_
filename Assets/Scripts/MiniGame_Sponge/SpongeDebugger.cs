using MiniTeam.Core;
using UnityEngine;
using static SpongeGameState;

namespace MiniTeam.Sponge
{
    public class SpongeDebugger : MonoBehaviour
    {
        private bool _show = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) _show = !_show;
        }

        private void OnGUI()
        {
            if (!_show) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 300), GUI.skin.box);

            GUILayout.Label("=== Sponge Debugger (F1 토글) ===");

            var sgm = SpongeGameManager.Instance;
            var mgm = MiniGameManager.Instance;

            GUILayout.Label($"SpongeGameManager: {(sgm != null ? "OK" : "NULL")}");
            GUILayout.Label($"CurrentState: {(sgm != null ? sgm.CurrentState.ToString() : "-")}");
            GUILayout.Label($"ConditionsMet: {(sgm != null ? sgm.IsAllConditionsMet().ToString() : "-")}");

            GUILayout.Space(5);
            GUILayout.Label($"MiniGameManager: {(mgm != null ? "OK" : "NULL")}");
            GUILayout.Label($"currentStage: {(mgm != null ? mgm.progressData.currentStage.ToString() : "-")}");

            GUILayout.Space(10);

            if (GUILayout.Button("Force GameClear → Hub"))
            {
                if (mgm != null)
                    mgm.OnMiniGameClear();
                else
                    Debug.LogError("[SpongeDebugger] MiniGameManager.Instance is null!");
            }

            if (GUILayout.Button("Force State: Resolution"))
            {
                if (sgm != null)
                    sgm.ChangeState(GameState.Resolution);
                else
                    Debug.LogError("[SpongeDebugger] SpongeGameManager.Instance is null!");
            }

            if (GUILayout.Button("Skip All Conditions"))
            {
                if (sgm != null)
                    sgm.SkipAllRequiredConditions();
            }

            GUILayout.EndArea();
        }
    }
}
