using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 타운스빌 배경 무한 스크롤
    // Inspector: bg1, bg2에 같은 배경 스프라이트 오브젝트 연결
    // bg2는 bg1 바로 위에 배치 (y = bg1.y + bgHeight)
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("배경 오브젝트 2개 (같은 스프라이트)")]
        public Transform bg1;
        public Transform bg2;

        [Header("스크롤 속도")]
        public float scrollSpeed = 2f;

        private float bgHeight;

        void Start()
        {
            // SpriteRenderer 크기로 배경 높이 계산
            var sr = bg1.GetComponent<SpriteRenderer>();
            bgHeight = sr != null ? sr.bounds.size.y : 10f;
        }

        void Update()
        {
            float delta = scrollSpeed * Time.deltaTime;
            bg1.position += Vector3.down * delta;
            bg2.position += Vector3.down * delta;

            // 화면 아래로 벗어나면 다른 배경 위로 이동
            if (bg1.position.y < -bgHeight)
                bg1.position = new Vector3(bg1.position.x, bg2.position.y + bgHeight, bg1.position.z);

            if (bg2.position.y < -bgHeight)
                bg2.position = new Vector3(bg2.position.x, bg1.position.y + bgHeight, bg2.position.z);
        }
    }
}
