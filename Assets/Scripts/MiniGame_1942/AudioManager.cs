using UnityEngine;

namespace MiniTeam.Shooting1942
{
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

        [Header("볼륨")]
        [Range(0f, 1f)] public float bgmVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop   = true;
            bgmSource.volume = bgmVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop   = false;
            sfxSource.volume = sfxVolume;
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
