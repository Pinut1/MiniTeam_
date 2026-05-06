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
        public AudioClip sfxEnemyDie;
        public AudioClip sfxBossHit;
        public AudioClip sfxPlayerHit;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void PlayBGM(AudioClip clip) => SoundManager.Instance?.PlayBGM(clip);
        public void StopBGM()              => SoundManager.Instance?.StopBGM();
        public void PlaySFX(AudioClip clip) => SoundManager.Instance?.PlaySFX(clip);
    }
}
