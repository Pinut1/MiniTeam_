using UnityEngine;
using UnityEngine.UI;

public class BanillaNpcManager : MonoBehaviour
{
    [Header("레이저 정밀 조준 설정 (여NPC와 동일)")]
    [SerializeField] private float eyeOffset = 1.35f;         // 눈 높이
    [SerializeField] private float eyeForwardOffset = 0.1f;   // 눈 앞쪽 오프셋
    [SerializeField] private float laserScaleFactor = 1f;     // 스프라이트 배율
    [SerializeField] private float laserThickness = 0.2f;     // 레이저 굵기

    [Header("대결 게이지 설정")]
    public Transform playerTransform; // 플레이어 위치 (외부 매니저에서 할당)

    [Range(0f, 1f)]
    public float clashGauge = 0.5f;

    [Header("UI 설정")]
    public Image gaugeImage; // 화면에 띄울 게이지 바 이미지

    public Sprite heartSprite;

    [Header("연타 설정")]
    public float vanillaPushSpeed = 0.3f; // 가만히 있을 때 바닐라가 밀어붙이는 속도 (초당)
    public float playerClickPower = 0.05f; // 스페이스바 1번 누를 때 차오르는 게이지 양

    private Animator anim;
    private GameObject vanillaLaser;
    private SpriteRenderer laserSR;
    private bool isClashing = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        Transform laserTransform = transform.Find("Laser_Yellow_0");
        if (laserTransform != null)
        {
            vanillaLaser = laserTransform.gameObject;
            laserSR = vanillaLaser.GetComponent<SpriteRenderer>();
            vanillaLaser.SetActive(false);
        }

        // 게임 시작 시 하이어라키에 있는 기존 게이지 UI를 자동으로 찾아 연결합니다.
        if (gaugeImage == null)
        {
            // 스크린샷의 하이어라키 경로(Canvas -> Gage -> Image)를 그대로 추적해서 찾습니다.
            GameObject findGauge = GameObject.Find("Canvas/Gage");
            if (findGauge != null)
            {
                gaugeImage = findGauge.GetComponent<UnityEngine.UI.Image>();
            }
            else
            {
                Debug.LogWarning("기존 게이지 UI를 찾을 수 없습니다. 하이어라키 이름을 확인해주세요.");
            }
        }
    }

    void Update() // 연타 입력을 받기 위해 Update 추가
    {
        if (isClashing)
        {
            // 1. 바닐라의 지속적인 공격 (시간이 지날수록 게이지가 0 쪽으로 깎임)
            clashGauge -= vanillaPushSpeed * Time.deltaTime;

            // 2. 플레이어의 연타 방어/반격 (스페이스바를 누를 때마다 게이지가 1 쪽으로 오름)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                clashGauge += playerClickPower;
            }

            // 3. 게이지가 0 미만, 1 초과로 넘어가지 않게 고정
            clashGauge = Mathf.Clamp(clashGauge, 0f, 1f);

            // 4. UI 게이지 이미지 업데이트 (화면의 막대바가 실시간으로 변함)
            if (gaugeImage != null)
            {
                gaugeImage.fillAmount = clashGauge;
            }

            // 5. 승패 판정
            if (clashGauge <= 0f)
            {
                WinClash(); // 바닐라 승리 (플레이어 패배)
            }
            else if (clashGauge >= 1f)
            {
                LoseClash(); // 바닐라 패배 (플레이어 승리)
            }
        }
    }


    void LateUpdate()
    {
        // 대결 중일 때만 레이저 길이와 위치를 실시간 업데이트
        if (isClashing && vanillaLaser != null && playerTransform != null)
        {
            float facingDirection = Mathf.Sign(transform.localScale.x);

            // 1. 발사점(바닐라 눈) 계산 (GirlNpcReaction과 완벽히 동일한 방식)
            Vector3 firePoint = transform.position + new Vector3(eyeForwardOffset * facingDirection, eyeOffset, 0);

            // 2. 레이저 시작 위치를 눈으로 고정
            vanillaLaser.transform.position = firePoint;

            // 3. 플레이어의 충돌 기준점 (높이는 바닐라의 눈높이와 맞춰서 일직선으로 연출)
            Vector3 playerPoint = playerTransform.position;
            playerPoint.y = firePoint.y;

            // 4. 플레이어와 바닐라 사이의 충돌 지점 계산 (게이지 비율에 따라 중간 지점이 이동)
            // clashGauge가 0이면 플레이어에게 닿음, 1이면 바닐라 눈앞까지 밀림
            Vector3 clashPoint = Vector3.Lerp(playerPoint, firePoint, clashGauge);

            // 5. 방향 설정 (충돌 지점을 향하도록)
            Vector3 dir = clashPoint - firePoint;
            vanillaLaser.transform.right = dir;

            // 6. 완벽한 길이 적용 (GirlNpcReaction과 완벽히 동일한 공식)
            if (laserSR != null && laserSR.sprite != null)
            {
                float currentDist = dir.magnitude;
                float baseWidth = laserSR.sprite.bounds.size.x;
                float parentScaleX = Mathf.Abs(transform.localScale.x);

                float exactScaleX = (currentDist / baseWidth) / parentScaleX;

                if (transform.localScale.x < 0)
                {
                    exactScaleX *= -1f;
                }

                exactScaleX *= laserScaleFactor;
                vanillaLaser.transform.localScale = new Vector3(exactScaleX, laserThickness, 1f);
            }
        }
    }

    // ★ 대결 시작 (다른 게임/연타 매니저 스크립트에서 호출)
    public void StartClash(Transform playerTarget)
    {
        playerTransform = playerTarget;
        clashGauge = 0.5f; // 정중앙에서 시작
        isClashing = true;

        if (vanillaLaser != null) vanillaLaser.SetActive(true);
        if (anim != null) anim.SetBool("IsCrying", false);
    }

    // ★ 바닐라 승리 = 플레이어 패배
    public void WinClash()
    {
        isClashing = false;
        if (vanillaLaser != null) vanillaLaser.SetActive(false);

        // 바닐라가 이겼을 때 울음(Crying) 애니메이션
        if (anim != null) anim.SetBool("IsCrying", true);
    }

    // ★ 바닐라 패배 = 플레이어 승리
    public void LoseClash()
    {
        isClashing = false;
        if (vanillaLaser != null) vanillaLaser.SetActive(false);

        // 바닐라가 졌을 때 웃음(Smile) 애니메이션으로 엔딩
        if (anim != null)
        {
            anim.SetBool("IsCrying", false);
            anim.SetTrigger("ToSmile");
        }
    }
}