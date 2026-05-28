#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using MiniTeam.Shooting1942;

public static class EntryCutsceneSetup
{
    [MenuItem("MiniTeam/1942/Wire EntryCutscene References")]
    public static void WireReferences()
    {
        var ec = Object.FindAnyObjectByType<EntryCutsceneManager>(FindObjectsInactive.Include);
        if (ec == null) { Debug.LogError("[Setup] EntryCutsceneManager not found"); return; }
        var box = ec.gameObject;   // Dialogue_Box

        // 말풍선 배경 Image
        var image = box.transform.Find("Image");
        if (image == null) { Debug.LogError("[Setup] Dialogue_Box/Image not found"); return; }

        // 대사 TMP (Image/Text (TMP))
        var dialogueTMP = image.GetComponentInChildren<TextMeshProUGUI>(true);
        if (dialogueTMP == null) { Debug.LogError("[Setup] dialogue TMP not found under Image"); return; }

        // 화자 라벨 TMP — 없으면 Dialogue_Box 직접 자식으로 생성 (Image flip 영향 제외)
        var speakerTr = box.transform.Find("SpeakerLabel");
        TextMeshProUGUI speakerTMP;
        if (speakerTr == null)
        {
            var go = new GameObject("SpeakerLabel", typeof(RectTransform));
            go.transform.SetParent(box.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 4f);
            rt.sizeDelta        = new Vector2(0f, 28f);
            speakerTMP           = go.AddComponent<TextMeshProUGUI>();
            speakerTMP.fontSize  = 18f;
            speakerTMP.fontStyle = FontStyles.Bold;
            speakerTMP.alignment = TextAlignmentOptions.Left;
            speakerTMP.color     = new Color(0.15f, 0.15f, 0.7f);
        }
        else
        {
            // 기존에 Image 아래에 있으면 Dialogue_Box로 이동
            if (speakerTr.parent != box.transform)
                speakerTr.SetParent(box.transform, false);
            speakerTMP = speakerTr.GetComponent<TextMeshProUGUI>();
        }

        // CanvasGroup
        var cg = box.GetComponent<CanvasGroup>();
        if (cg == null) cg = box.AddComponent<CanvasGroup>();

        // SpeechBubble
        var bubble = box.GetComponent<SpeechBubble>();
        if (bubble == null) bubble = box.AddComponent<SpeechBubble>();
        bubble.group         = cg;
        bubble.speakerLabel  = speakerTMP;
        bubble.dialogueLabel = dialogueTMP;

        // EntryCutsceneManager
        ec.bubble     = bubble;
        ec.mojoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Sprites/MiniGame_1942/Enemy/Mozozozo.png");

        var ctrl = Object.FindAnyObjectByType<ShootingGameController>();
        if (ctrl != null) ctrl.entryCutscene = ec;

        EditorUtility.SetDirty(box);
        if (ctrl != null) EditorUtility.SetDirty(ctrl.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(box.scene);

        Debug.Log($"[Setup] Done | group:{cg!=null} image:{image!=null} speaker:{speakerTMP!=null} dialogue:{dialogueTMP!=null} bubble:{ec.bubble!=null} mojo:{ec.mojoSprite!=null} ctrl:{ctrl?.entryCutscene!=null}");
    }
}
#endif
