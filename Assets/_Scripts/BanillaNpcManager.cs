using UnityEngine;

public class BanillaNpcManager : MonoBehaviour
{
    // ★ PlayerLaser에서 찾고 있으니 절대 지우면 안 되는 변수!
    public Sprite heartSprite;

    public CutsceneNpcManager duelManager;

    void Start()
    {
        if (duelManager == null)
        {
            duelManager = FindAnyObjectByType<CutsceneNpcManager>();
        }
    }

    private void OnMouseDown()
    {
        if (duelManager != null)
        {
            // ★ 매니저가 대결 가능한 상태(canStartDuel이 true)일 때만 클릭을 허용합니다.
            // 대결이 이미 진행 중이거나, 완승해서 canStartDuel이 false가 되면 클릭이 아예 씹힙니다.
            if (duelManager.canStartDuel)
            {
                duelManager.StartLaserDuel();
            }
        }
    }
}