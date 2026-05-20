using MiniTeam.Core;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 1942 전용 오디오 클립 슬롯 — 실제 재생은 Core.SoundManager에 위임
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmWave1;
        public AudioClip bgmWave2;
        public AudioClip bgmBoss;
        public AudioClip bgmClear;
        public AudioClip bgmGameOver;

        [Header("효과음")]
        public AudioClip sfxPlayerShoot;
        public AudioClip sfxEnemyHit;
        public AudioClip sfxEnemyDie;
        public AudioClip sfxBossHit;
        public AudioClip sfxPlayerHit;

        [Header("보스 파괴 효과음")]
        public AudioClip sfxBossDeathSmall;  // 소형 폭발 반복 (Enemy_Boom)
        public AudioClip sfxBossDeathFinal;  // 최종 대형 폭발 (Mozozozo_Boom)

        [Header("이어하기")]
        public AudioClip sfxContinueCoin;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayBGM(AudioClip clip)                        => SoundManager.Instance?.PlayBGM(clip);
        public void StopBGM()                                      => SoundManager.Instance?.StopBGM();
        public void PlaySFX(AudioClip clip)                        => SoundManager.Instance?.PlaySFX(clip);
        public void PlaySFX(AudioClip clip, float volumeScale)     => SoundManager.Instance?.PlaySFX(clip, volumeScale);
    }
}
