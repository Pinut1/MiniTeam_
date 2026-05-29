using System;
using System.Collections;
using UnityEngine;
using MiniTeam.Pokemon;

namespace MiniTeam.Shooting1942
{
    public class EntryCutsceneManager : MonoBehaviour
    {
        [Header("DialogueDB")]
        [Tooltip("Resources/Dialogues/{dialogueFile}.json — entry_1, entry_2 ... 순서대로 읽음")]
        public string dialogueFile = "1942";
        [Tooltip("JSON 값 형식: \"화자|대사\". 이 이름과 같은 화자일 때만 위쪽 말풍선 + 등장 연출")]
        public string mojoSpeaker = "모조조조";

        [Header("모조조조 진입")]
        public Sprite mojoSprite;
        public float  mojoEntryX       =  0f;
        public float  mojoSpawnY       =  8f;
        public float  mojoTalkY        =  4f;
        public float  mojoMoveDuration =  0.5f;

        [Header("말풍선")]
        public SpeechBubble bubble;

        private GameObject mojoGO;

        public void Play(Action onComplete) => Play("entry_", true, onComplete);

        public void Play(string keyPrefix, Action onComplete) => Play(keyPrefix, true, onComplete);

        public void Play(string keyPrefix, bool mojoAnimation, Action onComplete)
        {
            Debug.Log($"[EntryCutscene] Play({keyPrefix}) | GO active:{gameObject.activeInHierarchy} | bubble:{bubble != null}");
            DialogueDB.Instance.Load(dialogueFile);
            if (bubble != null && bubble.group != null)
            {
                bubble.group.alpha          = 0f;
                bubble.group.blocksRaycasts = false;
            }
            StartCoroutine(CutsceneRoutine(keyPrefix, mojoAnimation, onComplete));
        }

        IEnumerator CutsceneRoutine(string keyPrefix, bool mojoAnimation, Action onComplete)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) player.enabled = false;

            int index = 1;
            while (true)
            {
                string raw = DialogueDB.Instance.Get($"{keyPrefix}{index}");
                if (string.IsNullOrEmpty(raw) || raw.StartsWith("["))
                    break;

                int sep = raw.IndexOf('|');
                string speaker = sep >= 0 ? raw.Substring(0, sep)  : "";
                string text    = sep >= 0 ? raw.Substring(sep + 1) : raw;
                bool isMojo = speaker == mojoSpeaker;

                if (isMojo && mojoAnimation)
                {
                    EnsureMojo();
                    yield return StartCoroutine(MoveMojo(mojoSpawnY, mojoTalkY));
                }

                yield return StartCoroutine(bubble.FadeIn(speaker, text, isMojo));
                yield return StartCoroutine(WaitForInput());
                yield return StartCoroutine(bubble.FadeOut());

                if (isMojo && mojoAnimation)
                {
                    yield return StartCoroutine(MoveMojo(mojoTalkY, mojoSpawnY));
                    mojoGO.SetActive(false);
                }

                index++;
            }

            if (player != null) player.enabled = true;
            onComplete?.Invoke();
        }

        void EnsureMojo()
        {
            if (mojoGO != null) { mojoGO.SetActive(true); return; }
            mojoGO = new GameObject("Mojo_Cutscene");
            mojoGO.transform.position    = new Vector3(mojoEntryX, mojoSpawnY, 0f);
            mojoGO.transform.localScale  = new Vector3(0.16f, 0.16f, 1f);
            var sr               = mojoGO.AddComponent<SpriteRenderer>();
            sr.sprite            = mojoSprite;
            sr.sortingLayerName  = "Enemy";
            sr.sortingOrder      = 10;
        }

        IEnumerator MoveMojo(float fromY, float toY)
        {
            mojoGO.SetActive(true);
            var from = new Vector3(mojoEntryX, fromY, 0f);
            var to   = new Vector3(mojoEntryX, toY,   0f);
            for (float t = 0; t < mojoMoveDuration; t += Time.deltaTime)
            {
                mojoGO.transform.position = Vector3.Lerp(from, to, t / mojoMoveDuration);
                yield return null;
            }
            mojoGO.transform.position = to;
        }

        IEnumerator WaitForInput()
        {
            yield return null;
            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space));
        }
    }
}
