using MiniTeam.Core;
using UnityEngine;

public class HubDebugger : MonoBehaviour
{
    private bool _show = true;

    private readonly (string label, string scene)[] _miniGames =
    {
        ("1942 슈팅",    "1942_Shooting"),
        ("스폰지밥",      "AceSponge"),
        ("눈빛 보내기",   "SuSuRunScene"),
        ("테트리스",      "Keroris"),
        ("포켓몬",        "Pokemon"),
        ("배틀",          "Battle"),
    };

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2)) _show = !_show;
    }

    private void OnGUI()
    {
        if (!_show) return;

        var mgm = MiniGameManager.Instance;

        GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 300), GUI.skin.box);

        GUILayout.Label("=== Hub Debugger (F2 토글) ===");
        GUILayout.Label($"currentStage: {(mgm != null ? mgm.currentStage.ToString() : "NULL")}");
        GUILayout.Label($"IsInMiniGame: {(mgm != null ? mgm.IsInMiniGame.ToString() : "NULL")}");

        GUILayout.Space(5);
        GUILayout.Label("[ 미니게임 직행 ]");

        foreach (var (label, scene) in _miniGames)
        {
            if (GUILayout.Button(label))
            {
                if (mgm == null)
                {
                    Debug.LogError("[HubDebugger] MiniGameManager.Instance is null!");
                    continue;
                }
                if (mgm.IsInMiniGame)
                {
                    Debug.LogWarning("[HubDebugger] 이미 미니게임 진행 중입니다.");
                    continue;
                }
                Debug.Log($"[HubDebugger] {scene} 진입");
                mgm.EnterMiniGame(scene);
            }
        }

        GUILayout.EndArea();
    }
}
