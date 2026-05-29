using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    public class CaveGoalTrigger : MonoBehaviour
    {
        [Header("대사 키")]
        public string blockedDialogueKey = "cave_blocked";
        public string clearDialogueKey   = "cave_clear";

        private bool isCleared = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (PokemonGameController.Instance == null) return;
            if (isCleared) return;

            if (PokemonGameController.Instance.IsPokemonEventDone)
                StartCoroutine(ClearRoutine());
            else
                StartCoroutine(ShowBlocked());
        }

        IEnumerator ClearRoutine()
        {
            isCleared = true;
            if (MapDialogueUI.Instance != null)
            {
                string text = DialogueDB.Instance != null
                    ? DialogueDB.Instance.Get(clearDialogueKey)
                    : "아구몬과 함께 동굴로 들어갔다!";
                yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
            }
            PokemonGameController.Instance?.OnGameClear();
        }

        IEnumerator ShowBlocked()
        {
            if (MapDialogueUI.Instance == null) yield break;
            string text = DialogueDB.Instance != null
                ? DialogueDB.Instance.Get(blockedDialogueKey)
                : "동굴로 들어갈 수 없다!";
            yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
        }
    }
}
