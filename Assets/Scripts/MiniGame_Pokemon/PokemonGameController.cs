using UnityEngine;
using UnityEngine.SceneManagement;
using MiniTeam.Core;

namespace MiniTeam.Pokemon
{
    public class PokemonGameController : MonoBehaviour, IMiniGame
    {
        public static PokemonGameController Instance { get; private set; }

        [Header("리스폰 위치")]
        public Transform spawnPoint;

        public bool HasItem            { get; private set; } = false;
        public bool IsPokemonEventDone { get; private set; } = false;

        private PlayerMapController player;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            SceneManager.SetActiveScene(gameObject.scene);
            player = FindAnyObjectByType<PlayerMapController>();
        }

        public void RespawnPlayer()
        {
            if (player == null || spawnPoint == null) return;
            player.transform.position = spawnPoint.position;
        }

        public void GiveItem()
        {
            HasItem = true;
        }

        public void SetPokemonEventDone()
        {
            IsPokemonEventDone = true;
        }

        public void OnGameClear()
        {
            if (MiniGameManager.Instance != null)
                MiniGameManager.Instance.OnMiniGameClear();
            else
                Debug.Log("[Pokemon] Game Clear! (단독 테스트)");
        }

        public void OnGameFail()
        {
            if (MiniGameManager.Instance != null)
                MiniGameManager.Instance.OnMiniGameFail();
            else
                Debug.Log("[Pokemon] Game Fail! (단독 테스트)");
        }
    }
}
