using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    public enum MapItemType { PokemonBall, StrangeCandy, Digivice }

    [RequireComponent(typeof(BoxCollider2D))]
    public class MapItem : MonoBehaviour
    {
        [Header("아이템 종류")]
        public MapItemType itemType = MapItemType.PokemonBall;

        private bool picked = false;

        void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (picked || !other.CompareTag("Player")) return;
            picked = true;
            StartCoroutine(PickupRoutine());
        }

IEnumerator PickupRoutine()
        {
            var player = FindAnyObjectByType<PlayerMapController>();
            player?.SetControllable(false);

            string key = GetDialogueKey();
            string msg = DialogueDB.Instance != null
                ? DialogueDB.Instance.Get(key)
                : $"[ {GetItemName()}{GetEulReul(GetItemName())} 획득했다! ]";

            if (MapDialogueUI.Instance != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(msg));
            else
                yield return new WaitForSeconds(2f);

            PokemonGameController.Instance?.CollectItem(itemType);
            player?.SetControllable(true);
            gameObject.SetActive(false);
        }

        string GetItemName() => itemType switch
        {
            MapItemType.PokemonBall  => "포켓몬볼",
            MapItemType.StrangeCandy => "이상한사탕",
            MapItemType.Digivice     => "디지바이스",
            _                        => "아이템",
        };

string GetDialogueKey() => itemType switch
        {
            MapItemType.PokemonBall  => "item_pokemonball",
            MapItemType.StrangeCandy => "item_strangecandy",
            MapItemType.Digivice     => "item_digivice",
            _                        => "item_pokemonball",
        };


        // 마지막 글자 받침 유무로 을/를 판별
        static string GetEulReul(string word)
        {
            if (string.IsNullOrEmpty(word)) return "을";
            int code = word[^1] - 0xAC00;
            if (code < 0 || code > 11171) return "을";
            return (code % 28) == 0 ? "를" : "을";
        }
    }
}
