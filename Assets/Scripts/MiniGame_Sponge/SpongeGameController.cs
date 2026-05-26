using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Sponge
{
    // 담당: 장한나
    public class SpongeGameController : MonoBehaviour, IMiniGame
    {
        private void OnEnable()
        {
            SpongeGameManager.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            SpongeGameManager.OnStateChanged -= HandleStateChanged;
        }

        void HandleStateChanged(SpongeGameState.GameState state)
        {
            if (state == SpongeGameState.GameState.Resolution)
                OnGameClear();
        }

        public void OnGameClear()
        {
            Debug.Log("[Sponge] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            Debug.Log("[Sponge] Game Fail!");
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
