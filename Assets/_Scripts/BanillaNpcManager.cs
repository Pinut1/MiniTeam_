using UnityEngine;

public class BanillaNpcManager : MonoBehaviour
{
    // ★ PlayerLaser에서 찾고 있으니 절대 지우면 안 되는 변수!
    public Sprite heartSprite;

    // 듀얼 매니저 (이름이 다르면 실제 사용하시는 스크립트 이름으로 바꿔주세요)
    public CutsceneNpcManager duelManager;

    void Start()
    {
        // 씬 시작 시 매니저를 알아서 찾아서 꽂아줍니다 (프리팹 연결 안 됨 방지)
        if (duelManager == null)
        {
            duelManager = FindAnyObjectByType<CutsceneNpcManager>();
        }
    }

    private void OnMouseDown()
    {
        // 클릭하면 복잡한 짓 하지 말고 그냥 듀얼 매니저한테 대결 시작하라고 토스!
        if (duelManager != null)
        {
            duelManager.StartLaserDuel();
        }
    }
}